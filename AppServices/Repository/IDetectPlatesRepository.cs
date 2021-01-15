using System.Collections.Generic;
using ANPR.Models;
using Emgu.CV;

namespace ANPR.AppServices.Repository
{
    public interface IDetectPlatesRepository
    {
        List<PossiblePlate> DetectPlatesInScene(Mat imgOriginalScene);
    }
}