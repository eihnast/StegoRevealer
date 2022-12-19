using System;
using System.Collections.Generic;
using System.Linq;

namespace StegoRevealer.WinUi.Lib
{
    /// <summary>
    /// Хранилище объектов<br/>
    /// Структура для хранения объектов по типу класса<br/>
    /// Хранит словарь "Класс" - "Список объектов класса" и реализует работу с ним
    /// </summary>
    public class InstancesList
    {
        private Dictionary<Type, List<object>> _instances = new();  // Словарь объектов

        /// <summary>
        /// Добавление объекта
        /// </summary>
        public void Add(object instance)
        {
            var instanceType = instance.GetType();
            if (_instances.ContainsKey(instanceType))
            {
                _instances[instanceType].Add(instance);
            }
            else
            {
                _instances.Add(instanceType, new List<object>() { instance });
            }
        }

        /// <summary>
        /// Возвращает список объектов указанного класса
        /// </summary>
        public List<object> GetByType(Type type)
        {
            if (_instances.ContainsKey(type))
                return _instances[type];
            else
                return new List<object>();
        }

        /// <summary>
        /// Возвращает первый объект указанного класса
        /// </summary>
        public object? GetFirstByType(Type type)
        {
            if (_instances.ContainsKey(type) && _instances[type].Count > 0)
                return _instances[type].First();
            return null;
        }

        /// <summary>
        /// Возвращает количество объектов указанного класса
        /// </summary>
        public int InstancesCount(Type type)
        {
            if (_instances.ContainsKey(type))
                return _instances[type].Count;
            return 0;
        }

        /// <summary>
        /// Удаляет указанный объект из хранилища
        /// </summary>
        public void DeleteInstance(object instance)
        {
            var type = instance.GetType();
            if (_instances.ContainsKey(type))
            {
                _instances[type].Remove(instance);
                ClearEmptyKey(type);
            }
        }

        /// <summary>
        /// Удаляет из хранилища все записи о классах, не содержащие сохранённых объектов
        /// </summary>
        private void ClearEmptyKeys()
        {
            var toRemoveList = new List<Type>();
            foreach (var key in _instances.Keys)
                if (_instances[key].Count == 0)
                    toRemoveList.Add(key);

            foreach (var key in toRemoveList)
                _instances.Remove(key);
        }

        /// <summary>
        /// Удаляет запись о классе из хранилища, если она не содержит сохранённых объектов
        /// </summary>
        private void ClearEmptyKey(Type key)
        {
            if (_instances.ContainsKey(key) && _instances[key].Count == 0)
                _instances.Remove(key);
        }
    }
}
