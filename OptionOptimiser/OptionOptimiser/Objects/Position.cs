using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OptionOptimiser.Calculators;

namespace OptionOptimiser.Objects
{
    internal class Position
    {
        public string UserSetPositionName;  //For every list [i] is the option identifier, so Quantities[1] and BreakEvenPoints[1] regard the same option
        public List<Option> Options;
        public int NumberOfOptions = 0;
        public List<int> Quantities;
        public List<char> PutCall;
        public List<char> LongShort;
        public List<double> OptionValues;
        public string StrategyName;
        public List<double> BreakEvenPoints;
        public double TotMaxWin = 0;
        public double TotMaxLoss = 0;
        public double TotNetCreditDebit = 0;
        public double TotMargin = 0;
        public List<double> StrikePrices;
        public double Spot;
        public double DeltaOfPosition;
        public double GammaOfPosition;
        public double ThetaOfPosition;
        public double VegaOfPosition;
        public double RhoOfPosition;

        public static DateTime MaturityDate;


        public Position(Option OpeningPosition, string Name)
        {
            UserSetPositionName = Name;
            AddOption(OpeningPosition);
        }

        public void AddOption(Option AddedOption)
        {
            Options.Add(AddedOption);
            PutCall.Add(AddedOption.GetPutCall());
            LongShort.Add(AddedOption.GetLongShort());
            Spot = AddedOption.GetSpot(); //cur asset value
            MaturityDate = AddedOption.GetMaturityDate();
            BreakEvenPoints.Add(AddedOption.GetBreakEvenPoint());
            OptionValues.Add(AddedOption.GetValue());
            SetMaxWinLossDebCredMarg(AddedOption);
            SetGreeks(AddedOption);
            NumberOfOptions++;
        }
        public void RemoveOption(int i) //when click x find i
        {
            Options.RemoveAt(i);
            PutCall.RemoveAt(i);
            LongShort.RemoveAt(i);
            BreakEvenPoints.RemoveAt(i);
            OptionValues.RemoveAt(i);
        }
        public void SetMaxWinLossDebCredMarg(Option AddedOption)
        {
            TotMaxWin = PositionCalculators.CalcMaxWin(TotMaxWin, AddedOption.GetMaxWin());
            TotMaxLoss = PositionCalculators.CalcMaxWin(TotMaxLoss, AddedOption.GetMaxLoss());
            TotNetCreditDebit = PositionCalculators.CalcNetDebCred(TotNetCreditDebit, AddedOption.GetNetDebCred()); //dont really have to do this but oh well.
            TotMargin = PositionCalculators.CalcNetEstMargin(TotMargin, AddedOption.GetMargin());
        }
        public void SetGreeks(Option AddedOption)
        {
            DeltaOfPosition = PositionCalculators.CalcTotalDelta(DeltaOfPosition, AddedOption.GetDelta());
            GammaOfPosition = PositionCalculators.CalcTotalGamma(GammaOfPosition, AddedOption.GetGamma());
            VegaOfPosition = PositionCalculators.CalcTotalVega(VegaOfPosition, AddedOption.GetVega());
            ThetaOfPosition = PositionCalculators.CalcTotalTheta(ThetaOfPosition, AddedOption.GetTheta());
            RhoOfPosition = PositionCalculators.CalcTotalRho(RhoOfPosition, AddedOption.GetRho());
        }
    }
}
