namespace StegoRevealer.StegoCore.StegoMethods
{
    public interface IExtractor
    {
        public IExtractResult Extract(IParams parameters);
        public IExtractResult Extract();
    }
}
