using Avalonia.Threading;
using ReactiveUI;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using StegoRevelaer.API;
using StegoRevelaer.API.Services;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace StegoRevealer.UI.ViewModels.MainWindowViewModels;

public class ApiLauncherViewModel : MainWindowViewModelBaseChild
{
    private ApiHost? _apiHost;

    /// <summary>Запущен ли сервер API</summary>
    public bool IsApiLaunched
    {
        get => _isApiLaunched;
        set => this.RaiseAndSetIfChanged(ref _isApiLaunched, value);
    }
    private bool _isApiLaunched = false;

    /// <summary>Текст кнопки запуска/остановки</summary>
    public string LaunchBtnLabel
    {
        get => _launchBtnLabel;
        set => this.RaiseAndSetIfChanged(ref _launchBtnLabel, value);
    }
    private string _launchBtnLabel = string.Empty;


    // Настройки

    private bool _configHttpsRedirectionEnabled = ApiConfigurator.Settings.HttpsRedirection;
    public bool ConfigHttpsRedirectionEnabled
    {
        get => _configHttpsRedirectionEnabled;
        set
        {
            ApiConfigurator.Settings.HttpsRedirection = value;
            ApiConfigurator.SaveConfig();
            this.RaiseAndSetIfChanged(ref _configHttpsRedirectionEnabled, value);
        }
    }

    private string _configHttpsAddressValue = ApiConfigurator.Settings.HttpsAddress;
    public string ConfigHttpsAddressValue
    {
        get => _configHttpsAddressValue;
        set
        {
            ApiConfigurator.Settings.HttpsAddress = value;
            ApiConfigurator.SaveConfig();
            this.RaiseAndSetIfChanged(ref _configHttpsAddressValue, value);
        }
    }

    private string _configHttpAddressValue = ApiConfigurator.Settings.HttpAddress;
    public string ConfigHttpAddressValue
    {
        get => _configHttpAddressValue;
        set
        {
            ApiConfigurator.Settings.HttpAddress = value;
            ApiConfigurator.SaveConfig();
            this.RaiseAndSetIfChanged(ref _configHttpAddressValue, value);
        }
    }

    // Логи API
    // private readonly StringBuilder _logsBuilder = new();
    private string _logs = string.Empty;

    public string Logs
    {
        get => _logs;
        set => this.RaiseAndSetIfChanged(ref _logs, value);
    }

    public void Push(string line)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // _logsBuilder.AppendLine(line);
            // Logs = _logsBuilder.ToString();
            Logs += line + "\n";
        }, DispatcherPriority.Background);
    }


    // Конструкторы и установка начальных значений

    // Установка стандартных значений
    private void CreateDefaults()
    {
        LaunchBtnLabel = L["ApiLauncherTab.LaunchBtn.Start"];
    }

    public ApiLauncherViewModel(MainWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList)
    {
        CreateDefaults();
    }

    [Experimental]
    public ApiLauncherViewModel() : base()
    {
        CreateDefaults();
    }


    public async Task LaunchApi()
    {
        Logs = string.Empty;
        var apiHost = await Task.Run(() => _apiHost = new ApiHost(Push));
        _apiHost?.Start();
        IsApiLaunched = true;
    }

    public async Task StopApi()
    {
        if (_apiHost is not null)
            await _apiHost.Stop();
        IsApiLaunched = false;
    }
}
