using Avalonia.Threading;
using ReactiveUI;
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
        _dctBlocks = [];
    }

    [Experimental]
    public KzhaHistoInfoViewModel() : base()
    {
        _dctBlocks = [];
    }


    private List<double[,]> _dctBlocks { get; set; }
    private readonly ConcurrentDictionary<ScIndexPair, double[]> _cachedCSequences = new();

    private CancellationTokenSource? _loadCts;

    public static List<ScIndexPair> IndexPairs { get; } = typeof(HidingCoefficients)
                                                         .GetFields(BindingFlags.Public | BindingFlags.Static)
                                                         .Where(f => f.FieldType == typeof(ScIndexPair))
                                                         .Select(f => (ScIndexPair)f.GetValue(null)!)
                                                         .ToList();

    public static List<ScIndexPair> DefaultIndexPairs { get; } = [HidingCoefficients.Coeff34, HidingCoefficients.Coeff35, HidingCoefficients.Coeff45];


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


    public void CreateFrequencyView(ImageHandler img)
    {
        _dctBlocks = FrequencyViewTools.GetDctBlocks(img).ToList();
        _cachedCSequences.Clear();
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

            if (!_cachedCSequences.TryGetValue(indexPair.Value, out var cachedCSequence))
            {
                cachedCSequence = await Task.Run(() =>
                {
                    var computed = FrequencyViewTools.GetCSequence(_dctBlocks, indexPair.Value);
                    return computed.ToArray();
                }, ct);

                _cachedCSequences.TryAdd(indexPair.Value, cachedCSequence);
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
}
