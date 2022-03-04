using ANPRCV.Models;

namespace ANPRCV.AppServices.Service
{
    public interface IANPRService
    {
        bool LoadModule();
        PlateDetectionResult DetectPlate(string imagePath, LoadType type);
    }
}