using Application.Interfaces;
using QRCoder;

namespace Infrastructure.Services;

public class QrCodeGeneratorService : IQrCodeGeneratorService
{
    public string GenerateQrCodeSvg(string content)
    {
        using (var qrGenerator = new QRCodeGenerator())
        {
            var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            var qrCodeSvg = new SvgQRCode(qrCodeData);
            return qrCodeSvg.GetGraphic(5);
        }
    }

}