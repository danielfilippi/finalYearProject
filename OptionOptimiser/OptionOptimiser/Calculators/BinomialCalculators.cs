using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptionOptimiser.Objects;

namespace OptionOptimiser.Calculators
{
    internal class BinomialCalculators
    {
        //PLAIN BINOMIAL METHOD
        public static double Binomial(int Steps, double Spot, double Strike, double RiskFreeRate, double Volatility, double TimeToMaturity, char PutCall, char EuroAme, Stock underlying)
        {
            double TimePerStep = TimeToMaturity / Steps;//More like TimeInDaysPerStep
            double upvalue = Math.Exp(Volatility * Math.Sqrt(TimePerStep));
            double downvalue = 1 / upvalue;
            double p = (Math.Exp(RiskFreeRate * TimePerStep) - downvalue) / (upvalue - downvalue);

            //BUILD TREE

            double[,] Tree = new double[Steps + 1, Steps + 1];
            Tree[0, 0] = Spot;
            for (int i = 1; i <= Steps; i++)
            {
                Tree[i, 0] = Tree[i - 1, 0] * upvalue;
                for (int j = 1; j <= i; j++)
                {
                    Tree[i, j] = Tree[i - 1, j - 1] * downvalue;
                }
            }

            //CALC OPTION VALUE AT EACH FINAL STEP

            double[,] ReversedTree = new double[Steps + 1, Steps + 1];
            for (int i = 0; i <= Steps; i++)
            {
                if (PutCall == 'P') ReversedTree[Steps, i] = Math.Max(0, Strike - Tree[Steps, i]);
                else if (PutCall == 'C') ReversedTree[Steps, i] = Math.Max(0, Tree[Steps, i] - Strike);
            }

            //FIND OPTION PRICE AT ROOT NODE

            for (int i = Steps - 1; i >= 0; i--)
            {
                for (int j = 0; j <= Steps - 1; j++)
                {
                    if (EuroAme == 'E') ReversedTree[i, j] = Math.Exp(-RiskFreeRate * TimePerStep) * (p * ReversedTree[i + 1, j] + (1 - p) * ReversedTree[i + 1, j + 1]);
                    else if (EuroAme == 'A')
                    {
                        if (PutCall == 'P') ReversedTree[i, j] = Math.Max(Strike - Tree[i, j], Math.Exp(-RiskFreeRate * TimePerStep) * (p * ReversedTree[i + 1, j] + (1 - p) * ReversedTree[i + 1, j + 1]));
                        else if (PutCall == 'C') ReversedTree[i, j] = Math.Max(Tree[i, j] - Strike, Math.Exp(-RiskFreeRate * TimePerStep) * (p * ReversedTree[i + 1, j] + (1 - p) * ReversedTree[i + 1, j + 1]));
                    }
                }
            }
            return ReversedTree[0, 0];
        }

        //continuous dividend model as discreet is out of my realm ofpossibility i think
        public static double BinomialWithDividends(int Steps, double Spot, double Strike, double RiskFreeRate, double Volatility, double TimeToMaturity, char PutCall, char EuroAme, Stock underlying)
        {
            double TimePerStep = TimeToMaturity / Steps;//More like TimeInDaysPerStep
            double upvalue = Math.Exp(Volatility * Math.Sqrt(TimePerStep));
            double downvalue = 1 / upvalue;
            double p = (Math.Exp((RiskFreeRate - underlying.GetDividendYield()/100) * TimePerStep) - downvalue) / (upvalue - downvalue);

            //BUILD TREE

            double[,] Tree = new double[Steps + 1, Steps + 1];
            Tree[0, 0] = Spot;
            for (int i = 1; i <= Steps; i++)
            {
                Tree[i, 0] = Tree[i - 1, 0] * upvalue;
                for (int j = 1; j <= i; j++)
                {
                    Tree[i, j] = Tree[i - 1, j - 1] * downvalue;
                }
            }

            //CALC OPTION VALUE AT EACH FINAL STEP

            double[,] ReversedTree = new double[Steps + 1, Steps + 1];
            for (int i = 0; i <= Steps; i++)
            {
                if (PutCall == 'P') ReversedTree[Steps, i] = Math.Max(0, Strike - Tree[Steps, i]);
                else if (PutCall == 'C') ReversedTree[Steps, i] = Math.Max(0, Tree[Steps, i] - Strike);
            }

            //FIND OPTION PRICE AT ROOT NODE

            for (int i = Steps - 1; i >= 0; i--)
            {
                for (int j = 0; j <= Steps - 1; j++)
                {
                    if (EuroAme == 'E') ReversedTree[i, j] = Math.Exp(-RiskFreeRate * TimePerStep) * (p * ReversedTree[i + 1, j] + (1 - p) * ReversedTree[i + 1, j + 1]);
                    else if (EuroAme == 'A')
                    {
                        if (PutCall == 'P') ReversedTree[i, j] = Math.Max(Strike - Tree[i, j], Math.Exp(-RiskFreeRate * TimePerStep) * (p * ReversedTree[i + 1, j] + (1 - p) * ReversedTree[i + 1, j + 1]));
                        else if (PutCall == 'C') ReversedTree[i, j] = Math.Max(Tree[i, j] - Strike, Math.Exp(-RiskFreeRate * TimePerStep) * (p * ReversedTree[i + 1, j] + (1 - p) * ReversedTree[i + 1, j + 1]));
                    }
                }
            }
            return ReversedTree[0, 0];
        }
    }
}
