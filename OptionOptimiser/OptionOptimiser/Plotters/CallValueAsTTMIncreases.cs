using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using OptionOptimiser.Calculators;
using OptionOptimiser.Objects;

namespace OptionOptimiser.Plotters
{
    internal class CallValueAsTTMIncreases
    {
        public List<Tuple<int, double, double>> DaysandValues = new List<Tuple<int, double, double>>();
        public CallValueAsTTMIncreases(double Spot, double Strike, double RiskFreeRate, char EuroAme, Option optionToAnalyse, Stock underlying)
        {

            for (int day = 1; day <= 35; day++) //4 years exactly (365.25*4) EDIT: Changed to only 30
            {
                double TimeToMaturity = day / 365.25;
                int Steps = 50;

                double Volatility = underlying.ALLVOLATILITIES[4][day].Value;

                double TheoreticalValue = BinomialCalculators.BinomialWithDividends(Steps, Spot, Strike, RiskFreeRate, Volatility, TimeToMaturity, 'C', EuroAme, underlying);
                double ImpliedVolatility = VolatilityCalculators.CalculateImpliedVolatility(Volatility, TheoreticalValue, Spot, Strike, TimeToMaturity, RiskFreeRate, 'C' , EuroAme, underlying, Steps);  //spot, spot to calculate iv for at the money call

                var toAdd = Tuple.Create(day,
                                         BinomialCalculators.BinomialWithDividends(Steps, Spot, Strike, RiskFreeRate, ImpliedVolatility, TimeToMaturity, 'C', EuroAme, underlying),
                                         ImpliedVolatility); 
                DaysandValues.Add(toAdd);
            }
        }
        public override string ToString()
        {
            StringBuilder returnString = new StringBuilder();
            for (int i = 0; i < DaysandValues.Count; i++)
            {
                double priceChange = 0;
                try
                {
                    priceChange = DaysandValues[i].Item2 - DaysandValues[i - 1].Item2;
                }
                catch (Exception ex)
                {
                    priceChange = DaysandValues[i].Item2;
                    continue;
                }
                returnString.AppendLine($"Price of call {(DaysandValues[i].Item1)} days away: {DaysandValues[i].Item2} - Up by: {priceChange} - IV: {DaysandValues[i].Item3}");
            }
            return returnString.ToString();
        }

        public void SaveToExcel(string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");
                worksheet.Cell("A1").Value = "Days away";
                worksheet.Cell("B1").Value = "Call Price";
                worksheet.Cell("C1").Value = "Price change";
                worksheet.Cell("D1").Value = "IV";

                for (int i = 0; i < DaysandValues.Count; i++)
                {
                    double daysAway = DaysandValues[i].Item1;
                    double price = DaysandValues[i].Item2;
                    double iv = DaysandValues[i].Item3;
                    double priceChange = 0;
                    if (i > 0)
                    {
                        priceChange = DaysandValues[i].Item2 - DaysandValues[i - 1].Item2;
                    }
                    // Replace NaN or infinity values with 0
                    daysAway = double.IsNaN(daysAway) || double.IsInfinity(daysAway) ? 0 : daysAway;
                    price = double.IsNaN(price) || double.IsInfinity(price) ? 0 : price;
                    priceChange = double.IsNaN(priceChange) || double.IsInfinity(priceChange) ? 0 : priceChange;
                    iv = double.IsNaN(iv) || double.IsInfinity(iv) ? 0 : iv;

                    worksheet.Cell(i + 2, 1).Value = DaysandValues[i].Item1;
                    worksheet.Cell(i + 2, 2).Value = DaysandValues[i].Item2;
                    worksheet.Cell(i + 2, 3).Value = priceChange;
                    worksheet.Cell(i + 2, 4).Value = DaysandValues[i].Item3;
                }

                workbook.SaveAs(filePath);
            }
        }

    }
}