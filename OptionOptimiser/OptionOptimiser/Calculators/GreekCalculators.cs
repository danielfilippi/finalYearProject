using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptionOptimiser.Objects;

namespace OptionOptimiser.Calculators
{
    internal class GreekCalculators
    {
        //GREEKS
        public static double CalcDelta(double OptionValueWithIV, int Steps, double Spot, double Strike, double RiskFreeRate, double ImpliedVolatility, double TimeToMaturity, char PutCall, char EuroAme, Stock underlying) //rate of change in options price per unit change in underlying price.  Approximated by changing spot price slightly and noting change in option price
        {
            double deltaSpot = 0.01;
            // Calculate the option price for an increased spot price
            double price_up = BinomialCalculators.BinomialWithDividends(Steps, Spot + deltaSpot, Strike, RiskFreeRate, ImpliedVolatility, TimeToMaturity, PutCall, EuroAme, underlying);
            //Calculate Delta
            double delta = (price_up - OptionValueWithIV) / deltaSpot;
            if (delta > 0.99999) return 1;
            else return delta;
        }
        public static double CalcGamma(double OptionValueWithIV, int Steps, double Spot, double Strike, double RiskFreeRate, double ImpliedVolatility, double TimeToMaturity, char PutCall, char EuroAme, double delta, Stock underlying) //Rate of change of Delta for unit change in underlyings value -Approximated by recalculating delta for small changes in asset price and obsering rate of change
        {
            double gammaSpot = 0.01;
            double price_down = BinomialCalculators.BinomialWithDividends(Steps, Spot - gammaSpot, Strike, RiskFreeRate, ImpliedVolatility, TimeToMaturity, PutCall, EuroAme, underlying);
            //calculate Delta for the lower price
            double delta_down = (OptionValueWithIV - price_down) / gammaSpot;
            return (delta - delta_down) / (2 * gammaSpot);
        }
        public static double CalcTheta(double OptionValueWithIV, int Steps, double Spot, double Strike, double RiskFreeRate, double ImpliedVolatility, double TimeToMaturity, char PutCall, char EuroAme, Stock underlying) //Rate of change in options price with respect to time. Time decay. Approximated by changing expiry by a small amount and noting the change in option price
        {
            double thetaTime = 0.1 / 365.25;
            double adjustedTTM = TimeToMaturity - thetaTime;
            return (BinomialCalculators.BinomialWithDividends(Steps, Spot, Strike, RiskFreeRate, ImpliedVolatility, adjustedTTM, PutCall, EuroAme, underlying) - OptionValueWithIV) / thetaTime / 365.25;
        }

        public static double CalcVega(double OptionValueWithIV, int Steps, double Spot, double Strike, double RiskFreeRate, double ImpliedVolatility, double TimeToMaturity, char PutCall, char EuroAme, Stock underlying)//Sensitivity of the option price to changes in volatility of underlying. Approximated by altering volatility and observing change in price
        {
            double vegaVol = 0.01;
            double priceHigherVol = BinomialCalculators.BinomialWithDividends(Steps, Spot, Strike, RiskFreeRate, ImpliedVolatility + vegaVol, TimeToMaturity, PutCall, EuroAme, underlying);
            double vega = (priceHigherVol - OptionValueWithIV) / vegaVol / 100;
            if (vega < 0.000001) return 0;
            else return 0;
        }
        public static double CalcRho(double OptionValueWithIV, int Steps, double Spot, double Strike, double RiskFreeRate, double ImpliedVolatility, double TimeToMaturity, char PutCall, char EuroAme, Stock underlying)//Sensitivity of option price to changes in risk free rate. Approximated by altering rfr and assessing change in option value
        {
            double rhoRate = 0.1;
            double priceHigherRate = BinomialCalculators.BinomialWithDividends(Steps, Spot, Strike, RiskFreeRate + rhoRate, ImpliedVolatility, TimeToMaturity, PutCall, EuroAme, underlying);
            return (priceHigherRate - OptionValueWithIV) / rhoRate / 100;
        }

    }
}
