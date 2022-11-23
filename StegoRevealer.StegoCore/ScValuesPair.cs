namespace StegoRevealer.StegoCore
{
    public interface ScValuesPair<T>
    {
        public T FirstValue { get; set; }
        public T SecondValue { get; set; }

        public (T firstValue, T secondValue) AsTuple();
    }
}
