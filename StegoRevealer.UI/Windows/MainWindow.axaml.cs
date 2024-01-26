using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels;

namespace StegoRevealer.UI.Windows;

public partial class MainWindow : Window
{
    private MainWindowViewModel _vm = null!;
    private List<Button> _headerButtons = null!;

    private SolidColorBrush _selectedBtnBackgroundBrush;
    private SolidColorBrush _unselectedBtnBackgroundBrush;

    public MainWindow()
    {
        _selectedBtnBackgroundBrush = CommonTools.GetBrush("SrDarkMiddle");
        _unselectedBtnBackgroundBrush = CommonTools.GetBrush("SrDark");

        InitializeComponent();
        this.Loaded += MainWindow_Loaded;
        this.Closing += MainWindow_Closing;
    }

    private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<MainWindowViewModel>(this.DataContext);

        _headerButtons = new List<Button> { AnalyzerBtn, HiderBtn, ExtractorBtn, AboutBtn, SettingsBtn };
        UpdateHeaderBtnSelection(AnalyzerBtn);
        _vm.TurnToAnalyzer();
    }

    private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
    {
        Configurator.SaveConfig();
    }

    private void UpdateHeaderBtnSelection(Button selectedBtn)
    {
        foreach (var btn in _headerButtons)
            btn.Background = _unselectedBtnBackgroundBrush;
        selectedBtn.Background = _selectedBtnBackgroundBrush;
    }

    private void AnalyzerBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UpdateHeaderBtnSelection(AnalyzerBtn);
        _vm.TurnToAnalyzer();
    }
    private void HiderBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UpdateHeaderBtnSelection(HiderBtn);
        _vm.TurnToHider();
    }
    private void ExtractorBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UpdateHeaderBtnSelection(ExtractorBtn);
        _vm.TurnToExtractor();
    }
    private void AboutBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UpdateHeaderBtnSelection(AboutBtn);
        _vm.TurnToInfoPage();
    }
    private void SettingsBtn_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        UpdateHeaderBtnSelection(SettingsBtn);
        _vm.TurnToSettingsPage();
    }
}