using System.Collections;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao
{
    // TODO: общее описание параметров и принципов их формирования и влияния на метод Коха-Жао
    /*
     
     */

    /// <summary>
    /// Параметры метода Коха-Жао
    /// </summary>
    public class KochZhaoParameters : StegoMethodParams, IParams
    {
        private const int BlockSize = 8;  // Линейный размер блока матрицы ДКП
        private const int BlockPixelsNum = BlockSize * BlockSize;  // Количестве пикселей в блоке матрицы ДКП

        /// <summary>
        /// Возвращает размер блока
        /// </summary>
        public int GetBlockSize() => BlockSize;

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
                    _dataAsBitArray = StringBitsTools.StringToBitArray(value);
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
                    _toExtractBitLength = CalcUsingBlocksNum();
                return _toExtractBitLength;
            }
            set
            {
                _toExtractBitLength = value;
            }
        }

        /// <inheritdoc/>
        public override int ToExtractColorBytesNum { get { return GetNeededToHideColorBytesNum(ToExtractBitLength); } }


        /// <inheritdoc/>
        public override UniqueList<ImgChannel> Channels { get; }
            = new UniqueList<ImgChannel> { ImgChannel.Blue };


        private ScImageBlocks _imgBlocks;

        /// <summary>
        /// Возвращает матрицу блоков
        /// </summary>

        public ScImageBlocks GetImgBlocksGrid() => _imgBlocks;

        /// <summary>
        /// Матрица блоков изображения
        /// </summary>
        public ScImageBlocks ImgBlocksGrid { get { return _imgBlocks; } }

        /// <summary>
        /// Порог для разницы коэффициентов скрытия
        /// </summary>
        public double Threshold { get; set; } = 120;


        /// <inheritdoc/>
        public override bool InterlaceChannels { get; set; } = true;

        /// <inheritdoc/>
        public override bool VerticalHiding { get; set; } = false;


        // Стартовые индексы

        private StartValues _startBlocks = GetDefaultStartBlocks();

        /// <summary>
        /// Стартовые блоки процессов скрытия/извлечения
        /// </summary>
        public StartValues StartBlocks
        {
            get
            {
                return new(_startBlocks);
            }
            set
            {
                if (value.Length != Channels.Count)
                    throw new ArgumentException("Number of StartPixels is not equal Channels number");
                _startBlocks = value;
            }
        }

        /// <inheritdoc/>
        public override StartValues StartPoints
        {
            get { return StartBlocks; }
            set { StartBlocks = value; }
        }

        /// <summary>
        /// Коэффициенты матрицы ДКП для скрытия
        /// </summary>
        public ScIndexPair HidingCoeffs { get; set; } = HidingCoefficients.Coeff45;


        /// <summary>
        /// Возвращает стандарный список стартовых пикселей
        /// </summary>
        private static StartValues GetDefaultStartBlocks()
        {
            return new StartValues(
                (ImgChannel.Blue, 0)
            );
        }


        public KochZhaoParameters(ImageHandler imgHandler) : base(imgHandler)
        {
            _imgBlocks = new ScImageBlocks(this);
        }


        /// <inheritdoc/>
        public override void Reset()
        {
            Seed = null;
            InterlaceChannels = true;

            Channels.Clear();
            Channels.Append(ImgChannel.Blue);

            _toExtractBitLength = 0;
            Data = "";

            _imgBlocks = new ScImageBlocks(this);
            _startBlocks = GetDefaultStartBlocks();
        }


        /// <summary>
        /// Количество цветовых байт, которое необходимо для сокрытия (извлечения) всей информации (с учётом размера блоков)
        /// </summary>
        public override int GetNeededToHideColorBytesNum(int? dataBitLength = null)
        {
            if (dataBitLength is null)
                dataBitLength = DataBitLength;
            return Convert.ToInt32(Math.Round((double)dataBitLength * BlockPixelsNum, MidpointRounding.ToPositiveInfinity));
        }

        /// <summary>
        /// Количество блоков, которое необходимо для сокрытия (извлечения) всей информации (с учётом размера блоков)
        /// </summary>
        public int GetNeededToHideBlocksNum(int? dataBitLength = null)
        {
            if (dataBitLength is null)
                dataBitLength = DataBitLength;
            var allBlocksNum = GetAllBlocksNum();
            return dataBitLength.Value >= allBlocksNum ? allBlocksNum : dataBitLength.Value;
        }

        /// <summary>
        /// Количество доступных для скрытия блоков (с учётом стартовых)
        /// </summary>
        private int CalcUsingBlocksNum()
        {
            int allBlocksInChannelNum = _imgBlocks.BlocksInRow * _imgBlocks.BlocksInRow;

            int blocksNum = 0;
            foreach (var channel in Channels)
            {
                int startBlock = StartBlocks[channel];
                blocksNum += (allBlocksInChannelNum - (startBlock + 1));
            }

            return blocksNum;
        }

        /// <summary>
        /// Число используемых блоков с учётом заданных стартовых пикселей
        /// </summary>
        public int GetUsingBlocksNum() => CalcUsingBlocksNum();

        /// <summary>
        /// Количество всех блоков (с учётом всех каналов)
        /// </summary>
        public int GetAllBlocksNum()
        {
            return Channels.Count * (_imgBlocks.BlocksInRow * _imgBlocks.BlocksInRow);
        }
    }
}
