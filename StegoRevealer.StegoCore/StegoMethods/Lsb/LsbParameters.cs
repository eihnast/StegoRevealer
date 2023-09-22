using System.Collections;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.StegoMethods.Lsb
{
    /*
     * ПАРАМЕТРЫ МЕТОДА СКРЫТИЯ В НЗБ
     * Изменение Data (скрываемых данных), StartPixels (стартовых пикселей) и LsbNum (количества используемых НЗБ)
     *   вызывает метод RepairStartPixels, который перерасчитывает стартовые пиксели исходя из объёма, необходимого
     *   для скрытия всей информации.
     *   Т.е. если при текущем выборе стартовых пикселей скрыть указанный объём информации невозможно,
     *   стартовые пиксели будут "сдвигаться" к началу, чтобы имелась возможность уместить все данные.
     * (!) Общий принцип: ОБЪЁМ ДАННЫХ ВАЖНЕЕ ВЫБОРА СТАРТОВЫХ ПИКСЛЕЙ.
     * Если объём данных больше либо равен ёмкости контейнера (с учётом числа НЗБ), то используется вся ёмкость.
     * Псевдослучайное скрытие не учитывает ограничения, заданные стартовыми пикселям.
     * Последовательное скрытие не учитывает заданное значение ключа ГПСЧ (Seed).
     */

    /// <summary>
    /// Параметры НЗБ-стеганографии
    /// </summary>
    public class LsbParameters : StegoMethodParams, IParams
    {
        /// <inheritdoc/>
        public override StegoOperationType StegoOperation { get; set; } = StegoOperationType.Hiding;

        /// <inheritdoc/>
        public override int? Seed { get; set; } = null;

        private string _data = "";
        private BitArray _dataAsBitArray = new BitArray(0);

        /// <inheritdoc/>
        public override string Data
        { 
            get 
            {
                return _data; 
            } 
            set
            {
                if (StegoOperation is StegoOperationType.Hiding)
                {
                    _data = value;
                    _dataAsBitArray = StringBitsTools.StringToBitArray(value, linearBitArrays: true);
                    RepairStartPixels();
                }
            }
        }

        /// <inheritdoc/>
        public override BitArray DataBitArray { get { return _dataAsBitArray; } }

        /// <inheritdoc/>
        public override int DataBitLength { get { return _dataAsBitArray.Length; } }


        // Количество извлекаемых бит информации и цветовых байт изображения
        private int _toExtractBitLength = 0;

        /// <inheritdoc/>
        public override int ToExtractBitLength
        {
            get
            {
                if (_toExtractBitLength <= 0)
                    _toExtractBitLength = GetUsedVolumeOverall() * LsbNum;
                return _toExtractBitLength;
            }
            set
            {
                _toExtractBitLength = value;
            }
        }

        /// <inheritdoc/>
        public override int ToExtractColorBytesNum { get { return GetNeededColorBytesNum(ToExtractBitLength); } }


        /// <inheritdoc/>
        public override UniqueList<ImgChannel> Channels { get; }
            = new UniqueList<ImgChannel> { ImgChannel.Red, ImgChannel.Green, ImgChannel.Blue };

        /// <inheritdoc/>
        public override bool InterlaceChannels { get; set; } = true;  // Чередовать ли каналы при скрытии (иначе - поканально)

        /// <inheritdoc/>
        public override TraverseType TraverseType { get; set; } = TraverseType.Horizontal;  // Тип обхода массива пикселей (блоков)


        // Стартовые индексы

        private StartValues _startPixels = GetDefaultStartPixels();

        /// <inheritdoc/>
        public StartValues StartPixels
        {
            get
            {
                return new(_startPixels);
            }
            set
            {
                if (value.Length != Channels.Count)
                    throw new ArgumentException("Number of StartPixels is not equal Channels number");
                _startPixels = value;
                RepairStartPixels();
            }
        }

        /// <inheritdoc/>
        public override StartValues StartPoints
        { 
            get { return StartPixels; }
            set { StartPixels = value; } 
        }


        private int _lsbNum = 1;
        
        /// <summary>
        /// Количество НЗБ (бит), используемых для скрытия
        /// </summary>
        public int LsbNum
        { 
            get
            {
                return _lsbNum;
            }
            set
            {
                _lsbNum = value;
                RepairStartPixels();
            }
        }


        /// <summary>
        /// Возвращает стандарный список стартовых пикселей
        /// </summary>
        private static StartValues GetDefaultStartPixels()
        {
            return new StartValues(
                ( ImgChannel.Red, 0 ),
                ( ImgChannel.Green, 0 ),
                ( ImgChannel.Blue, 0 )
            );
        }


        public LsbParameters(ImageHandler imgHandler) : base(imgHandler) { }


        /// <inheritdoc/>
        public override void Reset()
        {
            Seed = null;
            InterlaceChannels = true;
            LsbNum = 1;

            Channels.Clear();
            Channels.Append(ImgChannel.Red).Append(ImgChannel.Green).Append(ImgChannel.Blue);
            _startPixels = GetDefaultStartPixels();

            _toExtractBitLength = 0;
            Data = "";
        }

        /// <summary>
        /// Метод учитывает количество активных каналов в списке задействованных для скрытия
        /// </summary>
        private int CalcRealContainerVolume()
        {
            var (w, h ,d) = Image.GetImgSizes();
            return w * h * Channels.Count;  // Возвращает количество пикселей
        }

        /// <summary>
        /// Возвращает число пикселей, задействованных в скрытии или извлечении с учётом стартовых пикселей
        /// </summary>
        private int GetUsedVolumeOverall()
        {
            var containerVolume = CalcRealContainerVolume();
            var oneChannelVolume = containerVolume / Channels.Count;
            return GetUsedVolumes(oneChannelVolume).Sum();  // Возвращает количество пикселей
        }

        /// <summary>
        /// Возвращает объём занятого места в каждом канале при текущем выборе стартовых пикселей
        /// </summary>
        private int[] GetUsedVolumes(int oneChannelVolume)
        {
            var usedVolumes = new int[Channels.Count];
            for (int i = 0; i < Channels.Count; i++)
                usedVolumes[i] = oneChannelVolume - (_startPixels[i] + 1);
            return usedVolumes;  // Возвращает количества пикселей
        }

        // Количество цветовых байт, которое необходимо для сокрытия (извлечения) всей информации (с учётом числа НЗБ)
        /// <inheritdoc/>
        public override int GetNeededColorBytesNum(int? dataBitLength = null)
        {
            if (dataBitLength is null)
                dataBitLength = DataBitLength;
            return Convert.ToInt32(Math.Round((double)dataBitLength / LsbNum, MidpointRounding.ToPositiveInfinity));
        }

        /// <summary>
        /// Изменение начальных индексов для обеспечения возможности скрытия всей информации
        /// </summary>
        private void RepairStartPixels()
        {
            if (StegoOperation is StegoOperationType.Extracting || Data is null || Data.Length <= 0)  // Если это параметры извлечения, данных нет
                return;

            // Длина данных, делённая на число используемых НЗБ
            // Т.о. дальше работаем с числом необходимых для скрытия пикселей с учётом количества скрываемых бит на пиксель
            int tempDataNum = GetNeededColorBytesNum();

            var allVolume = CalcRealContainerVolume();  // Объём в пикселях (без учёта числа НЗБ)
            if (tempDataNum > allVolume)  // Сдвигаем в 0, если объём данных не меньше всего контейнера
                _startPixels = StartValues.GetZeroStartValues(Channels);

            // Вычисление заполняемого места при текущем выборе стартовых пикселей
            var oneChannelVolume = allVolume / Channels.Count;
            var usedVolumes = GetUsedVolumes(oneChannelVolume);

            int volumesDifference = DataBitLength - usedVolumes.Sum();
            if (volumesDifference > 0)  // При текущих стартовых индексах все данные не поместятся
            {
                int k = 0;
                int i = 0;

                // Сдвигаем поканально значения стартовых пикселей, чтобы могли поместиться все данные
                while (i < volumesDifference)
                {
                    if (_startPixels[k] > 0)  // Если стартовый пиксель не сдвинут максимально - сдвигаем на 1
                    {
                        _startPixels[k]--;
                        i++;
                    }

                    k++;
                    if (k >= _startPixels.Length)  // Переходим снова к первому каналу (сдвиг поканальный)
                        k = 0;
                }
            }

            // Финальная проверка
            bool isError = false;
            foreach (var channel in Channels)
            {
                if (_startPixels[channel] < 0)
                {
                    isError = true;
                    break;
                }
            }
            if (!isError && GetUsedVolumes(oneChannelVolume).Sum() < tempDataNum)
                isError = true;
            if (isError)
                throw new Exception("There is no able to define correct start pixels values");
        }
    }
}
