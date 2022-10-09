using StegoRevealer.StegoCore.ImageHandlerLib;
using System.Collections;

namespace StegoRevealer.StegoCore.StegoMethods.KochZhao
{
    /*

     */
    public class KochZhaoParameters : StegoMethodParams, IParams
    {
        private const int BlockSize = 8;  // Линейный размер блока матрицы ДКП
        private const int BlockPixelsNum = BlockSize * BlockSize;  // Количестве пикселей в блоке матрицы ДКП

        public int GetBlockSize() => BlockSize;


        public override bool HidingOperation { get; set; } = true;

        public override int? Seed { get; set; } = null;  // Сид для ГПСЧ при псевдослучайном скрытии (определяет тип скрытия)

        private string _data = "";
        private BitArray _dataAsBitArray = new BitArray(0);
        public override string Data  // Скрываемая информация
        {
            get
            {
                return _data;
            }
            set
            {
                if (HidingOperation)
                {
                    _data = value;
                    _dataAsBitArray = StringBitsTools.StringToBitArray(value);
                }
            }
        }

        public override BitArray DataBitArray { get { return _dataAsBitArray; } }
        public override int DataBitLength { get { return _dataAsBitArray.Length; } }


        // Количество извлекаемых бит информации и цветовых байт изображения
        private int _toExtractBitLength = 0;
        public override int ToExtractBitLength  // Количество извлекаемых бит
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
        public override int ToExtractColorBytesNum { get { return GetNeededToHideColorBytesNum(ToExtractBitLength); } }


        // Переделать - не лист, обеспечивать уникальность
        public override UniqueList<ImgChannel> Channels { get; }  // Каналы, используемые для скрытия
            = new UniqueList<ImgChannel> { ImgChannel.Blue };

        private ScImageBlocks _imgBlocks;
        public ScImageBlocks GetImgBlocksGrid() => _imgBlocks;
        public ScImageBlocks ImgBlocksGrid { get { return _imgBlocks; } }


        public int Threshold { get; set; } = 120;  // Порог для разницы коэффициентов скрытия


        public override bool InterlaceChannels { get; set; } = true;  // Чередовать ли каналы при скрытии (иначе - поканально)

        public override bool VerticalHiding { get; set; } = false;  // Вести скрытие по вертикалям (столбцам пикселей, а не линиям)

        private StartValues _startBlocks = GetDefaultStartBlocks();
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
        public override StartValues StartPoints
        {
            get { return StartBlocks; }
            set { StartBlocks = value; }
        }

        // Коэффициенты матрицы ДКП для скрытия
        public (int coef1, int coef2) HidingCoeffs { get; set; } = HidingCoefficients.Coeff45;


        // Возвращает стандарный список стартовых пикселей
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


        // Количество цветовых байт, которое необходимо для сокрытия (извлечения) всей информации (с учётом размера блоков)
        public override int GetNeededToHideColorBytesNum(int? dataBitLength = null)
        {
            if (dataBitLength is null)
                dataBitLength = DataBitLength;
            return Convert.ToInt32(Math.Round((double)dataBitLength * BlockPixelsNum, MidpointRounding.ToPositiveInfinity));
        }

        // Количество блоков, которое необходимо для сокрытия (извлечения) всей информации (с учётом размера блоков)
        public int GetNeededToHideBlocksNum(int? dataBitLength = null)
        {
            if (dataBitLength is null)
                dataBitLength = DataBitLength;
            var allBlocksNum = GetAllBlocksNum();
            return dataBitLength.Value >= allBlocksNum ? allBlocksNum : dataBitLength.Value;
        }

        // Количество доступных для скрытия блоков (с учётом стартовых)
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

        // Число используемых блоков с учётом заданных стартовых пикселей
        public int GetUsingBlocksNum() => CalcUsingBlocksNum();

        // Количество всех блоков (с учётом всех каналов)
        public int GetAllBlocksNum()
        {
            return Channels.Count * (_imgBlocks.BlocksInRow * _imgBlocks.BlocksInRow);
        }
    }
}
