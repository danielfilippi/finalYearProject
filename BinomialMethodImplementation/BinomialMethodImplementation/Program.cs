using System;
using System.Drawing;
// See https://aka.ms/new-console-template for more information



double Binomial(int Steps, double Spot, double Strike, double RiskFreeRate, double Volatility, double TimeToMaturity, char PutCall, char EuroAme)
{
    double DaysPerStep = TimeToMaturity / Steps;
    double upvalue = Math.Exp(Volatility * Math.Sqrt(DaysPerStep));
    double downvalue = 1 / upvalue;
    double p = (Math.Exp(RiskFreeRate*DaysPerStep)-downvalue)/(upvalue-downvalue);

    //BUILD TREE

    double[,] Tree = new double[Steps+1, Steps+1];
    Tree[0, 0] = Spot;
    for(int i = 1; i<=Steps; i++)
    {
        Tree[i, 0] = Tree[i-1, 0]*upvalue;
        for(int j = 1; j<=i; j++)
        {
            Tree[i, j] = Tree[i - 1, j-1] * downvalue;
        }
    }

    //CALC OPTION VALUE AT EACH FINAL STEP

    double[,] ReversedTree = new double[Steps + 1, Steps + 1];
    for (int i = 0; i<= Steps; i++)
    {
        if (PutCall == 'P') ReversedTree[Steps, i] = Math.Max(0, Strike - Tree[Steps, i]);
        else if (PutCall == 'C') ReversedTree[Steps, i] = Math.Max(0, Tree[Steps, i] - Strike);
    }

    //FIND OPTION PRICE AT ROOT NODE

    for (int i = Steps-1; i>=0; i--)
    {
        for(int j =0; j<= Steps-1; j++)
        {
            if (EuroAme == 'E') ReversedTree[i, j] = Math.Exp(-RiskFreeRate * DaysPerStep) * (p * ReversedTree[i + 1, j] + (1 - p) * ReversedTree[i + 1, j + 1]);
            else if(EuroAme == 'A')
            {
                if (PutCall == 'P') ReversedTree[i, j] = Math.Max(Strike - Tree[i, j], Math.Exp(-RiskFreeRate * DaysPerStep) * (p * ReversedTree[i + 1, j] + (1 - p) * ReversedTree[i + 1, j + 1]));
                else if (PutCall == 'C') ReversedTree[i, j] = Math.Max(Tree[i, j] - Strike, Math.Exp(-RiskFreeRate * DaysPerStep) * (p * ReversedTree[i + 1, j] + (1 - p) * ReversedTree[i + 1, j + 1]));
            }
        }
    }
    return ReversedTree[0,0];
}

int days = 200;

double Spot = 100;
double Strike = 99;
double RiskFreeRate = 0.05;
double Volatility = 0.2575;
double TimeToMaturity = days/(365.00);
int Steps = 1000;

Console.WriteLine("Spot price: " + Spot);
Console.WriteLine("Strike price: " + Strike);
Console.WriteLine("Days to maturity: " + days);
Console.WriteLine("Volatility: " + Volatility);
Console.WriteLine("Put value for European option: " + Binomial(Steps, Spot, Strike, RiskFreeRate, Volatility, TimeToMaturity, 'P', 'E'));
Console.WriteLine("Call value for European option: " + Binomial(Steps, Spot, Strike, RiskFreeRate, Volatility, TimeToMaturity, 'C', 'E'));
Console.WriteLine("Put value for American option: " + Binomial(Steps, Spot, Strike, RiskFreeRate, Volatility, TimeToMaturity, 'P', 'A'));
Console.WriteLine("Call value for American option: " + Binomial(Steps, Spot, Strike, RiskFreeRate, Volatility, TimeToMaturity, 'C', 'A'));

