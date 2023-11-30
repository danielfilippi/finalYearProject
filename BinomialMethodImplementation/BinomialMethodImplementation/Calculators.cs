﻿using Newtonsoft.Json.Linq;
using System;
using System.Globalization;



namespace BinomialMethodImplementation
{
    internal class Calculators
    {
        public static double CalculateImpliedVolatility(double annualVolatility, double marketPrice, double S, double K, double T, double r, bool isCall)
        {
            double sigma = annualVolatility; // initial guess for volatility, 
            double sigmaPrev = 0;
            double tolerance = 1e-5;
            int maxIterations = 100;
            int iterations = 0;

            while (Math.Abs(sigma - sigmaPrev) > tolerance && iterations < maxIterations)
            {
                sigmaPrev = sigma;
                double d1 = (Math.Log(S / K) + (r + sigma * sigma / 2.0) * T) / (sigma * Math.Sqrt(T));
                double d2 = d1 - sigma * Math.Sqrt(T);
                double optionPrice = isCall ? S * NormCDF(d1) - K * Math.Exp(-r * T) * NormCDF(d2) : K * Math.Exp(-r * T) * NormCDF(-d2) - S * NormCDF(-d1);
                double vega = S * Math.Sqrt(T) * NormPDF(d1);

                sigma -= (optionPrice - marketPrice) / vega; // Newton-Raphson iteration
                iterations++;
            }

            return sigma;
        }
        private static double NormCDF(double x)
        {
            // Using the approximation of the error function
            return 0.5 * (1 + Erf(x / Math.Sqrt(2)));
        }
        private static double Erf(double x) //https://www.johndcook.com/blog/csharp_erf/ had to use this as no Math.erf() 
        {
            // constants for approximation
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = x < 0 ? -1 : 1;
            x = Math.Abs(x);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return sign * y;
        }

        private static double NormPDF(double x)
        {
            return Math.Exp(-x * x / 2) / Math.Sqrt(2 * Math.PI);
        }

        public async static Task<double> GetVolatilityDataAtAppropriateTimeframe(string Symbol, int days)
        {
            string interval=""; //adapting data granularity for option length. a yearly volatility variable for a weekly option is not too good
            if(days > 1825)
            {
                interval = "1mo";
            }
            else if (days > 30 && days <= 1825)
            {
                interval = "1d";
            }
            else if (days <= 30)
            {
                interval = "15m";
            }
            var client = new HttpClient();
            string uri = "https://mboum-finance.p.rapidapi.com/hi/history?symbol=" + Symbol + "&interval="+ interval +"&diffandsplits=false";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri),
                Headers =
                {
                    { "X-RapidAPI-Key", "33c2bbacf2msh125241b72a73a14p125482jsna4e4a00dcae6" },
                    { "X-RapidAPI-Host", "mboum-finance.p.rapidapi.com" },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();

                //parse the JSON response
                var jsonResponse = JObject.Parse(body);
                var endDate = DateTime.Now;
                var startDate = endDate.AddDays(-days); //Option maturity length - days backwards
                var closingPrices = jsonResponse["body"]
                    .Children()
                    .Select(token => new
                    {
                        Date = DateTime.ParseExact((string)token.First()["date"], "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        Close = (double)token.First()["close"]
                    })
                    .Where(x => x.Date >= startDate && x.Date <= endDate)
                     .Select(x => x.Close)
                     .ToList();

                return CalculateVolatility(closingPrices, interval);
            }

        }

        //here im calculating the volatility for the exact number of days in the past that the option is going to last
        private static double CalculateVolatility(List<double> closingPrices, string interval)
        {
            //ensure we have at least two days of data to calculate returns
            if (closingPrices.Count < 2) throw new InvalidOperationException("Insufficient data for volatility calculation.");

            var intervalReturns = new List<double>();
            for (int i = 1; i < closingPrices.Count; i++)
            {
                double intervalReturn = (closingPrices[i] - closingPrices[i - 1]) / closingPrices[i - 1];
                intervalReturns.Add(intervalReturn);
            }

            double standardDeviation = CalculateStandardDeviation(intervalReturns);
            double annualiseVolatility = 0;
            if (interval == "1d") annualiseVolatility = Math.Sqrt(252); //assuming 252 trading days py
            else if (interval == "15m") annualiseVolatility = Math.Sqrt(252 * 26); //Assuming 6.5 trading hours per day
            else if (interval == "3mo") annualiseVolatility = Math.Sqrt(12);
            return standardDeviation * annualiseVolatility; 
        }


        private static double CalculateStandardDeviation(List<double> values)
        {
            double avg = values.Average();
            double sumOfSquares = values.Sum(value => Math.Pow(value - avg, 2));
            return Math.Sqrt(sumOfSquares / (values.Count - 1));
        }
    }
}
