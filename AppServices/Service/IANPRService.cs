using System.Threading.Tasks;
using ANPR.Models;

namespace ANPR.AppServices.Service
{
    public interface IANPRService
    {
        Task<bool> LoadModule();
        Task<PlateDetectionResult> DetectPlate(string imagePath, LoadType type);
    }
}