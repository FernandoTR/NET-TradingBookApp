namespace Application.Interfaces;

public interface IQrCodeGeneratorService
{
    string GenerateQrCodeSvg(string content);
}
