﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BinomialMethodImplementation
{
    internal class Option
    {
        DateTime MaturityDate;
        private static int days;
        private static int Steps;
        private static double TimeToMaturity;


        private static double Spot;
        private static double Strike;

        private static double RiskFreeRate;

        private static double Volatility;
        private static double ImpliedVolatility;
        //If the original volatility and the derived IV are very close to each other is a good sign. It suggests that the IV calculation is aligning well with the market conditions as reflected in the original model.

        public static double TheoreticalValue;
        public static double OptionValueWithIV;

        public static char EuroAme;
        public static char PutCall;
        public static char LongShort;

        private static bool call;


        public static double theta;
        public static double thetaDecayDays;
        public double ValueAfterDaysOfTimeDecay;

        public static double UserSetMargin;
        public static double UserSetAdditionalMargin;

        public static double BreakEvenPoint;
        public static double MaxWin;
        public static double MaxLoss;
        public static double NetDebit;
        public static double NetCredit;
        public static double Margin;
        public static double UnderlyingOwned;


        public Stock underlying;

        /*
        days = 200;
        Spot = 100;
        Strike = 99;
        RiskFreeRate = 0.05;
        Volatility = 0.25;
        TimeToMaturity = days / (365.00);
        Steps = days* 5; //High resolution model
        */


        public Option(string Symbol)
        {
            days = GetDaysToMaturity();
            underlying = new Stock(Symbol, days);
            Thread.Sleep(1000);
            Spot = underlying.GetValue();
            Thread.Sleep(1000);
            Strike = GetStrikePrice();
            Volatility = underlying.GetVolatility();
            RiskFreeRate = 0.05;
            GetNatureOfOption();

            TimeToMaturity = days / (365.25);
            Steps = days * 5; //High resolution model

            //FIRST WE CALCULATE THE THEORETICAL OPTION VALUE - THEN WE CAN DERIVE THE IV FROM It
            TheoreticalValue = BinomialWithDividends(Steps, Spot, Strike, RiskFreeRate, Volatility, TimeToMaturity, PutCall, EuroAme);
            ImpliedVolatility = Calculators.CalculateImpliedVolatility(Volatility, TheoreticalValue, Spot, Strike, TimeToMaturity, RiskFreeRate, call);
            OptionValueWithIV = BinomialWithDividends(Steps, Spot, Strike, RiskFreeRate, ImpliedVolatility, TimeToMaturity, PutCall, EuroAme);
            FindBreakEven();
            FindMaxLoss();
            FindMaxProfit();
            FindNetDebCred();
            FindPossibleMargin();
            
            
            SetTheta();
            //ValueAfterDaysOfTimeDecay = SetThetaDecay(ValueWithIV);
        }
        private static void FindPossibleMargin()//margin possible if user doesnt own underlying asset
        {
            Margin = 0;
            Console.WriteLine("Enter your desired base margin percentage: Represented as a number between 0-1");
            UserSetMargin = double.Parse(Console.ReadLine());
            Console.WriteLine("Enter your desired additional margin percentage: Represented as a number between 0-1");
            UserSetAdditionalMargin = double.Parse(Console.ReadLine());
            if (LongShort == 'S')
            {
                Margin += Spot * UserSetMargin;
                if ((PutCall == 'P' && Strike > Spot) || (PutCall == 'C' && Strike < Spot))
                {
                    double outOfTheMoneyAmt = Math.Abs(Spot - Strike);
                    Margin += outOfTheMoneyAmt * UserSetAdditionalMargin;
                }
            }
            return;
        }
        private static void FindNetDebCred()
        {
            if (LongShort == 'L') NetDebit = -OptionValueWithIV; //buy option you are giving money
            else NetCredit = OptionValueWithIV;  //selling option you are receiving money
        }
        private static void FindMaxProfit() //for 1 share, not 100
        {
            if (LongShort == 'L')
            {
                if (PutCall == 'C') MaxWin = -1; //Unlimited
                else if (PutCall == 'P') MaxWin = Strike - OptionValueWithIV;
            }
            else MaxWin = OptionValueWithIV;
        }
        private static void FindMaxLoss() //Just copy above function and change L to S. Put-call parity
        {
            if (LongShort == 'S')
            {
                if (PutCall == 'C') MaxLoss = -1; //Unlimited
                else if (PutCall == 'P') MaxLoss = Strike - OptionValueWithIV;
            }
            else MaxLoss = OptionValueWithIV;
        }
        private static void FindBreakEven()
        {
            if (call) BreakEvenPoint = Strike + OptionValueWithIV;
            if (!call) BreakEvenPoint = Strike - OptionValueWithIV;
        }
        private static void SetTheta() //one day theta 
        {
            double adjustedTTM = TimeToMaturity - (1 / 365.25);
            theta = -(OptionValueWithIV - Binomial(Steps, Spot, Strike, RiskFreeRate, ImpliedVolatility, adjustedTTM, PutCall, EuroAme));
        }
        private static double SetThetaDecay(double OptionValue)
        {
            Console.WriteLine("Enter amount of days by which you'd like to simulate the effect of time decay on the options price.\n" +
                                        "For example, if you want to estimate the effect of one days worth of time decay (a common analysis), set this to 1.\n" +
                                        "If you're interested in the effect of time decay over a specific number of days, such as a week or a month, enter that number.\n" +
                                        "If the option is nearing its expiration and you want to understand the impact of time decay as expiration approaches, you can set this to the number of days left until expiration.\n\n");
            while (true)
            {
                Console.WriteLine("Enter desired days to decay: ");
                string input = Console.ReadLine();
                if (double.TryParse(input, out double thetaDays))
                {
                    if (thetaDays > 0 && thetaDays < days) return SetThetaDecayOptionValue(OptionValue, thetaDays);
                    else Console.WriteLine("Theta must be above zero and less than the amount of days to maturity");
                }
                else Console.WriteLine("Invalid input. Please enter a numeric value.");
            }
        }
        private static double SetThetaDecayOptionValue(double optionValue, double thetaDays)
        {
            thetaDecayDays = thetaDays;
            double adjustedTTM = TimeToMaturity - (thetaDays / 365.25);
            double optionPriceXDaysLess = Binomial(Steps, Spot, Strike, RiskFreeRate, ImpliedVolatility, adjustedTTM, PutCall, EuroAme);

            //double optionPriceXDaysLess = ValueWithIV - theta * days;

            return optionPriceXDaysLess;
        }
        private static void GetNatureOfOption()
        {
            Console.WriteLine("L for Long (Buy option), S for Short (Sell option). Caps don't matter");
            LongShort = char.Parse(Console.ReadLine().ToUpper());
            Console.WriteLine("E for European, A for American. Caps don't matter");
            EuroAme = char.Parse(Console.ReadLine().ToUpper());
            Console.WriteLine("P for Put, C for Call. Caps don't matter");
            PutCall = char.Parse(Console.ReadLine().ToUpper());
            if (PutCall == 'C') call = true; //lol
            else call = false;
        }
        private static double GetStrikePrice()
        {
            while (true)
            {
                Console.WriteLine("Enter desired strike price: ");
                string input = Console.ReadLine();
                if (double.TryParse(input, out double strikePrice))
                {
                    if (strikePrice > 0) return strikePrice;
                    else Console.WriteLine("Please enter a positive number for the strike price.");
                }
                else Console.WriteLine("Invalid input. Please enter a numeric value.");
            }
        }
        private static int GetDaysToMaturity()
        {
            while (true)
            {
                Console.WriteLine("Enter the maturity date for the option in DD/MM/YYYY format: ");
                string input = Console.ReadLine();
                if (DateTime.TryParseExact(input, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime maturityDate))
                {
                    DateTime today = DateTime.Today;
                    if (maturityDate < today) Console.WriteLine("The entered date is in the past. Please enter a future date.");
                    else return (maturityDate - today).Days;
                }
                else Console.WriteLine("Invalid date format. Please enter the date in DD/MM/YYYY format.");
            }
        }

        public override string ToString()
        {
            string MaxWinString = MaxWin.ToString();
            if (MaxWinString == "-1") MaxWinString = "Unlimited";
            string MaxLossString = MaxLoss.ToString();
            if (MaxLossString == "-1") MaxLossString = "Unlimited";

            string o = LongShort.ToString() + "" + EuroAme.ToString() + " " + PutCall.ToString() + " " + underlying.GetSym() + "\n" +
                           "Maturity date (days away): " + days + "\n" +
                           "Spot price: " + Spot + " " + underlying.GetCurrency() + "\n" + 
                           "Strike price: " + Strike + "\n" +
                           "Risk Free Rate: " + RiskFreeRate + "\n" +
                           "Implied Volatility: " + ImpliedVolatility + "\n" +
                           "Option Value: " + OptionValueWithIV + "\n" +
                           "One day theta: " + theta + "\n" +
                           //"Option value after " + thetaDecayDays + " days of theta decay: " + ValueAfterDaysOfTimeDecay + "\n"
                           "Break even point: " + BreakEvenPoint + " " + underlying.GetCurrency() + "\n" +
                           "Max Win: " + MaxWinString + "\n" +
                           "Max Loss: " + MaxLossString + "\n" +
                           "Net Debit: " + NetDebit + "\n" +
                           "Net Credit: " + NetCredit + "\n" +
                           "Total margin to be paid in stock or cash: " + Margin + "\n";
                            
            return o;
        }
        private static double Binomial(int Steps, double Spot, double Strike, double RiskFreeRate, double Volatility, double TimeToMaturity, char PutCall, char EuroAme)
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
        private double BinomialWithDividends(int Steps, double Spot, double Strike, double RiskFreeRate, double Volatility, double TimeToMaturity, char PutCall, char EuroAme)
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
