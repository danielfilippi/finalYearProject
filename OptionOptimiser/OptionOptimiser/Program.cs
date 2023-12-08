using Microsoft.VisualBasic.FileIO;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using OptionOptimiser.Objects;
using OptionOptimiser.Plotters;
// See https://aka.ms/new-console-template for more information


Console.WriteLine("Enter Symbol");
string Symbol = Console.ReadLine();
Option a = new Option(Symbol);
Console.WriteLine(a);
PutValueAsTTMIncreases putvalues = new PutValueAsTTMIncreases(a.Spot, a.Strike, a.RiskFreeRate, a.EuroAme,a, a.underlying);  //REFACTOR LATER TO TAKE OUT ALL THE a.XX SHIT
CallValueAsTTMIncreases callvalues = new CallValueAsTTMIncreases(a.Spot, a.Strike, a.RiskFreeRate, a.EuroAme,a, a.underlying);

Console.WriteLine(putvalues);
Console.WriteLine(callvalues);
putvalues.SaveToExcel("C:\\Users\\danie\\Documents\\FINALYEARPROJ\\excels\\putvalues.xlsx");
callvalues.SaveToExcel("C:\\Users\\danie\\Documents\\FINALYEARPROJ\\excels\\callvalues.xlsx");


//AMERICAN OPTIONS DONE
//IV CALCULATION DONE
//REAL TIME DATA DONE
//GREEKS ALL DONE EXCEPT GAMMA
//GREEKS SENSITIVITY ANALYSIS TODO
//INTEREST RATE CHANGES TODO
//HISTORICAL VOLATILITY ANALYSIS HALF DONE
//SCENARIO ANALYSIS TODO e.g allowing users to see how the option's value would change under different market conditions (e.g., a significant drop in the stock market).










//Try this: aapl, strike 150. Will return all NaN?? issue with iv calcuation, converges to -inf