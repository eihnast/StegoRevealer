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
                result = L["JointAnalysis.Decisions.O1"];
                break;
            case 2:
                result = string.Format(L["JointAnalysis.Decisions.O2"], rs);
                break;
            case 3:
                result = string.Format(L["JointAnalysis.Decisions.O3"], csa);
                break;
            case 4:
                result = string.Format(L["JointAnalysis.Decisions.O4"], rs);
                break;
            case 5:
                result = L["JointAnalysis.Decisions.O5"];
                break;
            case 6:
                result = L["JointAnalysis.Decisions.O6"];
                break;
            case 7:
                result = L["JointAnalysis.Decisions.O7"];
                break;
            default:
                result = L["JointAnalysis.Decisions.Undefined"];
                break;
        }

        return result;
    }
}
