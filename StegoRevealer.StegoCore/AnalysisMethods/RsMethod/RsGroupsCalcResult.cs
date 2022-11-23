namespace StegoRevealer.StegoCore.AnalysisMethods.RsMethod
{
    public class RsGroupsCalcResult
    {
        public int Regulars { get; set; } = 0;
        public int Singulars { get; set; } = 0;
        public int RegularsWithInvertedMask { get; set; } = 0;
        public int SingularsWithInvertedMask { get; set; } = 0;
    }
}
