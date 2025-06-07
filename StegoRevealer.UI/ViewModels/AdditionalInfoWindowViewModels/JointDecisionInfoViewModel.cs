using ReactiveUI;
using StegoRevealer.StegoCore.AnalysisMethods.ChiSquareAnalysis;
using StegoRevealer.StegoCore.AnalysisMethods.RsMethod;
using StegoRevealer.UI.Tools.MvvmTools;
using StegoRevealer.UI.ViewModels.BaseViewModels;
using System.Collections.Generic;
using System.Reactive;

namespace StegoRevealer.UI.ViewModels.AdditionalInfoWindowViewModels;

public class JointDecisionInfoViewModel : AdditionalInfoWindowViewModelBaseChild
{
    public JointDecisionInfoViewModel(AdditionalInfoWindowViewModel rootViewModel, InstancesListAccessor viewModelsList) : base(rootViewModel, viewModelsList) { }

    [Experimental]
    public JointDecisionInfoViewModel() : base() { }


    public void ProcessResults(ChiSquareResult csaRes, RsResult rsRes)
    {
        CsaMessageRelativeVolume = csaRes.MessageRelativeVolume * 100;
        RsMessageRelativeVolume = rsRes.MessageRelativeVolume * 100;
    }


    private double _csaMessageRelativeVolume = 0.0;  // В %
    public double CsaMessageRelativeVolume
    {
        get => _csaMessageRelativeVolume;
        set => this.RaiseAndSetIfChanged(ref _csaMessageRelativeVolume, value);
    }

    private double _rsMessageRelativeVolume = 0.0;  // В %
    public double RsMessageRelativeVolume
    {
        get => _rsMessageRelativeVolume;
        set => this.RaiseAndSetIfChanged(ref _rsMessageRelativeVolume, value);
    }


    private static readonly List<(int ZoneId, (double X, double Y)[] Vertices)> Zones = new()
    {
        (1, new (double, double)[] { (0,0), (0.1,0), (0.1,4), (0,4) }),
        (2, new (double, double)[] { (0,0), (0.1,0), (10,30), (0,30) }),
        (3, new (double, double)[] { (0.1,0), (30,0), (30,30) }),
        (4, new (double, double)[] { (0,30), (30,30), (30,80), (0,80) }),
        (5, new (double, double)[] { (0,80), (30,80), (30,100), (0,100) }),
        (6, new (double, double)[] { (95,0), (100,0), (100,30), (95,30) }),
        (7, new (double, double)[] { (95,80), (100,80), (100,100), (95,100) }),
    };

    public static int GetZoneForPoint(double x, double y)
    {
        foreach (var (zoneId, vertices) in Zones)
        {
            if (PointInPolygon(x, y, vertices))
                return zoneId;
        }

        return 0; // Ни в одну зону не попадает
    }

    private static bool PointInPolygon(double x, double y, (double X, double Y)[] poly)
    {
        int n = poly.Length;
        bool inside = false;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            var (xi, yi) = poly[i];
            var (xj, yj) = poly[j];

            bool intersect = ((yi > y) != (yj > y)) &&
                             (x < (xj - xi) * (y - yi) / (yj - yi + 1e-10) + xi);
            if (intersect)
                inside = !inside;
        }

        return inside;
    }

    public string GetDecision()
    {
        var rs = RsMessageRelativeVolume;
        var csa = CsaMessageRelativeVolume;
        string result = string.Empty;

        int zone = GetZoneForPoint(csa, rs);

        switch (zone)
        {
            case 1:
                result = "Сектор О.1: Скрытое сообщение не обнаружено, контейнер пуст";
                break;
            case 2:
                result = $"Сектор О.2: Сообщение занимает около {rs:0}%, встраивание псевдослучайное";
                break;
            case 3:
                result = $"Сектор О.3: Сообщение занимает около {csa:0}%, встраивание последовательное";
                break;
            case 4:
                result = $"Сектор О.4: Сообщение занимает около {rs:0}%, встраивание псевдослучайное";
                break;
            case 5:
                result = $"Сектор О.5: Сообщение занимает не менее 80%, встраивание псевдослучайное";
                break;
            case 6:
                result = "Сектор О.6: Сообщение занимает около 100%, встраивание последовательное";
                break;
            case 7:
                result = "Сектор О.7: Сообщение занимает около 100%, встраивание псевдослучайное";
                break;
            default:
                result = "Однозначного вывода сделать нельзя, точка не попадает ни в один из секторов";
                break;
        }

        return result;
    }
}
