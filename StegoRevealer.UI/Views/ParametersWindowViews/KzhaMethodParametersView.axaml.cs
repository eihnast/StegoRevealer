using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DynamicData;
using DynamicData.Binding;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.UI.Tools;
using StegoRevealer.UI.ViewModels.ParametersWindowViewModels;

namespace StegoRevealer.UI.Views.ParametersWindowViews;

public partial class KzhaMethodParametersView : UserControl
{
    private KzhaMethodParametersViewModel _vm = null!;

    public KzhaMethodParametersView()
    {
        InitializeComponent();

        base.Loaded += KzhaMethodParametersView_Loaded;
    }

    private void KzhaMethodParametersView_Loaded(object? sender, RoutedEventArgs e)
    {
        _vm = CommonTools.GetViewModel<KzhaMethodParametersViewModel>(this.DataContext);

        _vm.SelectedCoeffs.Connect().RefCount()
                .Sort(SelectedCoeffsSorting())
                .Bind(out SelectedCoeffsCollectionView).DisposeMany().Subscribe();
        SelectedCoeffsListBox.ItemsSource = SelectedCoeffsCollectionView;
        _vm.AvailableCoeffs.Connect().RefCount()
                .Sort(AvailableCoeffsSorting())
                .Bind(out AvailableCoeffsCollectionView).DisposeMany().Subscribe();
        AvailableCoeffsListBox.ItemsSource = AvailableCoeffsCollectionView;
    }


    private ReadOnlyObservableCollection<ScIndexPair> SelectedCoeffsCollectionView = null!;
    private static IComparer<ScIndexPair> SelectedCoeffsSorting() => SortExpressionComparer<ScIndexPair>.Ascending(pair => pair.ToString());

    private ReadOnlyObservableCollection<ScIndexPair> AvailableCoeffsCollectionView = null!;
    private static IComparer<ScIndexPair> AvailableCoeffsSorting() => SortExpressionComparer<ScIndexPair>.Ascending(pair => pair.ToString());

    private void SelectedCoeffsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedItems = new List<ScIndexPair>(e.AddedItems.Cast<ScIndexPair>());
        SelectedCoeffsListBox.SelectedItem = null;
        foreach (var item in selectedItems)
            _vm.CoeffToAvailable(item);
    }
    private void AvailableCoeffsListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedItems = new List<ScIndexPair>(e.AddedItems.Cast<ScIndexPair>());
        AvailableCoeffsListBox.SelectedItem = null;
        foreach (var item in selectedItems)
            _vm.CoeffToSelected(item);
    }


    #region Input Filtering

    private void FilterForInteger_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowInteger);
    private void FilterForInteger_TextInput(object? sender, Avalonia.Input.TextInputEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowInteger);
    private async void FilterForInteger_PastingFromClipboard(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => await CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowInteger);

    private void FilterForDouble_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowDouble);
    private void FilterForDouble_TextInput(object? sender, Avalonia.Input.TextInputEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowDouble);
    private async void FilterForDouble_PastingFromClipboard(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => await CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowDouble);

    private void FilterForPositiveInteger_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveInteger);
    private void FilterForPositiveInteger_TextInput(object? sender, Avalonia.Input.TextInputEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveInteger);
    private async void FilterForPositiveInteger_PastingFromClipboard(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => await CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveInteger);

    private void FilterForPositiveDouble_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveDouble);
    private void FilterForPositiveDouble_TextInput(object? sender, Avalonia.Input.TextInputEventArgs e) => CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveDouble);
    private async void FilterForPositiveDouble_PastingFromClipboard(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => await CommonTools.FilterInput(sender, e, Lib.FilterInputStrategy.AllowPositiveDouble);

    #endregion
}
