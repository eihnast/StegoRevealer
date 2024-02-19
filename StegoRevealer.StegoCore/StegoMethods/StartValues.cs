using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.StegoMethods;

/// <summary>
/// Словарь стартовых индексов по каналам
/// </summary>
public class StartValues
{
    private Dictionary<ImgChannel, int> _values;  // Стартовые индексы для каналов
    private Dictionary<int, ImgChannel> _keysOrder = new();  // Порядок каналов
    
    private int _lastKey = -1;

    /// <summary>
    /// Длина словаря (количество каналов)
    /// </summary>
    public int Length { get { return _values.Count; } }


    // Конструкторы

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


    /// <summary>
    /// Проверяет, существует ли заданное значение для указанного канала
    /// </summary>
    public bool HasValueForChannel(ImgChannel channel) => _values.ContainsKey(channel);

    /// <summary>
    /// Возвращает список каналов, для которых указаны стартовые значения в текущем хранилище
    /// </summary>
    public List<ImgChannel> GetAddedImgChannels() => _values.Keys.ToList();


    /// <summary>
    /// Возвращает нулевые стартовые индексы для указанных каналов
    /// </summary>
    public static StartValues GetZeroStartValues(ICollection<ImgChannel> channels)
    {
        StartValues startValues = new();
        foreach (var channel in channels)
            startValues[channel] = 0;
        return startValues;
    }

    /// <summary>
    /// Возвращает нулевые стартовые индексы для всех возможных каналов
    /// </summary>
    public static StartValues GetZeroStartValues()
    {
        StartValues startValues = new();
        foreach (ImgChannel channel in Enum.GetValues(typeof(ImgChannel)))
            startValues[channel] = 0;
        return startValues;
    }


    // Доступ по индексаторам и логика обновления значений в словарях

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
