using StegoRevealer.StegoCore.AnalysisMethods;
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
    public static readonly Dictionary<AnalysisMethod, Type> ParamsViewForAnalysisMethod = new()
    {
        { AnalysisMethod.ChiSquare, typeof(ChiSqrMethodParametersView) },
        { AnalysisMethod.RegularSingular, typeof(RsMethodParametersView) },
        { AnalysisMethod.KochZhaoAnalysis, typeof(KzhaMethodParametersView) }
    };

    /// <summary>
    /// Класс параметров по методу стегоанализа
    /// </summary>
    public static readonly Dictionary<AnalysisMethod, Type> ParamsTypeForAnalysisMethod = new()
    {
        { AnalysisMethod.ChiSquare, typeof(ChiSquareParameters) },
        { AnalysisMethod.RegularSingular, typeof(RsParameters) },
        { AnalysisMethod.KochZhaoAnalysis, typeof(KzhaParameters) }
    };

    /// <summary>
    /// Класс DTO параметров по методу стегоанализа
    /// </summary>
    public static readonly Dictionary<Type, Type> MethodParametersToDtoMap = new()
    {
        { typeof(ChiSquareParameters), typeof(ChiSqrParamsDto) },
        { typeof(RsParameters), typeof(RsParamsDto) },
        { typeof(KzhaParameters), typeof(KzhaParamsDto) }
    };
}
