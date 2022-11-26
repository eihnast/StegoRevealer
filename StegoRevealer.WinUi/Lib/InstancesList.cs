using System;
using System.Collections.Generic;
using System.Linq;

namespace StegoRevealer.WinUi.Lib
{
    public class InstancesList
    {
        private Dictionary<Type, List<object>> _viewModelsInstances = new();

        public void Add(object instance)
        {
            var instanceType = instance.GetType();
            if (_viewModelsInstances.ContainsKey(instanceType))
            {
                _viewModelsInstances[instanceType].Add(instance);
            }
            else
            {
                _viewModelsInstances.Add(instanceType, new List<object>() { instance });
            }
        }

        public List<object> GetByType(Type type)
        {
            if (_viewModelsInstances.ContainsKey(type))
                return _viewModelsInstances[type];
            else
                return new List<object>();
        }

        public object? GetFirstByType(Type type)
        {
            if (_viewModelsInstances.ContainsKey(type) && _viewModelsInstances[type].Count > 0)
                return _viewModelsInstances[type].First();
            return null;
        }

        public int InstancesCount(Type type)
        {
            if (_viewModelsInstances.ContainsKey(type))
                return _viewModelsInstances[type].Count;
            return 0;
        }

        public void DeleteInstance(object instance)
        {
            var type = instance.GetType();
            if (_viewModelsInstances.ContainsKey(type))
            {
                _viewModelsInstances[type].Remove(instance);
                ClearEmptyKey(type);
            }
        }

        private void ClearEmptyKeys()
        {
            var toRemoveList = new List<Type>();
            foreach (var key in _viewModelsInstances.Keys)
                if (_viewModelsInstances[key].Count == 0)
                    toRemoveList.Add(key);

            foreach (var key in toRemoveList)
                _viewModelsInstances.Remove(key);
        }

        private void ClearEmptyKey(Type key)
        {
            if (_viewModelsInstances.ContainsKey(key) && _viewModelsInstances[key].Count == 0)
                _viewModelsInstances.Remove(key);
        }
    }
}
