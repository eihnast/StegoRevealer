using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.KochZhaoAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.UI.Lib.MethodsHelper;
using StegoRevealer.UI.Views.ParametersWindowViews;
using System;
using System.Collections.Generic;

namespace StegoRevealer.UI.Lib;

/// <summary>
/// Вспомогательные словари соответствий
/// </summary>
public static class Dictionaries
{
    /// <summary>
    /// Класс представления параметров (View) по методу стегоанализа
    /// </summary>
    public static Dictionary<AnalyzerMethod, Type> ParamsViewForAnalyzerMethod = new()
    {
        { AnalyzerMethod.ChiSquare, typeof(ChiSqrMethodParametersView) },
        { AnalyzerMethod.RegularSingular, typeof(RsMethodParametersView) },
        { AnalyzerMethod.KochZhaoAnalysis, typeof(KzhaMethodParametersView) }
    };

    /// <summary>
    /// Класс параметров по методу стегоанализа
    /// </summary>
    public static Dictionary<AnalyzerMethod, Type> ParamsTypeForAnalyzerMethod = new()
    {
        { AnalyzerMethod.ChiSquare, typeof(ChiSquareParameters) },
        { AnalyzerMethod.RegularSingular, typeof(RsParameters) },
        { AnalyzerMethod.KochZhaoAnalysis, typeof(KzhaParameters) }
    };

    /// <summary>
    /// Класс DTO параметров по методу стегоанализа
    /// </summary>
    public static Dictionary<Type, Type> MethodParametersToDtoMap = new()
    {
        { typeof(ChiSquareParameters), typeof(ChiSqrParamsDto) },
        { typeof(RsParameters), typeof(RsParamsDto) },
        { typeof(KzhaParameters), typeof(KzhaParamsDto) }
    };
}
