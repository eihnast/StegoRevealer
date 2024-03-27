namespace StegoRevealer.Utils.DataPreparer.Entities;

public class StartParams
{
    public bool SkipPreparing {  get; set; } = false;
    public bool SkipAnalysis{  get; set; } = false;
    public bool UseWeakPoolForCalculations { get; set; } = false;
    public bool ManyHidings { get; set; } = false;
    public bool BasketOperations { get; set; } = false;
}
