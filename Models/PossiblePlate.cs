using Emgu.CV;
using Emgu.CV.Structure;

namespace ANPR.Models
{
    public class PossiblePlate
    {
        #region Properties
        public Mat ImgPlate { get; set; }
        public Mat ImgGrayscale;
        public Mat ImgThresh;

        public RotatedRect RrLocationOfPlateInScene { get; set; }

        public string StrChars { get; set; }
        #endregion

        #region Constractor
        public PossiblePlate()
        {
            ImgPlate = new Mat();
            ImgGrayscale = new Mat();
            ImgThresh = new Mat();

            RrLocationOfPlateInScene = new RotatedRect();

            StrChars = "";
        }
        #endregion
    }
}
