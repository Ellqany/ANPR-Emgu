using OpenCvSharp;

namespace ANPRCV.AppServices.Repository
{
    public interface IPreprocessRepository
    {
        void Start(Mat imgOriginal, ref Mat imgGrayscale, ref Mat imgThresh);
        Mat<float> Readfile(string FileName);
    }
}