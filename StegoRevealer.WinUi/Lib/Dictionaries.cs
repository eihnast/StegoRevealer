using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.WinUi.Views.ParametersViews;
using System;
using System.Collections.Generic;

namespace StegoRevealer.WinUi.Lib
{
    public static class Dictionaries
    {
        public static Dictionary<AnalyzerMethod, Type> ParamsViewForAnalyzerMethod = new()
        {
            { AnalyzerMethod.ChiSquare, typeof(ChiSqrMethodParamsView) },
            { AnalyzerMethod.RegularSingular, typeof(RsMethodParamsView) },
            { AnalyzerMethod.KochZhaoAnalysis, typeof(KzhaMethodParamsView) }
        };

        public static Dictionary<AnalyzerMethod, Type> ParamsTypeForAnalyzerMethod = new()
        {
            { AnalyzerMethod.ChiSquare, typeof(ChiSquareParameters) },
            { AnalyzerMethod.RegularSingular, typeof(RsParameters) },
            { AnalyzerMethod.KochZhaoAnalysis, typeof(KzhaParameters) }
        };
    }
}
