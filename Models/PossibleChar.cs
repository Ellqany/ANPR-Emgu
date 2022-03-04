using OpenCvSharp;
using System;

namespace ANPRCV.Models
{
    public class PossibleChar
    {
        #region Properties
        public Point[] Contour { get; set; }
        public Rect BoundingRect { get; set; }

        public int IntCenterX { get; set; }
        public int IntCenterY { get; set; }

        public double DblDiagonalSize { get; set; }
        public double DblAspectRatio { get; set; }
        public int IntRectArea { get; set; }
        #endregion

        #region Constractor
        public PossibleChar(Point[] _contour)
        {
            Contour = _contour;

            BoundingRect = Cv2.BoundingRect(Contour);

            IntCenterX = Convert.ToInt32((BoundingRect.Left + BoundingRect.Right) / (double)2);
            IntCenterY = Convert.ToInt32((BoundingRect.Top + BoundingRect.Bottom) / (double)2);

            DblDiagonalSize = Math.Sqrt((Math.Pow(BoundingRect.Width, 2)) + (Math.Pow(BoundingRect.Height, 2)));

            DblAspectRatio = Convert.ToDouble(BoundingRect.Width) / Convert.ToDouble(BoundingRect.Height);

            IntRectArea = BoundingRect.Width * BoundingRect.Height;
        }
        #endregion
    }
}
