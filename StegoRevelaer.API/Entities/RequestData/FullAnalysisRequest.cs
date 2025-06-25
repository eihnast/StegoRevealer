using StegoRevealer.StegoCore.CommonLib.Entities;
using StegoRevealer.StegoCore.ImageHandlerLib;

namespace StegoRevelaer.API.Entities.RequestData;

public class FullAnalysisRequest : BaseAnalysisRequest
{
    public CsaRequest CsaRequest { get; set; } = new CsaRequest();
    public RsRequest RsRequest { get; set; } = new RsRequest();
    public SpaRequest SpaRequest { get; set; } = new SpaRequest();
    public FanRequest FanRequest { get; set; } = new FanRequest();
    public ZcaRequest ZcaRequest { get; set; } = new ZcaRequest();
    public CkzhaRequest CkzhaRequest { get; set; } = new CkzhaRequest();
    public ComplexSsaRequest ComplexSsaRequest { get; set; } = new ComplexSsaRequest();
    public StatmRequest StatmRequest { get; set; } = new StatmRequest();

    public JointAnalysisMethodsParameters CreateParameters(ImageHandler imgHandler)
    {
        var parameters = new JointAnalysisMethodsParameters()
        {
            ChiSquareParameters = CsaRequest.CreateParameters(imgHandler),
            RsParameters = RsRequest.CreateParameters(imgHandler),
            SpaParameters = SpaRequest.CreateParameters(imgHandler),
            FanParameters = FanRequest.CreateParameters(imgHandler),
            ZcaParameters = ZcaRequest.CreateParameters(imgHandler),
            KzhaParameters = CkzhaRequest.CreateParameters(imgHandler),
            ComplexSaMethodParameters = ComplexSsaRequest.CreateParameters(imgHandler),
            StatmParameters = StatmRequest.CreateParameters(imgHandler)
        };

        return parameters;
    }
}
