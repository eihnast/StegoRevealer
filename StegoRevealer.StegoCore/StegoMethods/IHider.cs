namespace StegoRevealer.StegoCore.StegoMethods
{
    public interface IHider
    {
        public IHideResult Hide(string? data);
    }
}
