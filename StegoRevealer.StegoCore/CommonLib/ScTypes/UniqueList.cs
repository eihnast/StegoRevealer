namespace StegoRevealer.StegoCore.CommonLib.ScTypes
{
    /// <summary>
    /// Коллекция на основе List, обеспечивающая наличие только уникальных элементов<br/>
    /// Уникальность проверяется только по Contains, без сравнения по значению полей объектов
    /// </summary>
    public class UniqueList<T> : List<T>
    {
        public UniqueList() : base() { }

        public UniqueList(IEnumerable<T> collection) : base()
        {
            AddRange(collection);
        }

        public UniqueList(int capacity) : base(capacity) { }


        /// <summary>
        /// Добавить объект в коллекцию
        /// </summary>
        public new void Add(T item)
        {
            if (!Contains(item))
                base.Add(item);
        }

        /// <summary>
        /// Добавить все элементы коллекции в данную
        /// </summary>
        public new void AddRange(IEnumerable<T> collection)
        {
            foreach (T item in collection)
                Add(item);
        }
    }
}
