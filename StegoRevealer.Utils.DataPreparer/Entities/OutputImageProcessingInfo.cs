using static StegoRevealer.Utils.DataPreparer.Program;
using StegoRevealer.StegoCore.CommonLib;

namespace StegoRevealer.Utils.DataPreparer.Entities;

public class OutputImageProcessingInfo
{
    public string ImagePath { get; set; } = string.Empty;
    public string OriginImagePath { get; set; } = string.Empty;
    public MinMaxData Diapasone { get; set; } = Constants.Diapasones.First().Value;
    public StegoMethod StegoMethod { get; set; }
    public TraverseType TraverseType { get; set; }
    public int HidedVolumePercent { get; set; }
    public int HiddedBitVolume { get; set; }
    public int TextFileStartIndex { get; set; }
    public string HidedData { get; set; } = string.Empty;
    public int? Seed { get; set; }
}
