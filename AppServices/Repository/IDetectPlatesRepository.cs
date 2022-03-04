using ANPRCV.Models;
using OpenCvSharp;
using System.Collections.Generic;

namespace ANPRCV.AppServices.Repository
{
    public interface IDetectPlatesRepository
    {
        List<PossiblePlate> DetectPlatesInScene(Mat imgOriginalScene);
    }
}