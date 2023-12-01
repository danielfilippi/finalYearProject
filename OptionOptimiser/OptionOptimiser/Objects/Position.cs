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

        public static DateTime MaturityDate;


        public Position(Option OpeningPosition, string Name)
        {
            UserSetPositionName = Name;
            AddOption(OpeningPosition);
        }

        public void AddOption(Option OpeningPosition)
        {
            Options.Add(OpeningPosition);
            PutCall.Add(OpeningPosition.GetPutCall());
            LongShort.Add(OpeningPosition.GetLongShort());
            Spot = OpeningPosition.GetSpot(); //cur asset value
            MaturityDate = OpeningPosition.GetMaturityDate();
            BreakEvenPoints.Add(OpeningPosition.GetBreakEvenPoint());
            OptionValues.Add(OpeningPosition.GetValue());
            SetMaxWinLossDebCredMarg(NumberOfOptions);
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
        public void SetMaxWinLossDebCredMarg(int i)
        {
            TotMaxWin = PositionCalculators.CalcMaxWin(TotMaxWin, Options[i].GetMaxWin());
            TotMaxLoss = PositionCalculators.CalcMaxWin(TotMaxLoss, Options[i].GetMaxLoss());
            TotNetCreditDebit = PositionCalculators.CalcNetDebCred(TotNetCreditDebit, Options[i].GetNetDebCred()); //dont really have to do this but oh well.
            TotMargin = PositionCalculators.CalcNetEstMargin(TotMargin, Options[i].GetMargin());
        }
       

    }
}
