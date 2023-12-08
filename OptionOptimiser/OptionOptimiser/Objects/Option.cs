using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptionOptimiser.Calculators;

namespace OptionOptimiser.Objects
{
    internal class Option
    {
        //Nature of option
        private bool call;
        public char EuroAme;
        public char PutCall;
        public char LongShort;
        //Maturity info
        public DateTime MaturityDate;
        private int days;
        private int Steps;
        private double TimeToMaturity;
        //Current asset value and strike price
        public double Spot;
        public double Strike;
        //RFR
        public double RiskFreeRate;
        //Volatility data
        public string AdaptiveString;
        public double AdaptiveVolatility;
        public double ImpliedVolatility;
        //If the original volatility and the derived IV are very close to each other is a good sign. It suggests that the IV calculation is aligning well with the market conditions as reflected in the original model.
        //Option premiums
        public double TheoreticalValue;
        public double OptionValueWithIV;
        //Theta decay calc vars
        public double thetaDecayDays;
        public double ValueAfterDaysOfTimeDecay;
        //Margin vars
        public double UserSetMargin;
        public double UserSetAdditionalMargin; //if option out of the money
        //Financials
        public double BreakEvenPoint { get; set; }
        public double MaxWin;
        public double MaxLoss;
        public double NetDebitCredit; //negative = debit
        public double Margin;
        public double UnderlyingOwned;
        //Greeks
        public double delta;
        public double gamma;
        public double theta;
        public double vega;
        public double rho;

        public Stock underlying;
        public Option(string Symbol)
        {
            days = SetDaysToMaturity();
            underlying = new Stock(Symbol, days);
            Thread.Sleep(1000); //avoid race condition?? weirdness going on
            Spot = underlying.GetValue();
            Thread.Sleep(1000);
            Strike = SetStrikePrice();
            SetAdaptiveVolatility();

            RiskFreeRate = 0.05;
            SetNatureOfOption();

            TimeToMaturity = days / 365.25;
            Steps = days * 5; //High resolution model

            //FIRST WE CALCULATE THE THEORETICAL OPTION VALUE - THEN WE CAN DERIVE THE IV FROM It
            TheoreticalValue = BinomialCalculators.BinomialWithDividends(Steps, Spot, Strike, RiskFreeRate, AdaptiveVolatility, TimeToMaturity, PutCall, EuroAme, underlying);
            ImpliedVolatility = VolatilityCalculators.CalculateImpliedVolatility(AdaptiveVolatility, TheoreticalValue, Spot, Spot, TimeToMaturity, RiskFreeRate, PutCall, EuroAme, underlying, Steps);  //spot, spot so at the money option is being used to calc IV
            OptionValueWithIV = BinomialCalculators.BinomialWithDividends(Steps, Spot, Strike, RiskFreeRate, ImpliedVolatility, TimeToMaturity, PutCall, EuroAme, underlying);
            //FINANCIALS
            BreakEvenPoint = FinancialCalculators.FindBreakEven(OptionValueWithIV, Strike, call);
            MaxLoss = FinancialCalculators.FindMaxLoss(LongShort, PutCall, Strike, OptionValueWithIV);
            MaxWin = FinancialCalculators.FindMaxProfit(LongShort, PutCall, Strike, OptionValueWithIV);
            NetDebitCredit = FinancialCalculators.FindNetDebCred(OptionValueWithIV, LongShort);
            Margin = FinancialCalculators.CalcPossibleMargin(LongShort, PutCall, Spot, Strike);
            //GREEKS
            delta = GreekCalculators.CalcDelta(OptionValueWithIV, Steps, Spot, Strike, RiskFreeRate, ImpliedVolatility, TimeToMaturity, PutCall, EuroAme, underlying);
            gamma = GreekCalculators.CalcGamma(OptionValueWithIV, Steps, Spot, Strike, RiskFreeRate, ImpliedVolatility, TimeToMaturity, PutCall, EuroAme, delta, underlying);
            theta = GreekCalculators.CalcTheta(OptionValueWithIV, Steps, Spot, Strike, RiskFreeRate, ImpliedVolatility, TimeToMaturity, PutCall, EuroAme, underlying);
            vega = GreekCalculators.CalcVega(OptionValueWithIV, Steps, Spot, Strike, RiskFreeRate, ImpliedVolatility, TimeToMaturity, PutCall, EuroAme, underlying);
            rho = GreekCalculators.CalcRho(OptionValueWithIV, Steps, Spot, Strike, RiskFreeRate, ImpliedVolatility, TimeToMaturity, PutCall, EuroAme, underlying);


            //SetTheta();
            //ValueAfterDaysOfTimeDecay = SetThetaDecay(OptionValueWithIV);
        }
        private void SetAdaptiveVolatility()
        {
            if (days < 7)
            {
                AdaptiveVolatility = underlying.WeeklyVolatility;
                AdaptiveString = "Weekly";
            }//weekly for now as daily seems wrong
            else if (days >= 7 && days < 49) 
            {
                AdaptiveVolatility = underlying.WeeklyVolatility;
                AdaptiveString = "Weekly";
            }
            else if ((days >= 50 && days < 365)) 
            {
                AdaptiveVolatility = underlying.MonthlyVolatility;
                AdaptiveString = "Monthly";
            }
            else
            {
                AdaptiveVolatility = underlying.AnnualVolatility;
                AdaptiveString = "Yearly";
            }
        }
        private void SetNatureOfOption()
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
        private int SetDaysToMaturity()
        {
            while (true)
            {
                Console.WriteLine("Enter the maturity date for the option in DD/MM/YYYY format: ");
                string input = Console.ReadLine();
                if (DateTime.TryParseExact(input, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime maturityDate))
                {
                    DateTime today = DateTime.Today;
                    MaturityDate = maturityDate;
                    if (maturityDate < today) Console.WriteLine("The entered date is in the past. Please enter a future date.");
                    else return (maturityDate - today).Days;
                }
                else Console.WriteLine("Invalid date format. Please enter the date in DD/MM/YYYY format.");
            }
        }
        private double SetStrikePrice()
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
        public override string ToString()
        {
            string MaxWinString = "";
            if (MaxWin == double.PositiveInfinity) MaxWinString = "Unlimited";
            else MaxWinString = MaxWin.ToString();
            string MaxLossString = "";
            if (MaxLoss == double.NegativeInfinity) MaxLossString = "Unlimited";
            else MaxLossString = MaxLoss.ToString();

            string o = LongShort.ToString() + "" + EuroAme.ToString() + " " + PutCall.ToString() + " " + underlying.GetSym() + "\n" +
                           "Maturity date (days away): " + days + "\n" +
                           "Spot price: " + Spot + " " + underlying.GetCurrency() + "\n" +
                           "Strike price: " + Strike + "\n" +
                           "Risk Free Rate: " + RiskFreeRate + "\n" +
                           "Dividend Yield: " + underlying.DividendYield + "\n" +
                           "Theoretical Value: " + TheoreticalValue + "\n" +
                           $"Volatility ({AdaptiveString}):  " + AdaptiveVolatility + "\n" +
                           "Implied Volatility: " + ImpliedVolatility + "\n" +
                           "Option Value: " + OptionValueWithIV + "\n" +
                           //"One day theta: " + theta + "\n" +
                           //"Option value after " + thetaDecayDays + " days of theta decay: " + ValueAfterDaysOfTimeDecay + "\n"
                           "Break even point: " + BreakEvenPoint + " " + underlying.GetCurrency() + "\n" +
                           "Max Win: " + MaxWinString + "\n" +
                           "Max Loss: " + MaxLossString + "\n" +
                           "Net Debit/Credit: (Debit is negative) " + NetDebitCredit + "\n" +
                           "Total margin to be paid in stock or cash: " + Margin + "\n\n" +
                           "Delta: " + delta + "\n" +
                           "Gamma (NOT WORKING): " + gamma + "\n" +
                           "Theta: " + theta + "\n" +
                           "Vega: " + vega + "\n" +
                           "Rho: " + rho + "\n";

            return o;
        }
        public double GetBreakEvenPoint()
        {
            return BreakEvenPoint;
        }
        public char GetPutCall()
        {
            return PutCall;
        }
        public char GetLongShort()
        {
            return LongShort;
        }
        public double GetSpot()
        {
            return Spot;
        }
        public DateTime GetMaturityDate()
        {
            return MaturityDate;
        }
        public double GetValue()
        {
            return OptionValueWithIV;
        }
        public double GetMaxWin()
        {
            return MaxWin;
        }
        public double GetMaxLoss()
        {
            return MaxLoss;
        }
        public double GetNetDebCred()
        {
            return NetDebitCredit;
        }
        public double GetMargin()
        {
            return Margin;
        }

        public double GetDelta()
        {
            return delta;
        }
        public double GetGamma()
        {
            return gamma; 
        }
        public double GetVega()
        {
            return vega; // Assuming Vega() is a method that calculates Vega
        }
        public double GetTheta()
        {
            return theta; // Assuming Theta() is a method that calculates Theta
        }

        public double GetRho()
        {
            return rho; // Assuming Rho() is a method that calculates Rho
        }


    }
}
