using BinomialMethodImplementation;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
// See https://aka.ms/new-console-template for more information


Console.WriteLine("Enter Symbol");
string Symbol = Console.ReadLine();
Option a = new Option(Symbol);
Console.WriteLine(a);


//AMERICAN OPTIONS DONE
//IV CALCULATION DONE
//REAL TIME DATA DONE
//GREEKS ALL DONE EXCEPT GAMMA
//GREEKS SENSITIVITY ANALYSIS TODO
//INTEREST RATE CHANGES TODO
//HISTORICAL VOLATILITY ANALYSIS HALF DONE
//SCENARIO ANALYSIS TODO e.g allowing users to see how the option's value would change under different market conditions (e.g., a significant drop in the stock market).