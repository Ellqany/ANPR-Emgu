using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace ANPR.AppServices.Repository.Concreate
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

            CvInvoke.GaussianBlur(imgMaxContrastGrayscale, imgBlurred, new Size(GAUSSIAN_BLUR_FILTER_SIZE, GAUSSIAN_BLUR_FILTER_SIZE), 0);       // gaussian blur

            // adaptive threshold to get imgThresh
            CvInvoke.AdaptiveThreshold(imgBlurred, imgThresh, 255.0, AdaptiveThresholdType.GaussianC, ThresholdType.BinaryInv, ADAPTIVE_THRESH_BLOCK_SIZE, ADAPTIVE_THRESH_WEIGHT);
        }

        public Matrix<float> Readfile(Matrix<float> matrix, string FileName)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(matrix.GetType());
            StreamReader streamReader = new StreamReader(FileName);
            matrix = (Matrix<float>)xmlSerializer.Deserialize(streamReader);

            streamReader.Close();
            return matrix;
        }

        #region Private Methods
        Mat ExtractValue(Mat imgOriginal)
        {
            Mat imgHSV = new Mat();
            VectorOfMat vectorOfHSVImages = new VectorOfMat();

            CvInvoke.CvtColor(imgOriginal, imgHSV, ColorConversion.Bgr2Hsv);

            CvInvoke.Split(imgHSV, vectorOfHSVImages);

            var imgValue = vectorOfHSVImages[2];

            return imgValue;
        }

        Mat MaximizeContrast(Mat imgGrayscale)
        {
            Mat imgTopHat = new Mat();
            Mat imgBlackHat = new Mat();
            Mat imgGrayscalePlusTopHat = new Mat();
            Mat imgGrayscalePlusTopHatMinusBlackHat = new Mat();

            Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(-1, -1));

            CvInvoke.MorphologyEx(imgGrayscale, imgTopHat, MorphOp.Tophat, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            CvInvoke.MorphologyEx(imgGrayscale, imgBlackHat, MorphOp.Blackhat, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            CvInvoke.Add(imgGrayscale, imgTopHat, imgGrayscalePlusTopHat);
            CvInvoke.Subtract(imgGrayscalePlusTopHat, imgBlackHat, imgGrayscalePlusTopHatMinusBlackHat);

            return imgGrayscalePlusTopHatMinusBlackHat;
        }
        #endregion
    }
}