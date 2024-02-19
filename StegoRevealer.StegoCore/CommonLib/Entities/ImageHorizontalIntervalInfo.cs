using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.CommonLib.Entities;

public class ImageHorizontalIntervalInfo
{
    public ImgChannel ImgChannel { get; set; }
    public int RowId { get; set; }
    public int IntervalStartId { get; set; }
    public int IntervalEndId { get; set; }
}
