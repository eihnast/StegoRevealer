namespace StegoRevealer.StegoCore
{
    public class UniqueList<T> : List<T>
    {
        public UniqueList() : base()
        {
        }

        public UniqueList(IEnumerable<T> collection) : base()
        {
            AddRange(collection);
        }

        public UniqueList(int capacity) : base(capacity)
        {
        }

        public new void Add(T item)
        {
            if (!Contains(item))
                base.Add(item);
        }

        public new void AddRange(IEnumerable<T> collection)
        {
            foreach (T item in collection)
                Add(item);
        }
    }
}
