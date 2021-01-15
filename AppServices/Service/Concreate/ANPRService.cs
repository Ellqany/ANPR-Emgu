using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using ANPR.AppServices.Repository;
using ANPR.Models;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace ANPR.AppServices.Service.Concreate
{
    public class ANPRService : IANPRService
    {
        #region Private Variables
        readonly IDetectPlatesRepository DetectPlates;
        readonly IDetectCharsRepository DetectChars;
        #endregion

        #region Controller
        public ANPRService(IDetectPlatesRepository detectPlates, IDetectCharsRepository detectChars)
        {
            DetectPlates = detectPlates;
            DetectChars = detectChars;
        }
        #endregion

        public bool LoadModule()
        {
            return DetectChars.LoadKNNDataAndTrainKNN();
        }

        public PlateDetectionResult DetectPlate(string imagePath, LoadType type)
        {
            // attempt to open image
            Mat imgOriginalScene =
                (type == LoadType.FromPath) ? OpenImageWithErrorHandling(imagePath) : ImageToEmguImage(imagePath);

            if (imgOriginalScene == null || imgOriginalScene.IsEmpty)
            {
                return new PlateDetectionResult();
            }

            // detect plates
            List<PossiblePlate> listOfPossiblePlates = DetectPlates.DetectPlatesInScene(imgOriginalScene);

            // detect chars in plates
            listOfPossiblePlates = DetectChars.DetectCharsInPlates(listOfPossiblePlates);

            if (listOfPossiblePlates == null || listOfPossiblePlates.Count == 0)
            {
                return new PlateDetectionResult();
            }
            else
            {
                // if we get in here list of possible plates has at leat one plate
                // sort the list of possible plates in DESCENDING order (most number of chars to least number of chars)
                listOfPossiblePlates.Sort((onePlate, otherPlate) => otherPlate.StrChars.Length.CompareTo(onePlate.StrChars.Length));

                // suppose the plate with the most recognized chars
                // (the first plate in sorted by string length descending order)
                // is the actual plate

                PossiblePlate licPlate = listOfPossiblePlates.Where(x => x.StrChars.Length < 7).ToList()[0];
                if (licPlate.StrChars.Length == 0)
                {
                    return new PlateDetectionResult();
                }

                // draw red rectangle around plate
                return ExtractthePoints(licPlate);
            }
        }

        #region Private Methods
        Mat OpenImageWithErrorHandling(string path)
        {
            return CvInvoke.Imread(path, ImreadModes.Color);
        }

        Mat ImageToEmguImage(string path)
        {
            Mat mat = new Mat();
            byte[] buffer = Convert.FromBase64String(path);
            CvInvoke.Imdecode(buffer, ImreadModes.Color, mat);
            return mat;
        }

        PlateDetectionResult ExtractthePoints(PossiblePlate licPlate)
        {
            // get 4 vertices of rotated rect
            var ptfRectPoints = licPlate.RrLocationOfPlateInScene.GetVertices();

            // declare 4 points, integer type
            Point pt0 = new Point(Convert.ToInt32(ptfRectPoints[0].X), Convert.ToInt32(ptfRectPoints[0].Y));
            Point pt1 = new Point(Convert.ToInt32(ptfRectPoints[1].X), Convert.ToInt32(ptfRectPoints[1].Y));
            Point pt2 = new Point(Convert.ToInt32(ptfRectPoints[2].X), Convert.ToInt32(ptfRectPoints[2].Y));
            Point pt3 = new Point(Convert.ToInt32(ptfRectPoints[3].X), Convert.ToInt32(ptfRectPoints[3].Y));

            return new PlateDetectionResult()
            {
                Point1 = pt0,
                Point2 = pt1,
                Point3 = pt2,
                Point4 = pt3,
                Plate = licPlate.StrChars,
                FoundPlate = true
            };
        }
        #endregion
    }
}