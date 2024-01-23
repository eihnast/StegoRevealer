using System.Collections.Generic;
using System;

namespace StegoRevealer.UI.Tools.MvvmTools
{
    /// <summary>
    /// Обёртка над хранилищем <see cref="InstancesList"/>, предоставляющая доступ в определённом режиме <see cref="Lib.AccessMode"/><br/>
    /// После создания Accessor-а изменить режим доступа нельзя
    /// </summary>
    public class InstancesListAccessor
    {
        /// <summary>
        /// Установленный режим доступа
        /// </summary>
        public AccessMode AccessMode { get; } = AccessMode.Get;


        /// <summary>
        /// Проверка наличия доступа на считывание объектов
        /// </summary>
        private bool HasGetAccess => AccessMode.HasFlag(AccessMode.Get);

        /// <summary>
        /// Проверка наличия доступа на изменение (добавление/удаление) объектов
        /// </summary>
        private bool HasSetAccess => AccessMode.HasFlag(AccessMode.Set);


        private InstancesList _instancesList;  // Хранилище объектов


        public InstancesListAccessor(InstancesList instancesList, AccessMode accessMode)
        {
            _instancesList = instancesList;
            AccessMode = accessMode;
        }


        /// <summary>
        /// Возвращает первый объект указанного типа или null, если объектов такого типа нет в хранилище<br/>
        /// Требует доступ на считывание объектов: <see cref="Lib.AccessMode.Get"/>
        /// </summary>
        public object? GetFirst(Type type)
        {
            if (HasGetAccess)
                return _instancesList.GetFirstByType(type);

            return null;
        }

        /// <summary>
        /// Возвращает список всех объектов указанного типа или пустой список, если объектов такого типа нет в хранилище<br/>
        /// Требует доступ на считывание объектов: <see cref="Lib.AccessMode.Get"/>
        /// </summary>
        public List<object> GetAll(Type type)
        {
            if (HasGetAccess)
                return _instancesList.GetByType(type);
            return new List<object>();
        }

        /// <summary>
        /// Добавляет указанный объект в хранилище<br/>
        /// Требует доступ на изменение объектов: <see cref="Lib.AccessMode.Set"/>
        /// </summary>
        public void Add(object instance)
        {
            if (HasSetAccess)
                _instancesList.Add(instance);
        }

        /// <summary>
        /// Удаляет указанный объект из хранилища<br/>
        /// Требует доступ на изменение объектов: <see cref="Lib.AccessMode.Set"/>
        /// </summary>
        public void Remove(object instance)
        {
            if (HasSetAccess)
                _instancesList.DeleteInstance(instance);
        }
    }
}
