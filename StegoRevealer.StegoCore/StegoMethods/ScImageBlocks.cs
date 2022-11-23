using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;

namespace StegoRevealer.StegoCore.StegoMethods
{
    public class ScImageBlocks
    {
        private KochZhaoParameters _parameters;
        private ImageHandler _img;

        // Хранятся индексы левого верхнего пикселя каждого блока
        private Sc2DPoint[,] _blocksMatrix;


        private int _blocksInRow = 0;
        public int BlocksInRow { get { return _blocksInRow; } }


        private int _blocksInColumn = 0;
        public int BlocksInColumn { get { return _blocksInColumn; } }


        public int BlocksNum { get { return _blocksInColumn * _blocksInRow; } }


        public ScImageBlocks(KochZhaoParameters parameters)
        {
            _parameters = parameters;
            _img = _parameters.Image;
            _blocksMatrix = new Sc2DPoint[0, 0];  // Загулшка для устранения warning
            UpdateMatrix();
        }

        public void UpdateMatrix(bool updateImage = false)
        {
            if (updateImage)
                _img = _parameters.Image;

            int blockSize = _parameters.GetBlockSize();
            _blocksInRow = _img.ImgArray.Width / blockSize;
            _blocksInColumn = _img.ImgArray.Height / blockSize;

            _blocksMatrix = new Sc2DPoint[_blocksInColumn, _blocksInRow];
            for (int y = 0; y < _img.ImgArray.Height - 1; y += blockSize)
                for (int x = 0; x < _img.ImgArray.Width - 1; x += blockSize)
                    _blocksMatrix[y / blockSize, x / blockSize] = new Sc2DPoint(y, x);
        }

        public Sc2DPoint this[int y, int x]
        {
            get { return _blocksMatrix[y, x]; }
        }
    }
}
