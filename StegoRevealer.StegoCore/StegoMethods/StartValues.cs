using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.StegoMethods
{
    public class StartValues
    {
        private Dictionary<ImgChannel, int> _values;
        private Dictionary<int, ImgChannel> _keysOrder = new();
        private int _lastKey = -1;

        public int Length { get { return _values.Count; } }


        public StartValues()
        {
            _values = new();
        }

        public StartValues(StartValues oldStartValues)
        {
            _values = new(oldStartValues._values);
            _lastKey = oldStartValues._lastKey;
            _keysOrder = new(oldStartValues._keysOrder);
        }

        public StartValues(params KeyValuePair<ImgChannel, int>[] startValues)
        {
            _values = new();
            foreach (var startValue in startValues)
                this[startValue.Key] = startValue.Value;
        }

        public StartValues(params (ImgChannel, int)[] startValues)
        {
            _values = new();
            foreach (var (channel, val) in startValues)
                this[channel] = val;
        }


        public static StartValues GetZeroStartValues(ICollection<ImgChannel> channels)
        {
            StartValues startValues = new();
            foreach (var channel in channels)
                startValues[channel] = 0;
            return startValues;
        }


        public int this[ImgChannel channel]
        {
            get
            {
                return _values[channel];
            }
            set
            {
                if (!_values.ContainsKey(channel))
                {
                    _values.Add(channel, value);

                    _lastKey++;
                    _keysOrder.Add(_lastKey, channel);
                }
                else
                    _values[channel] = value;
            }
        }

        public int this[int index]
        {
            get
            {
                return this[_keysOrder[index]];
            }
            set
            {
                this[_keysOrder[index]] = value;
            }
        }
    }
}
