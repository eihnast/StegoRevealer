using Avalonia.Threading;
using ReactiveUI;
using StegoRevealer.StegoCore.CommonLib;
using StegoRevealer.StegoCore.CommonLib.ScTypes;
using StegoRevealer.StegoCore.ImageHandlerLib;
using StegoRevealer.StegoCore.StegoMethods.KochZhao;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace StegoRevealer.UI.ViewModels.AdditionalInfoWindowViewModels;

public class KzhaHistoInfoViewModel : AdditionalInfoWindowViewModelBaseChild
{
    public KzhaHistoInfoViewModel(AdditionalInfoWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList)
    {
        _cachedDCTBlocks = [];
    }

    [Experimental]
    public KzhaHistoInfoViewModel() : base()
    {
        _cachedDCTBlocks = [];
    }

    private ImageHandler? _img;
    private readonly ConcurrentDictionary<ScIndexPair, double[]> _cachedHorizontalCSequences = new();
    private readonly ConcurrentDictionary<ScIndexPair, double[]> _cachedVerticalCSequences = new();
    private readonly ConcurrentDictionary<TraverseType, List<double[,]>> _cachedDCTBlocks = new();

    private CancellationTokenSource? _loadCts;

    public static List<ScIndexPair> IndexPairs { get; } = typeof(HidingCoefficients)
                                                         .GetFields(BindingFlags.Public | BindingFlags.Static)
                                                         .Where(f => f.FieldType == typeof(ScIndexPair))
                                                         .Select(f => (ScIndexPair)f.GetValue(null)!)
                                                         .ToList();

    public static List<ScIndexPair> DefaultIndexPairs { get; } = [HidingCoefficients.Coeff34, HidingCoefficients.Coeff35, HidingCoefficients.Coeff45];


    private bool _isVerticalTraverseSelected = false;
    public bool IsVerticalTraverseSelected
    {
        get => _isVerticalTraverseSelected;
        set => this.RaiseAndSetIfChanged(ref _isVerticalTraverseSelected, value);
    }


    private List<ScIndexPair> _showingPairs = DefaultIndexPairs;
    public List<ScIndexPair> ShowingPairs
    {
        get => _showingPairs;
        set => this.RaiseAndSetIfChanged(ref _showingPairs, value);
    }

    private bool _useOnlyDefaultPairs = true;
    public bool UseOnlyDefaultPairs
    {
        get => _useOnlyDefaultPairs;
        set
        {
            this.RaiseAndSetIfChanged(ref _useOnlyDefaultPairs, value);
            if (value)
            {
                ShowingPairs = DefaultIndexPairs;
                SelectedIndexPairIndex = 0;
            }
            else
            {
                ShowingPairs = IndexPairs;
            }
        }
    }

    private double[] _cSequence = [];
    public double[] CSequence
    {
        get => _cSequence;
        set => this.RaiseAndSetIfChanged(ref _cSequence, value);
    }

    private int _selectedIndexPairIndex = -1;
    public int SelectedIndexPairIndex
    {
        get => _selectedIndexPairIndex;
        set
        {
            if (UseOnlyDefaultPairs)
            {
                if (value >= 0 && value < DefaultIndexPairs.Count)
                    SelectedIndexPair = DefaultIndexPairs[value];
            }
            else
            {
                if (value >= 0 && value < IndexPairs.Count)
                    SelectedIndexPair = IndexPairs[value];
            }
            this.RaiseAndSetIfChanged(ref _selectedIndexPairIndex, value);
        }
    }

    private ScIndexPair? _selectedIndexPair = null;
    public ScIndexPair? SelectedIndexPair
    {
        get => _selectedIndexPair;
        set
        {
            _ = RedrawHistoAsync(value);
            this.RaiseAndSetIfChanged(ref _selectedIndexPair, value);
        }
    }

    private bool _isLoadingProcess = false;
    public bool IsLoadingProcess
    {
        get => _isLoadingProcess;
        set => this.RaiseAndSetIfChanged(ref _isLoadingProcess, value);
    }


    public async Task CreateFrequencyView(ImageHandler img)
    {
        _img = img;
        await SetHorizontalTraverse();
    }

    private async Task RedrawHistoAsync(ScIndexPair? indexPair)
    {
        if (indexPair is null)
            return;

        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new CancellationTokenSource();
        var ct = _loadCts.Token;

        try
        {
            IsLoadingProcess = true;

            var actualCSequences = IsVerticalTraverseSelected ? _cachedVerticalCSequences : _cachedHorizontalCSequences;
            if (!actualCSequences.TryGetValue(indexPair.Value, out var cachedCSequence))
            {
                cachedCSequence = await Task.Run(() =>
                {
                    var computed = FrequencyViewTools.GetCSequence(
                        IsVerticalTraverseSelected ? _cachedDCTBlocks[TraverseType.Vertical] : _cachedDCTBlocks[TraverseType.Horizontal], 
                        indexPair.Value);
                    return computed.ToArray();
                }, ct);

                actualCSequences.TryAdd(indexPair.Value, cachedCSequence);
            }

            ct.ThrowIfCancellationRequested();

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                CSequence = cachedCSequence;
            });
        }
        catch (OperationCanceledException) { }
        finally
        {
            if (!ct.IsCancellationRequested)
                IsLoadingProcess = false;
        }
    }

    public async Task SetHorizontalTraverse()
    {
        if (_img is null)
            return;

        IsLoadingProcess = true;

        if (!_cachedDCTBlocks.TryGetValue(TraverseType.Horizontal, out var dctBlocks))
        {
            dctBlocks = await Task.Run(() =>
            {
                var dctBlocks = FrequencyViewTools.GetDctBlocks(_img, traverseType: TraverseType.Horizontal).ToList();
                return dctBlocks;
            });
            _cachedDCTBlocks.TryAdd(TraverseType.Horizontal, dctBlocks);
        }

        _ = RedrawHistoAsync(SelectedIndexPair);

        IsLoadingProcess = false;
    }

    public async Task SetVerticalTraverse()
    {
        if (_img is null)
            return;

        IsLoadingProcess = true;

        if (!_cachedDCTBlocks.TryGetValue(TraverseType.Vertical, out var dctBlocks))
        {
            dctBlocks = await Task.Run(() =>
            {
                var dctBlocks = FrequencyViewTools.GetDctBlocks(_img, traverseType: TraverseType.Vertical).ToList();
                return dctBlocks;
            });
            _cachedDCTBlocks.TryAdd(TraverseType.Vertical, dctBlocks);
        }

        _ = RedrawHistoAsync(SelectedIndexPair);

        IsLoadingProcess = false;
    }
}
