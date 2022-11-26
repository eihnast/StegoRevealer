using System;
using System.Collections.Generic;

namespace StegoRevealer.WinUi.Lib
{
    public class InstancesListAccessor
    {
        public AccessMode AccessMode { get; } = AccessMode.Get;
        private bool HasGetAccess => AccessMode.HasFlag(AccessMode.Get);
        private bool HasSetAccess => AccessMode.HasFlag(AccessMode.Set);


        private InstancesList _instancesList;

        public InstancesListAccessor(InstancesList instancesList, AccessMode accessMode)
        {
            _instancesList = instancesList;
            AccessMode = accessMode;
        }

        public object? GetFirst(Type type)
        {
            if (HasGetAccess)
                return _instancesList.GetFirstByType(type);

            return null;
        }

        public List<object> GetAll(Type type)
        {
            if (HasGetAccess)
                return _instancesList.GetByType(type);
            return new List<object>();
        }

        public void Set(object instance)
        {
            if (HasSetAccess)
                _instancesList.Add(instance);
        }

        public void Remove(object instance)
        {
            if (HasSetAccess)
                _instancesList.DeleteInstance(instance);
        }
    }
}
