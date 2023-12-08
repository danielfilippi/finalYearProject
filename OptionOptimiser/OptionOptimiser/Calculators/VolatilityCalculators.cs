using Newtonsoft.Json.Linq;
using OptionOptimiser.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OptionOptimiser.Calculators
{
    internal class VolatilityCalculators
    {

        //IMPLIED VOLATILITY

        public static double CalculateImpliedVolatility(double initialVola, double TheoreticalValue, double Spot, double Strike, double TimeToMaturity, double RiskFreeRate, char PutCall, char EuroAme, Stock underlying, int Steps)
        {
            double sigma = initialVola; // Start with an initial guess for implied volatility
            double sigmaPrev = 0;
            double tolerance = 1e-5;
            int maxIterations = 100;
            int iterations = 0;

            while (Math.Abs(sigma - sigmaPrev) > tolerance && iterations < maxIterations)
            {
                sigmaPrev = sigma;
                double binomialPrice = BinomialCalculators.BinomialWithDividends(Steps, Spot, Spot, RiskFreeRate, sigma, TimeToMaturity, PutCall, EuroAme, underlying);

                // Approximate Vega using finite difference
                double sigmaUp = sigma + 0.5;
                double priceUp = BinomialCalculators.BinomialWithDividends(Steps, Spot, Spot, RiskFreeRate, sigmaUp, TimeToMaturity, PutCall, EuroAme, underlying);
                double vega = (priceUp - binomialPrice) / 0.01;

                sigma -= (binomialPrice - TheoreticalValue) / vega; //Newton-Raphson iteration
                iterations++;
            }

            return sigma;
        }

        public static async Task<List<List<KeyValuePair<int, double>>>> GetVolatilityData(string Symbol)

        {
            List<KeyValuePair<int, double>> volatilityData = new List<KeyValuePair<int, double>>();
            var client = new HttpClient();

            string uriFortMinutely = $"https://yahoo-finance127.p.rapidapi.com/historic/{Symbol}/2m/36d";
            var closingPricesFortMinutely = await FetchClosingPricesWithTimestamps(client, uriFortMinutely, "2m");

            //daily rolling vola for up to 35 days ago
            string uriQuarterHourly = $"https://yahoo-finance127.p.rapidapi.com/historic/{Symbol}/15m/36d";
            var closingPricesQuarterHourly = await FetchClosingPricesWithTimestamps(client, uriQuarterHourly, "15m");

            //weekly rolling vola for up to 26 weeks (add one week to calc last 7 days)
            string uriHourly = $"https://yahoo-finance127.p.rapidapi.com/historic/{Symbol}/1h/189d";
            var closingPricesHourly = await FetchClosingPricesWithTimestamps(client, uriHourly, "1h");

            //yearly/monthly rolling vola for up to 4 years ago (add one year to calc last 365 days)
            string uriDaily = $"https://yahoo-finance127.p.rapidapi.com/historic/{Symbol}/1d/1826d";
            var closingPricesDaily = await FetchClosingPricesWithTimestamps(client, uriDaily, "1d");

            List<KeyValuePair<int, double>> dailyRollingVolatility = BuildDailyRollingVolatilityList(closingPricesQuarterHourly);
            List<KeyValuePair<int, double>> weeklyRollingVolatility = BuildWeeklyRollingVolatilityList(closingPricesHourly);
            List<KeyValuePair<int, double>> monthlyRollingVolatility = BuildMonthlyRollingVolatilityList(closingPricesDaily);
            List<KeyValuePair<int, double>> yearlyRollingVolatility = BuildYearlyRollingVolatilityList(closingPricesDaily);

            List<List<KeyValuePair<int, double>>> allRollingVolatilities = new List<List<KeyValuePair<int, double>>>
            {
                dailyRollingVolatility,
                weeklyRollingVolatility,
                monthlyRollingVolatility,
                yearlyRollingVolatility
            };
            return allRollingVolatilities;
        }
        /*private static List<double> GetDataFromDayToPresent(List<KeyValuePair<long, double>> data, int daysAgo)
        {
            var startDate = DateTimeOffset.UtcNow.AddDays(-daysAgo).Date;
            return data
                .Where(kvp => DateTimeOffset.FromUnixTimeSeconds(kvp.Key).Date >= startDate)
                .Select(kvp => kvp.Value)
                .ToList();
        }*/
        private static List<double> GetDataForRollingWindow(List<KeyValuePair<long, double>> data, int day, double RollingWindowSize)
        {
            var endDate = DateTimeOffset.UtcNow.AddDays(-day).Date;
            var startDate = endDate.AddDays(-RollingWindowSize);

            return data
                .Where(kvp =>
                    DateTimeOffset.FromUnixTimeSeconds(kvp.Key).Date >= startDate &&
                    DateTimeOffset.FromUnixTimeSeconds(kvp.Key).Date <= endDate)
                .Select(kvp => kvp.Value)
                .ToList();
        }
        private static List<KeyValuePair<int, double>> BuildDailyRollingVolatilityList(List<KeyValuePair<long, double>> quarterHourlyData)
        {
            var dailyVolatilityList = new List<KeyValuePair<int, double>>();

            //daily rolling volatility
            for (int day = 0; day <= 35; day++) //daily volatiliry
            {
                var dataFromDayToPresent = GetDataForRollingWindow(quarterHourlyData, day, 1);
                double volatility = CalculateVolatility(dataFromDayToPresent, "1h");
                dailyVolatilityList.Add(new KeyValuePair<int, double>(day, volatility));
            }
            return dailyVolatilityList;
        }
        private static List<KeyValuePair<int, double>> BuildWeeklyRollingVolatilityList(List<KeyValuePair<long, double>> hourlyData)
        {
            var weeklyVolatilityList = new List<KeyValuePair<int, double>>();

            //weekly rolling volatility               
            for (int day = 0; day <= 90; day++)
            {
                var dataFromDayToPresent = GetDataForRollingWindow(hourlyData, day, 7);
                double volatility = CalculateVolatility(dataFromDayToPresent, "1h");
                weeklyVolatilityList.Add(new KeyValuePair<int, double>(day, volatility));
            }
            return weeklyVolatilityList;
        }
        private static List<KeyValuePair<int, double>> BuildMonthlyRollingVolatilityList(List<KeyValuePair<long, double>> dailyData)
        {
            var monthlyVolatilityList = new List<KeyValuePair<int, double>>();

            for (int day = 0; day <= 1461; day++)
            {
                var dataFromDayToPresent = GetDataForRollingWindow(dailyData, day, 30);
                double volatility = CalculateVolatility(dataFromDayToPresent, "1d");
                monthlyVolatilityList.Add(new KeyValuePair<int, double>(day, volatility));
            }
            return monthlyVolatilityList;
        }
        private static List<KeyValuePair<int, double>> BuildYearlyRollingVolatilityList(List<KeyValuePair<long, double>> dailyData)
        {
            var yearlyVolatilityList = new List<KeyValuePair<int, double>>();

            //yearly rolling volatility - assuming 1y is 365.25 days
            for (int day = 0; day <= 1461; day++)
            {
                var dataFromDayToPresent = GetDataForRollingWindow(dailyData, day, 365.25);
                double volatility = CalculateVolatility(dataFromDayToPresent, "1d");
                yearlyVolatilityList.Add(new KeyValuePair<int, double>(day, volatility));
            }
            return yearlyVolatilityList;
        }



        private async static Task<List<KeyValuePair<long, double>>> FetchClosingPricesWithTimestamps(HttpClient client, string uri, string interval)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri),
                Headers =
            {
                { "X-RapidAPI-Key", "33c2bbacf2msh125241b72a73a14p125482jsna4e4a00dcae6" },
                { "X-RapidAPI-Host", "yahoo-finance127.p.rapidapi.com" },
            },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                var jsonResponse = JObject.Parse(body);


                JToken closeData = jsonResponse["indicators"]["quote"][0]["close"];
                JToken timestampData = jsonResponse["timestamp"];
                if (interval == "1d") closeData = jsonResponse["indicators"]["adjclose"][0]["adjclose"];


                var closingPrices = closeData
                    .Where(closeValue => closeValue.Type != JTokenType.Null)
                    .Select(closeValue => (double)closeValue)
                    .ToList();

                var timestamps = timestampData
                    .Where(timestamp => timestamp.Type != JTokenType.Null)
                    .Select(timestamp => (long)timestamp)
                    .ToList();

                var closingPricesWithTimestamps = closingPrices.Zip(timestamps, (price, timestamp) => new KeyValuePair<long, double>(timestamp, price)).ToList();

                return closingPricesWithTimestamps;
            }
        }

        private static double CalculateStandardDeviation(List<double> values)
        {
            double avg = values.Average();
            double sumOfSquares = values.Sum(value => Math.Pow(value - avg, 2));
            return Math.Sqrt(sumOfSquares / (values.Count - 1));
        }

        private static double CalculateVolatility(List<double> closingPrices, string interval)
        {
            //ensure we have at least two days of data to calculate returns
            if (closingPrices.Count < 2) return 0;

            var intervalReturns = new List<double>();
            for (int i = 1; i < closingPrices.Count; i++)
            {
                //double intervalReturn = (closingPrices[i] - closingPrices[i - 1]) / closingPrices[i - 1]; //simple return
                double intervalReturn = Math.Log(closingPrices[i] / closingPrices[i - 1]); //logarithmic return 
                intervalReturns.Add(intervalReturn);
            }


            double standardDeviation = CalculateStandardDeviation(intervalReturns);
            double annualiseVolatility = 0;
            if (interval == "1d") annualiseVolatility = Math.Sqrt(252); //assuming 252 trading days py
            else if (interval == "15m") annualiseVolatility = Math.Sqrt(252 * 26); //6.5*4
            else if (interval == "1h") annualiseVolatility = Math.Sqrt(252 * 6.5);//Assuming 6.5 trading hours per day
            else if (interval == "2m") annualiseVolatility = Math.Sqrt(252 * 195);
            return standardDeviation * annualiseVolatility;
        }

    }
   
}
