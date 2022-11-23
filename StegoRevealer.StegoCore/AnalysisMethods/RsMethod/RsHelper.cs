namespace StegoRevealer.StegoCore.AnalysisMethods.RsMethod
{
    public static class RsHelper
    {
        private const int _defaultPixelsGroupLength = 4;

        // Стандартная длина групп пикселей
        public static int DefaultPixelsGroupLength { get; } = _defaultPixelsGroupLength;

        // Стандартная маска флиппинга
        public static int[] DefaultFlippingMask { get; } = new int[_defaultPixelsGroupLength] { 1, 0, 0, 1 };

        // Стандартная функция регулярности
        public static int DefaultRegularityFunction(IList<int> nums)
        {
            int sum = 0;
            for (int i = 0; i < nums.Count - 1; i++)
                sum += Math.Abs(nums[i + 1] - nums[i]);
            return sum;
        }

        // Стандартная функция прямого флиппинга
        public static int DefaultFlipDirect(int value)
        {
            if ((value & 1) > 0)
                return value - 1;
            return value + 1;
        }

        // Стандартная функция обратного флиппинга
        public static int DefaultFlipBack(int value)
        {
            if ((value & 1) > 0)
                return value + 1;
            return value - 1;
        }

        // Стандартная функция нулевого флиппинга
        public static int DefaultFlipNone(int value) => value;


        // Вспомогательные методы реализации метода Regular-Singular

        // Метод инвертирования маски
        public static int[] InvertMask(int[] mask) =>
            mask.Select(x => x * -1).ToArray();

        // Метод получения стандартной инвертированной маски
        public static int[] GetDefaultInvertedMask() =>
            InvertMask(DefaultFlippingMask);

        // Метод определения типа группы
        public static RsGroupType DefineGroupType(int beforeFlippingResult, int afterFlippingResult)
        {
            if (afterFlippingResult > beforeFlippingResult)
                return RsGroupType.Regular;
            if (afterFlippingResult < beforeFlippingResult)
                return RsGroupType.Singular;
            if (afterFlippingResult == beforeFlippingResult)
                return RsGroupType.Unusuable;

            throw new Exception("Error while processing group type definition");
        }
        public static RsGroupType DefineGroupType((int beforeFlippingResult, int afterFlippingResult) regularityResult) =>
            DefineGroupType(regularityResult.beforeFlippingResult, regularityResult.afterFlippingResult);

        // Применить функции флиппинга к группе
        public static int[] ApplyFlipping(int[] group, Func<int, int>[] funcs)
        {
            if (group.Length != funcs.Length)
                throw new ArgumentException("Group length and number of flipping functions should be equal");

            var flippedGroup = new int[group.Length];
            for (int i = 0; i < group.Length; i++)
                flippedGroup[i] = funcs[i](group[i]);
            return flippedGroup;
        }
    }
}
