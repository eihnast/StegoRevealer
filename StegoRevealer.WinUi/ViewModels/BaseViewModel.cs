using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StegoRevealer.WinUi.ViewModels
{
    /// <summary>
    /// Базовый класс ViewModel
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;


        public BaseViewModel() { }


        // Оповещения об изменении полей

        protected virtual void OnPropertyChanged(string propertyName = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
