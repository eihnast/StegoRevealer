namespace StegoRevealer.StegoCore.ImageHandlerLib.Blocks
{
    public class ImageBlocksParameters
    {
        /// <summary>
        /// Изображение
        /// </summary>
        public ImageHandler Image { get; }

        /// <summary>
        /// Ширина блока
        /// </summary>
        public int BlockWidth { get; set; }

        /// <summary>
        /// Высота блока
        /// </summary>
        public int BlockHeight { get; set; }

        /// <summary>
        /// Если false - пиксели, остающиеся от деления изображения на блоки, будут преобразовываться в блоки меньших размеров<br/>
        /// Например, для матрицы пикселей (2x10) и блоков (2x4) оставшиеся пиксели будут добавлены в виде блока (2x2)<br/>
        /// Позволяет учесть абсолютно все пиксели изображения
        /// </summary>
        public bool OnlyWholeBlocks { get; set; } = true;


        // Конструкторы

        /// <summary>
        /// Параметры разбиения изображения на блоки
        /// </summary>
        /// <param name="image">Изображение</param>
        public ImageBlocksParameters(ImageHandler image) : this(image, image.Width, 1) { }

        /// <summary>
        /// Параметры разбиения изображения на блоки
        /// </summary>
        /// <param name="image">Изображение</param>
        /// <param name="blockSize">Размер квадратного блока</param>
        public ImageBlocksParameters(ImageHandler image, int blockSize) : this(image, blockSize, blockSize) { }

        /// <summary>
        /// Параметры разбиения изображения на блоки
        /// </summary>
        /// <param name="image">Изображение</param>
        /// <param name="blockWidth">Ширина блока</param>
        /// <param name="blockHeight">Высота блока</param>
        public ImageBlocksParameters(ImageHandler image, int blockWidth, int blockHeight)
        {
            Image = image;
            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
        }
    }
}
