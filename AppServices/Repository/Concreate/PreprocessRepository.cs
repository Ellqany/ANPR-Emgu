using Newtonsoft.Json;
using OpenCvSharp;
using System.IO;

namespace ANPRCV.AppServices.Repository.Concreate
{
    public class PreprocessRepository : IPreprocessRepository
    {
        #region Variables
        readonly int GAUSSIAN_BLUR_FILTER_SIZE = 5;
        readonly int ADAPTIVE_THRESH_BLOCK_SIZE = 19;
        readonly int ADAPTIVE_THRESH_WEIGHT = 9;
        #endregion

        public void Start(Mat imgOriginal, ref Mat imgGrayscale, ref Mat imgThresh)
        {
            // extract value channel only from original image to get imgGrayscale
            imgGrayscale = ExtractValue(imgOriginal);

            // maximize contrast with top hat and black hat
            Mat imgMaxContrastGrayscale = MaximizeContrast(imgGrayscale);

            Mat imgBlurred = new Mat();

            Cv2.GaussianBlur(imgMaxContrastGrayscale, imgBlurred, new Size(GAUSSIAN_BLUR_FILTER_SIZE, GAUSSIAN_BLUR_FILTER_SIZE), 0);       // gaussian blur

            // adaptive threshold to get imgThresh
            Cv2.AdaptiveThreshold(imgBlurred, imgThresh, 255.0, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.BinaryInv, ADAPTIVE_THRESH_BLOCK_SIZE, ADAPTIVE_THRESH_WEIGHT);
        }

        public Mat<float> Readfile(string FileName)
        {
            var jsonString = File.ReadAllText(FileName);
            float[,] newmatrix = JsonConvert.DeserializeObject<float[,]>(jsonString);
            int rows = newmatrix.GetLength(0);
            int cols = newmatrix.GetLength(1);

            Mat<float> matrix = new Mat<float>(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    matrix.Set(i, j, newmatrix[i, j]);
                }
            }

            return matrix;
        }

        #region Private Methods
        static Mat ExtractValue(Mat imgOriginal)
        {
            Mat imgHSV = new Mat();

            Cv2.CvtColor(imgOriginal, imgHSV, ColorConversionCodes.BGR2HSV);

            Cv2.Split(imgHSV, out Mat[] vectorOfHSVImages);

            var imgValue = vectorOfHSVImages[2];

            return imgValue;
        }

        static Mat MaximizeContrast(Mat imgGrayscale)
        {
            Mat imgTopHat = new Mat();
            Mat imgBlackHat = new Mat();
            Mat imgGrayscalePlusTopHat = new Mat();
            Mat imgGrayscalePlusTopHatMinusBlackHat = new Mat();

            Mat structuringElement = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3), new Point(-1, -1));

            Cv2.MorphologyEx(imgGrayscale, imgTopHat, MorphTypes.TopHat, structuringElement, new Point(-1, -1), 1, BorderTypes.Default, new Scalar());
            Cv2.MorphologyEx(imgGrayscale, imgBlackHat, MorphTypes.BlackHat, structuringElement, new Point(-1, -1), 1, BorderTypes.Default, new Scalar());

            Cv2.Add(imgGrayscale, imgTopHat, imgGrayscalePlusTopHat);
            Cv2.Subtract(imgGrayscalePlusTopHat, imgBlackHat, imgGrayscalePlusTopHatMinusBlackHat);

            return imgGrayscalePlusTopHatMinusBlackHat;
        }
        #endregion
    }
}