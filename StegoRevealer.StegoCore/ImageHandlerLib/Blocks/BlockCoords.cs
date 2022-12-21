using StegoRevealer.StegoCore.CommonLib.ScTypes;

namespace StegoRevealer.StegoCore.ImageHandlerLib.Blocks
{
    /// <summary>
    /// Структура хранения координат пикселей блока
    /// </summary>
    public struct BlockCoords
    {
        /// <summary>
        /// Координаты левого верхнего пикселя блока
        /// </summary>
        public Sc2DPoint lt { get; }

        /// <summary>
        /// Координаты правого нижнего пикселя блока
        /// </summary>
        public Sc2DPoint gt { get; }


        /// <summary>
        /// Создаёт структуру хранения координат пикселей блока
        /// </summary>
        /// <param name="lt">Координаты левого верхнего пикселя блока</param>
        /// <param name="gt">Координаты правого нижнего пикселя блока</param>
        public BlockCoords(Sc2DPoint lt, Sc2DPoint gt)
        {
            this.lt = lt;
            this.gt = gt;
        }
    }
}
