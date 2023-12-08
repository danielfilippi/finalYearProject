using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Diagnostics;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;
using OptionOptimiser.Calculators;

namespace OptionOptimiser.Objects
{
    internal class Stock
    {
        public string Sym;

        public double DividendYield { get; set; }

        public int DaysForVolatility;
        public double Value { get; set; }

        public string Currency { get; set; }
        public double AnnualVolatility { get; set; }
        public double DailyVolatility { get; set; }
        public double MonthlyVolatility { get; set; }
        public double WeeklyVolatility { get; set; }
        public double XDaysVolatility { get; set; } //volatility taking into account data since x days ago


        public Stock(string Symbol, int days)
        {
            Sym = Symbol;
            SetGeneralStockData(Symbol);
            //SetDividendYield();
            DaysForVolatility = days;

            DailyVolatility = SetVolatilityData(Symbol)[0][0].Value;
            WeeklyVolatility = SetVolatilityData(Symbol)[1][0].Value;
            MonthlyVolatility = SetVolatilityData(Symbol)[2][0].Value;
            AnnualVolatility = SetVolatilityData(Symbol)[3][0].Value;

        }
        private async void SetGeneralStockData(string Symbol)
        {
            var client = new HttpClient();
            string uri = "https://yahoo-finance127.p.rapidapi.com/price/" + Symbol;
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
                //dynamic deserialisation
                dynamic data = JsonConvert.DeserializeObject<dynamic>(body);
                // Update properties
                Value = data.regularMarketPrice.raw;
                Currency = data.currency;
            }

            string uri2 = "https://yahoo-finance127.p.rapidapi.com/key-statistics/" + Symbol;
            var request2 = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri2),
                Headers =
                {
                    { "X-RapidAPI-Key", "33c2bbacf2msh125241b72a73a14p125482jsna4e4a00dcae6" },
                    { "X-RapidAPI-Host", "yahoo-finance127.p.rapidapi.com" },
                },
            };
            using (var response2 = await client.SendAsync(request2))
            {
                response2.EnsureSuccessStatusCode();
                var body = await response2.Content.ReadAsStringAsync();
                //dynamic deserialisation
                dynamic data = JsonConvert.DeserializeObject<dynamic>(body);    
                // Update properties
                if (data?.dividendYield?.raw != null) DividendYield = data.dividendYield.raw;
                else DividendYield = 0;
            }
        }

        private List<List<KeyValuePair<int, double>>> SetVolatilityData(string Symbol)
        {
            
            
             return VolatilityCalculators.GetVolatilityData(Symbol).Result;
            

        }
        public double GetAnnualVolatility()
        {
            return AnnualVolatility;
        }
        public double GetValue()
        {
            return Value;
        }
        public string GetCurrency()
        {
            return Currency;
        }
        public string GetSym()
        {
            return Sym;
        }
        public double GetDividendYield()
        {
            return DividendYield;
        }
    }
}
