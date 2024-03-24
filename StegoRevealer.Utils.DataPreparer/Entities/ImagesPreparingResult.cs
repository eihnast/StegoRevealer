using System.Collections.Concurrent;

namespace StegoRevealer.Utils.DataPreparer.Entities;

public class ImagesPreparingResult
{
    public ConcurrentBag<OutputImage> OutputImages { get; set; } = new();
    public long ElapsedTime { get; set; } = 0;
}
