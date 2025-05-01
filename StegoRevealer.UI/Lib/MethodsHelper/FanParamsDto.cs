using StegoRevealer.StegoCore.AnalysisMethods.FanAnalysis;
using StegoRevealer.UI.Lib.ParamsHelpers;

namespace StegoRevealer.UI.Lib.MethodsHelper;

/// <summary>
/// DTO для параметров стегоаналитического FAN: 
/// <see cref="FanParameters"/>
/// </summary>
public class FanParamsDto : IParamsDto<FanParameters>
{
    public double Threshold { get; set; } = 3.401714170610843;

    public FanParamsDto() { }

    public FanParamsDto(FanParameters parameters)
    {
        Threshold = parameters.Threshold;
    }

    /// <inheritdoc/>
    public void FillParameters(ref FanParameters parameters)
    {
        parameters.Threshold = Threshold;
    }
}
