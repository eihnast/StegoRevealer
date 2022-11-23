namespace StegoRevealer.StegoCore.ImageHandlerLib
{
    public struct ScPointCoords
    {
        public int Y { get; set; }
        public int X { get; set; }
        public int ChannelId { get; set; }

        public ScPointCoords(int y, int x, int channelId)
        {
            Y = y;
            X = x;
            ChannelId = channelId;
        }

        public (int, int, int) AsTuple() => (Y, X, ChannelId);
    }
}
