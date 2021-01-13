using Emgu.CV;
using Emgu.CV.Util;
using System;
using System.Drawing;

namespace ANPR.Models
{
    public class PossibleChar
    {
        #region Properties
        public VectorOfPoint Contour { get; set; }
        public Rectangle BoundingRect { get; set; }

        public int IntCenterX { get; set; }
        public int IntCenterY { get; set; }

        public double DblDiagonalSize { get; set; }
        public double DblAspectRatio { get; set; }
        public int IntRectArea { get; set; }
        #endregion

        #region Constractor
        public PossibleChar(VectorOfPoint _contour)
        {
            Contour = _contour;

            BoundingRect = CvInvoke.BoundingRectangle(Contour);

            IntCenterX = Convert.ToInt32((BoundingRect.Left + BoundingRect.Right) / (double)2);
            IntCenterY = Convert.ToInt32((BoundingRect.Top + BoundingRect.Bottom) / (double)2);

            DblDiagonalSize = Math.Sqrt((Math.Pow(BoundingRect.Width, 2)) + (Math.Pow(BoundingRect.Height, 2)));

            DblAspectRatio = Convert.ToDouble(BoundingRect.Width) / Convert.ToDouble(BoundingRect.Height);

            IntRectArea = BoundingRect.Width * BoundingRect.Height;
        }
        #endregion
    }
}
