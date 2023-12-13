using System;
using MathNet.Numerics.Distributions;
using ClosedXML.Excel;
using System.Collections.Generic;
double todaysPrice = 100;
double strike = 95; //remember, this is the price that you can sell the asset at, at expiration
double optionPremium = 2;
double equityMarketRiskPremium = 0.05; //https://indialogue.io/clients/reports/public/5d9da61986db2894649a7ef2/5d9da63386db2894649a7ef5
double stockBeta = 2;
double RFR = 0.05;
double expectedStockReturn = RFR + (stockBeta * equityRiskPremium);
double volatility = 0.15;
double days = 30;
double dailyVolatility = volatility / Math.Sqrt(252);
double expDailyReturn = expectedStockReturn /252;
List<double> simmedValues = new List<double>();
List<double> payoffs = new List<double>();
double increment = optionPremium * 0.5; // 5% of the option's value


Random rand = new Random();

for (int i = 0; i<200; i++) //for european, just for now
{
    double simulatedPrice = todaysPrice;
    for(int j =0; j<days; j++)
    {
        double percentile = rand.NextDouble();
        double dailyReturn = Normal.InvCDF(expDailyReturn, dailyVolatility, percentile);
        simulatedPrice *= (1 + dailyReturn);
        //Console.WriteLine($"Percentile: {percentile}, Daily Return: {dailyReturn}");

    }
    simmedValues.Add(simulatedPrice);
    
}
// Calculate the DRV based on the simulated values
var drv = CalculateDRVForStockPriceDistribution(simmedValues, todaysPrice);

Console.WriteLine($"DISTRIBUTION OF STOCK PRICE AFTER {days} DAYS, INITIAL VALUE: {todaysPrice} \n");

// Print out the results
foreach (var range in drv)
{
    if(range.Value > 0)Console.WriteLine($"{range.Key}: {range.Value / (double)simmedValues.Count:P2}");
   
}
Console.WriteLine("\n");



foreach (double finalV in simmedValues)
{
    double payoff = finalV - strike - optionPremium;
    if (payoff < -optionPremium) payoff = -optionPremium;
    payoffs.Add(payoff);
    
  
}


var payoffDRV = CalculateDRVForPayoffDistribution(payoffs, optionPremium);

// Output the DRV to the console (or you can write it to an Excel file as needed)

Console.WriteLine($"DISTRIBUTION OF POTENTIAL PAYOFFS AFTER {days} DAYS, OPTION PREMIUM: {optionPremium}\n");

foreach (var range in payoffDRV)
{
    Console.WriteLine($"{range.Key}: {range.Value:P2}");
}

















Dictionary<string, int> CalculateDRVForStockPriceDistribution(List<double> values, double basePrice)
{
    var ranges = new Dictionary<string, int>();

    // Create ranges based on 5% intervals
    for (int i = -20; i <= 20; i++) // Assuming you want to go from -100% to +100% in 5% steps
    {
        double lowerBound = basePrice * (1 + (i * 0.05));
        double upperBound = basePrice * (1 + ((i + 1) * 0.05));
        ranges[$"{lowerBound:n2}<x<{upperBound:n2}"] = 0;
    }
    // Add a range for x >= 200% of basePrice
    ranges[$"x>={basePrice * 2:n2}"] = 0;

    // Categorize each value into a range
    foreach (var value in values)
    {
        bool foundRange = false;
        foreach (var range in ranges.Keys.ToList())
        {
            if (range.StartsWith("x>="))
            {
                if (value >= basePrice * 2)
                {
                    ranges[range]++;
                    foundRange = true;
                    break;
                }
            }
            else
            {
                var bounds = range.Split(new string[] { "<x<" }, StringSplitOptions.None);
                double lowerBound = double.Parse(bounds[0]);
                double upperBound = double.Parse(bounds[1]);
                if (value >= lowerBound && value < upperBound)
                {
                    ranges[range]++;
                    foundRange = true;
                    break;
                }
            }
        }

        // In case value is outside the defined ranges
        if (!foundRange)
        {
            string lastKey = ranges.Keys.Last();
            ranges[lastKey]++;
        }
    }

    return ranges;
}
static Dictionary<string, double> CalculateDRVForPayoffDistribution(List<double> payoffs, double optionPremium)
{
    var drv = new Dictionary<string, double>();
    double increment = optionPremium * 0.5; // 5% increments of the option's value

    // Define the range based on the maximum and minimum possible payoff
    double minPayoff = payoffs.Min();
    double maxPayoff = payoffs.Max();

    // Establish the range boundaries
    double lowerBound = minPayoff - (minPayoff % increment) - increment;
    double upperBound = maxPayoff - (maxPayoff % increment) + increment;

    // Initialize the ranges
    for (double i = lowerBound; i < upperBound; i += increment)
    {
        string rangeKey = $"{i:n2} to {i + increment:n2}";
        drv[rangeKey] = 0;
    }

    // Count the payoffs in each range
    foreach (var payoff in payoffs)
    {
        foreach (var key in drv.Keys.ToList())
        {
            double rangeStart = double.Parse(key.Split(' ')[0]);
            double rangeEnd = double.Parse(key.Split(' ')[2]);

            if (payoff >= rangeStart && payoff < rangeEnd)
            {
                drv[key]++;
                break;
            }
        }
    }

    // Calculate probabilities
    double totalPayoffs = payoffs.Count;
    foreach (var key in drv.Keys.ToList())
    {
        drv[key] /= totalPayoffs;
    }

    return drv;
}
