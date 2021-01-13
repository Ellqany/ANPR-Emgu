using System.Collections.Generic;
using System.Threading.Tasks;
using ANPR.Models;
using Emgu.CV;

namespace ANPR.AppServices.Repository
{
    public interface IDetectPlatesRepository
    {
        Task<List<PossiblePlate>> DetectPlatesInScene(Mat imgOriginalScene);
    }
}