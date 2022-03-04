using ANPRCV.Models;
using OpenCvSharp;
using System;
using System.Collections.Generic;

namespace ANPRCV.AppServices.Repository.Concreate
{
    public class DetectPlatesRepository : IDetectPlatesRepository
    {
        #region Variables
        readonly double PLATE_WIDTH_PADDING_FACTOR = 1.3;
        readonly double PLATE_HEIGHT_PADDING_FACTOR = 1.5;
        readonly IDetectCharsRepository DetectChars;
        readonly IPreprocessRepository Preprocess;

        Scalar SCALAR_WHITE = new Scalar(255.0, 255.0, 255.0);
        Scalar SCALAR_RED = new Scalar(0.0, 0.0, 255.0);
        #endregion

        #region Constractor
        public DetectPlatesRepository(IDetectCharsRepository detectChars, IPreprocessRepository preprocess)
        {
            DetectChars = detectChars;
            Preprocess = preprocess;
        }
        #endregion

        public List<PossiblePlate> DetectPlatesInScene(Mat imgOriginalScene)
        {
            // this will be the return value
            List<PossiblePlate> listOfPossiblePlates = new List<PossiblePlate>();

            Mat imgGrayscaleScene = new Mat();
            Mat imgThreshScene = new Mat();

            // preprocess to get grayscale and threshold images
            Preprocess.Start(imgOriginalScene, ref imgGrayscaleScene, ref imgThreshScene);

            // find all possible chars in the scene,
            // this function first finds all contours, then only includes contours that could be chars (without comparison to other chars yet)
            List<PossibleChar> listOfPossibleCharsInScene = FindPossibleCharsInScene(imgThreshScene);


            // given a list of all possible chars, find groups of matching chars
            // in the next steps each group of matching chars will attempt to be recognized as a plate
            List<List<PossibleChar>> listOfListsOfMatchingCharsInScene = DetectChars.FindListOfListsOfMatchingChars(listOfPossibleCharsInScene);

            foreach (List<PossibleChar> listOfMatchingChars in listOfListsOfMatchingCharsInScene)          // for each group of matching chars
            {
                var possiblePlate = ExtractPlate(imgOriginalScene, listOfMatchingChars);                         // attempt to extract plate

                if (possiblePlate.ImgPlate != null)
                {
                    // add to list of possible plates
                    listOfPossiblePlates.Add(possiblePlate);
                }
            }
            return listOfPossiblePlates;
        }

        #region  Private Methods
        List<PossibleChar> FindPossibleCharsInScene(Mat imgThresh)
        {
            // this is the return value
            List<PossibleChar> listOfPossibleChars = new List<PossibleChar>();
            Mat imgThreshCopy = imgThresh.Clone();
            int intCountOfPossibleChars = 0;


            /* TODO Change to default(_) if this is not a reference type */
            // find all contours
            Cv2.FindContours(imgThreshCopy, out Point[][] contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            // for each contour
            for (int i = 0; i < contours.Length; i++)
            {
                PossibleChar possibleChar = new PossibleChar(contours[i]);

                if (DetectChars.CheckIfPossibleChar(possibleChar))
                {
                    // increment count of possible chars
                    intCountOfPossibleChars += 1;
                    // and add to list of possible chars
                    listOfPossibleChars.Add(possibleChar);
                }
            }

            return listOfPossibleChars;
        }

        PossiblePlate ExtractPlate(Mat imgOriginal, List<PossibleChar> listOfMatchingChars)
        {
            // this will be the return value
            PossiblePlate possiblePlate = new PossiblePlate();

            // sort chars from left to right based on x position
            listOfMatchingChars.Sort((firstChar, secondChar) => firstChar.IntCenterX.CompareTo(secondChar.IntCenterX));

            // calculate the center point of the plate 
            // default listOfMatchingChars.Count - 1
            double dblPlateCenterX = Convert.ToDouble(listOfMatchingChars[0].IntCenterX + listOfMatchingChars[^1].IntCenterX) / 2.0;
            double dblPlateCenterY = Convert.ToDouble(listOfMatchingChars[0].IntCenterY + listOfMatchingChars[^1].IntCenterY) / 2.0;
            Point2f ptfPlateCenter = new Point2f(Convert.ToSingle(dblPlateCenterX), Convert.ToSingle(dblPlateCenterY));

            // calculate plate width and height
            int intPlateWidth = Convert.ToInt32(Convert.ToDouble(listOfMatchingChars[^1].BoundingRect.X + listOfMatchingChars[^1].BoundingRect.Width - listOfMatchingChars[0].BoundingRect.X) * PLATE_WIDTH_PADDING_FACTOR);

            int intTotalOfCharHeights = 0;

            foreach (PossibleChar matchingChar in listOfMatchingChars)
            {
                intTotalOfCharHeights += matchingChar.BoundingRect.Height;
            }

            var dblAverageCharHeight = Convert.ToDouble(intTotalOfCharHeights) / Convert.ToDouble(listOfMatchingChars.Count);

            var intPlateHeight = Convert.ToInt32(dblAverageCharHeight * PLATE_HEIGHT_PADDING_FACTOR);

            // calculate correction angle of plate region
            double dblOpposite = listOfMatchingChars[^1].IntCenterY - listOfMatchingChars[0].IntCenterY;
            double dblHypotenuse = DetectChars.DistanceBetweenChars(listOfMatchingChars[0], listOfMatchingChars[^1]);
            double dblCorrectionAngleInRad = Math.Asin(dblOpposite / dblHypotenuse);
            double dblCorrectionAngleInDeg = dblCorrectionAngleInRad * (180.0 / Math.PI);

            // assign rotated rect member variable of possible plate
            possiblePlate.RrLocationOfPlateInScene = new RotatedRect(ptfPlateCenter, new Size2f(Convert.ToSingle(intPlateWidth), Convert.ToSingle(intPlateHeight)), Convert.ToSingle(dblCorrectionAngleInDeg));

            // final steps are to perform the actual rotation
            Mat imgRotated = new Mat();
            Mat imgCropped = new Mat();

            // get the rotation matrix for our calculated correction angle
            Mat rotationMatrix = Cv2.GetRotationMatrix2D(ptfPlateCenter, dblCorrectionAngleInDeg, 1.0);

            // rotate the entire image
            Cv2.WarpAffine(imgOriginal, imgRotated, rotationMatrix, imgOriginal.Size());

            // crop out the actual plate portion of the rotated image
            Cv2.GetRectSubPix(imgRotated, possiblePlate.RrLocationOfPlateInScene.BoundingRect().Size, possiblePlate.RrLocationOfPlateInScene.Center, imgCropped);

            // copy the cropped plate image into the applicable member variable of the possible plate
            possiblePlate.ImgPlate = imgCropped;

            return possiblePlate;
        }
        #endregion
    }
}