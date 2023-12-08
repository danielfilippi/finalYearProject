using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionOptimiser.Calculators
{
    internal class FinancialCalculators
    {
        //MAXWINLOSSDEBCREDMARGIN

        public static double FindNetDebCred(double OptionValueWithIV, char LongShort)
        {
            if (LongShort == 'L') return -OptionValueWithIV; //buy option you are giving money
            else return OptionValueWithIV;  //selling option you are receiving money
        }
        public static double FindMaxProfit(char LongShort, char PutCall, double Strike, double OptionValueWithIV) //for 1 share, not 100
        {
            if (LongShort == 'L')
            {
                if (PutCall == 'C') return double.PositiveInfinity; //Unlimited
                else return Strike - OptionValueWithIV;
            }
            else return OptionValueWithIV;
        }
        public static double FindMaxLoss(char LongShort, char PutCall, double Strike, double OptionValueWithIV) //Just copy above function and change L to S. Put-call parity
        {
            if (LongShort == 'S')
            {
                if (PutCall == 'C') return double.NegativeInfinity; //Unlimited
                else return Strike - OptionValueWithIV;
            }
            else return OptionValueWithIV;
        }
        public static double FindBreakEven(double OptionValueWithIV, double Strike, bool call)
        {
            if (call) return Strike + OptionValueWithIV;
            else return Strike - OptionValueWithIV;
        }

        public static double CalcPossibleMargin(char LongShort, char PutCall, double Spot, double Strike)//margin possible if user doesnt own underlying asset
        {
            double Margin = 0;
            Console.WriteLine("Enter your desired base margin percentage: Represented as a number between 0-1");
            double UserSetMargin = double.Parse(Console.ReadLine());
            Console.WriteLine("Enter your desired additional margin percentage: Represented as a number between 0-1");
            double UserSetAdditionalMargin = double.Parse(Console.ReadLine());
            if (LongShort == 'S')
            {
                Margin += Spot * UserSetMargin;
                if (PutCall == 'P' && Strike > Spot || PutCall == 'C' && Strike < Spot)
                {
                    double outOfTheMoneyAmt = Math.Abs(Spot - Strike);
                    Margin += outOfTheMoneyAmt * UserSetAdditionalMargin;
                }
            }
            return Margin;
        }


    }
}
