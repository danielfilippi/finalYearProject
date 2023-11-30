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