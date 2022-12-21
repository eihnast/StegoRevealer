using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.StegoMethods;

namespace StegoRevealer.StegoCore.ImageHandlerLib.Blocks
{
    public class ImageBlocks
    {
        private ImageBlocksParameters _parameters;  // Параметры для разбиения на блоки
        private ImageHandler _img;  // Изображение

        // Матрица блоков: хранятся индексы левого верхнего и правого нижнего пикселя каждого блока
        private BlockCoords[,] _blocksMatrix;


        private int _blocksInRow = 0;

        /// <summary>
        /// Количество блоков в строке - ширина матрицы блоков
        /// </summary>
        public int BlocksInRow { get { return _blocksInRow; } }


        private int _blocksInColumn = 0;

        /// <summary>
        /// Количество блоков в столбце - высота матрицы блоков
        /// </summary>
        public int BlocksInColumn { get { return _blocksInColumn; } }


        /// <summary>
        /// Общее количество блоков
        /// </summary>
        public int BlocksNum { get { return _blocksInColumn * _blocksInRow; } }


        #pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public ImageBlocks(ImageBlocksParameters parameters)
        {
            _parameters = parameters;
            _img = _parameters.Image;
            UpdateMatrix();
        }
        #pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.


        /// <summary>
        /// Обновление матрицы блоков с изменёнными параметрами
        /// </summary>
        public void UpdateMatrix(bool updateImage = false)
        {
            if (updateImage)
                _img = _parameters.Image;

            // Размеры матрицы блоков
            _blocksInRow = _img.Width / _parameters.BlockWidth;
            _blocksInColumn = _img.Height / _parameters.BlockHeight;

            // Учёт возможных неполных блоков, если не установлен флаг учёта только целых блоков
            if (!_parameters.OnlyWholeBlocks)
            {
                if (_img.Width % _parameters.BlockWidth != 0)
                    _blocksInRow++;
                if (_img.Height % _parameters.BlockHeight != 0)
                    _blocksInColumn++;
            }

            // Формирование матрицы блоков
            _blocksMatrix = new BlockCoords[_blocksInColumn, _blocksInRow];
            for (int y = 0; y < _blocksInColumn; y++)
            {
                for (int x = 0; x < _blocksInRow; x++)
                {
                    // Левая верхняя координата всегда существует при обходе
                    // Но если блок последний, его реальная ширина и длина могут быть меньше BlockHeight и BlockWidth -
                    // в том случае, если выключен OnlyWholeBlocks

                    var lt = new Sc2DPoint(y * _parameters.BlockHeight, x * _parameters.BlockWidth);
                    var rd = new Sc2DPoint(
                        Math.Min(lt.Y + _parameters.BlockHeight, _img.Height - 1),
                        Math.Min(lt.X + _parameters.BlockWidth, _img.Width - 1)
                        );
                    _blocksMatrix[y, x] = new BlockCoords(lt, rd);
                }
            }
        }


        // Индексаторы

        /// <summary>
        /// Доступ по индексатору матрицы блоков
        /// </summary>
        public BlockCoords this[int y, int x]
        {
            get { return _blocksMatrix[y, x]; }
        }

        /// <summary>
        /// Доступ по индексатору списка блоков - т.е. по линейному индексу блока
        /// </summary>
        public BlockCoords this[int ind]
        {
            get 
            {
                int y = ind / _blocksInRow;
                int x = ind % _blocksInRow;
                return _blocksMatrix[y, x];
            }
        }
    }
}
