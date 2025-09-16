using Avalonia.Controls;
using ReactiveUI;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using StegoRevelaer.API;
using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace StegoRevealer.UI.ViewModels.MainWindowViewModels;

public class ApiLauncherViewModel : MainWindowViewModelBaseChild
{
    private ApiHost? _apiHost;

    /// <summary>Запущен ли сервер API</summary>
    public bool IsApiLaunched
    {
        get => _isApiLaunched;
        set
        {
            this.RaiseAndSetIfChanged(ref _isApiLaunched, value);
            LaunchBtnLabel = _isApiLaunched ? L["ApiLauncherTab.LaunchBtn.Stop"] : L["ApiLauncherTab.LaunchBtn.Start"];
        }
    }
    private bool _isApiLaunched = false;

    /// <summary>Текст кнопки запуска/остановки</summary>
    public string LaunchBtnLabel
    {
        get => _launchBtnLabel;
        set => this.RaiseAndSetIfChanged(ref _launchBtnLabel, value);
    }
    private string _launchBtnLabel = string.Empty;


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
        var apiHost = await Task.Run(() => _apiHost = new ApiHost());
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
