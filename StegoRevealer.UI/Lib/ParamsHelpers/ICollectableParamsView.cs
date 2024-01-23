namespace StegoRevealer.UI.Lib.ParamsHelpers;

/// <summary>
/// View параметров метода, обеспечивающий их сбор с представления в объект параметров
/// </summary>
public interface ICollectableParamsView
{
    /// <summary>
    /// Собрать параметры с представления в объект
    /// </summary>
    public object CollectParameters();
}
