using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Diagnostics;
using Newtonsoft.Json;
using static System.Net.WebRequestMethods;

namespace BinomialMethodImplementation
{
    internal class Stock
    {
        public static string Sym;

        public static int DaysForVolatility;
        public static double Value { get; set; }

        public static string Currency { get; set; }
        public static double AnnualVolatility { get; set; }
        public static double XDaysVolatility { get; set; }

        public Stock(string Symbol, int days)
        {
            Sym = Symbol;
            GetGeneralStockData(Symbol);
            DaysForVolatility = days;
            XDaysVolatility = SetVolatility(Symbol, DaysForVolatility);
        }

        private async static void GetGeneralStockData(string Symbol)
        {
            var client = new HttpClient();
            string uri = "https://mboum-finance.p.rapidapi.com/qu/quote/financial-data?symbol=" + Symbol;
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
                //dynamic deserialisation
                dynamic data = JsonConvert.DeserializeObject<dynamic>(body);
                // Update properties
                Value = data.body.currentPrice.raw;
                Currency = data.body.financialCurrency;
            }
        }

        private static double SetVolatility(string Symbol, int days)
        {
            return Calculators.GetVolatilityDataAtAppropriateTimeframe(Symbol, days).GetAwaiter().GetResult();
        }
        public double GetVolatility()
        {
            return XDaysVolatility;
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
    }
}
