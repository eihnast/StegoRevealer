using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;

namespace StegoRevealer.StegoCore.StegoMethods
{
    public class ScImageBlocks
    {
        private KochZhaoParameters _parameters;
        private ImageHandler _img;

        // Хранятся индексы левого верхнего пикселя каждого блока
        private (int, int)[,] _blocksMatrix;


        private int _blocksInRow = 0;
        public int BlocksInRow { get { return _blocksInRow; } }


        private int _blocksInColumn = 0;
        public int BlocksInColumn { get { return _blocksInColumn; } }


        public int BlocksNum { get { return _blocksInColumn * _blocksInRow; } }


        public ScImageBlocks(KochZhaoParameters parameters)
        {
            _parameters = parameters;
            _img = _parameters.Image;
            _blocksMatrix = new (int, int)[0, 0];  // Загулшка для устранения warning
            UpdateMatrix();
        }

        public void UpdateMatrix(bool updateImage = false)
        {
            if (updateImage)
                _img = _parameters.Image;

            int blockSize = _parameters.GetBlockSize();
            _blocksInRow = _img.ImgArray.Width / blockSize;
            _blocksInColumn = _img.ImgArray.Height / blockSize;

            _blocksMatrix = new (int, int)[_blocksInColumn, _blocksInRow];
            for (int y = 0; y < _img.ImgArray.Height; y += blockSize)
                for (int x = 0; x < _img.ImgArray.Width; x += blockSize)
                    _blocksMatrix[y / blockSize, x / blockSize] = (y, x);
        }

        public (int, int) this[int y, int x]
        {
            get { return _blocksMatrix[y, x]; }
        }
    }
}
