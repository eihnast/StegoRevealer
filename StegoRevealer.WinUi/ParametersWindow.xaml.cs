using HandyControl.Data;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.WinUi.Lib;
using StegoRevealer.WinUi.Lib.ParamsHelpers;
using StegoRevealer.WinUi.Views.ParametersViews;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StegoRevealer.WinUi
{
    /// <summary>
    /// Логика взаимодействия для ParametersWindow.xaml
    /// </summary>
    public partial class ParametersWindow : Window
    {
        private object _params;

        public Func<object> ParamsReciever { get; init; }

        public ParametersWindow(AnalyzerMethod analyzerMethod, object methodParameters)
        {
            Type paramsType = Dictionaries.ParamsTypeForAnalyzerMethod[analyzerMethod];
            if (paramsType is null)
                throw new ArgumentException("Не удаётся получить тип параметров");

            try { _params = Convert.ChangeType(methodParameters, paramsType); }
            catch { throw new ArgumentException("Ошибка распознавания объекта параметров метода анализа"); }
            if (_params is null)
                throw new ArgumentException("Не удалось распознать объект параметров метода анализа");

            InitializeComponent();

            object? view;
            if (Dictionaries.MethodParametersToDtoMap.ContainsKey(paramsType))
            {
                var paramsDto = Activator.CreateInstance(Dictionaries.MethodParametersToDtoMap[paramsType], _params);
                view = Activator.CreateInstance(Dictionaries.ParamsViewForAnalyzerMethod[analyzerMethod], paramsDto);
                RootContentControl.Content = view;
            }
            else
            {
                view = Activator.CreateInstance(Dictionaries.ParamsViewForAnalyzerMethod[analyzerMethod]);
                RootContentControl.Content = view;
            }

            var collectableView = view as ICollectableParamsView;
            if (collectableView is not null)
                Closing += (object? sender, CancelEventArgs e) => _params = collectableView.CollectParameters();

            ParamsReciever = () => _params;
        }
    }
}
