using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.AnalysisMethods.ComplexAnalysis;

public class ComplexSaMethodParameters
{
    /// <summary>
    /// Изображение
    /// </summary>
    public ImageHandler Image { get; set; }


    public ComplexSaMethodParameters(ImageHandler image)
    {
        Image = image;
    }
}
