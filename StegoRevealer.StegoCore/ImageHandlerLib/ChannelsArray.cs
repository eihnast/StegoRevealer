namespace StegoRevealer.StegoCore.ImageHandlerLib
{
    public class ChannelsArray
    {
        private readonly ScImage? _img = null;
        private readonly Dictionary<ImgChannel, ImageOneChannelArray?> _channelsArray;
        private bool _channelsStructChanged = true;

        // Количество "активных" каналов (реально используемых структурой)
        private int _channelsCount = 0;
        public int ChannelsCount
        {
            get
            {
                if (_channelsStructChanged)
                {
                    _channelsCount = 0;
                    foreach (var channel in _channelsArray.Values)
                        if (channel is not null && !channel.IsEmpty)
                            _channelsCount++;
                }

                return _channelsCount;
            }
        }

        // Приватный конструктор - заполняет null-ами каналы для _channelsArray
        private ChannelsArray()
        {
            _channelsArray = new Dictionary<ImgChannel, ImageOneChannelArray?>();
            foreach (ImgChannel channel in Enum.GetValues(typeof(ImgChannel)))
                _channelsArray.Add(channel, null);
        }

        public ChannelsArray(ScImage img) : this()
        {
            _img = img;

            foreach (ImgChannel channel in Enum.GetValues(typeof(ImgChannel)))
                CreateChannelArray(channel);
        }

        // Установка массива значений канала
        private void SetChannelData(ImgChannel channel, ImageOneChannelArray data)
        {
            _channelsArray[channel] = data;
            _channelsStructChanged = true;
        }

        // Создание массива одного канала
        private void CreateChannelArray(ImgChannel channel)
        {
            if (_img is null)
                return;

            var channelData = new ImageOneChannelArray(_img, channel);
            SetChannelData(channel, channelData);
        }

        // Получение массива одного канала
        public ImageOneChannelArray? GetChannelArray(ImgChannel channel)
        {
            if (_channelsArray[channel] is null)
                CreateChannelArray(channel);

            return _channelsArray[channel];
        }

        // Возвращает новый массив со значениями нужного индекса по всем каналам
        public ScPixel this[int y, int x]
        {
            get
            {
                if (_img is not null)
                    return _img[y, x];
                else
                    return new ScPixel();
            }
        }

        // Валидация на RGB - наличие непустых трёх каналов RGB
        private bool IsValidRgb()
        {
            if (_channelsArray is null)
                return false;
            if (_channelsArray[ImgChannel.Red] is null || (_channelsArray[ImgChannel.Red]?.IsEmpty ?? false))
                return false;
            if (_channelsArray[ImgChannel.Green] is null || (_channelsArray[ImgChannel.Green]?.IsEmpty ?? false))
                return false;
            if (_channelsArray[ImgChannel.Blue] is null || (_channelsArray[ImgChannel.Blue]?.IsEmpty ?? false))
                return false;
            return true;
        }

        // Проверка наличия канала прозрачности
        private bool HasTransparencyChannel()
        {
            if (_channelsArray is null)
                return false;
            if (_channelsArray[ImgChannel.Alpha] is null || (_channelsArray[ImgChannel.Alpha]?.IsEmpty ?? false))
                return false;
            return true;
        }

        public List<ImgChannel> GetChannelsList()
        {
            var channelsList = new List<ImgChannel>();

            foreach (var channel in _channelsArray)
                if (channel.Value is not null && !channel.Value.IsEmpty)
                    channelsList.Add(channel.Key);

            return channelsList;
        }
    }
}
