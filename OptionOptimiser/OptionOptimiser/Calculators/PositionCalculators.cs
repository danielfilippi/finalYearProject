using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptionOptimiser.Calculators
{
    internal class PositionCalculators
    {
        public static double CalcMaxWin(double TotMaxWin, double NewMaxWin)
        {
            if (NewMaxWin == double.PositiveInfinity) return double.PositiveInfinity;
            else return TotMaxWin += NewMaxWin;
        }
        public static double CalcMaxLoss(double TotMaxLoss, double NewMaxLoss)
        {
            if (NewMaxLoss == double.NegativeInfinity) return double.NegativeInfinity;
            else return TotMaxLoss += NewMaxLoss;
        }
        public static double CalcNetDebCred(double TotNetDebCred, double NewNetDebCred)
        {
            return TotNetDebCred + NewNetDebCred;
        }
        public static double CalcNetEstMargin(double TotMargin, double NewMargin) 
        {
            return TotMargin + NewMargin;
        }
        public static double CalcTotalDelta(double TotDelta, double NewDelta)
        {
             return TotDelta + NewDelta;
        }
        public static double CalcTotalGamma(double TotGamma, double NewGamma)
        {
            return TotGamma + NewGamma;
        }
        public static double CalcTotalVega(double TotVega, double NewVega)
        {
            return TotVega + NewVega;
        }
        public static double CalcTotalTheta(double TotTheta, double NewTheta)
        {
            return TotTheta + NewTheta;
        }
        public static double CalcTotalRho(double TotRho, double NewRho)
        {
            return TotRho + NewRho;
        }

    }
}
