using Emgu.CV;

namespace ANPR.AppServices.Repository
{
    public interface IPreprocessRepository
    {
         void Start(Mat imgOriginal, ref Mat imgGrayscale, ref Mat imgThresh);
         Matrix<float> Readfile(Matrix<float> matrix, string FileName);
    }
}