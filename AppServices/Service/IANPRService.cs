using ANPR.Models;

namespace ANPR.AppServices.Service
{
    public interface IANPRService
    {
        bool LoadModule();
        PlateDetectionResult DetectPlate(string imagePath, LoadType type);
    }
}