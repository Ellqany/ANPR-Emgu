using ANPRCV.Models;
using Microsoft.VisualBasic;
using OpenCvSharp;
using OpenCvSharp.ML;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ANPRCV.AppServices.Repository.Concreate
{
    public class DetectCharsRepository : IDetectCharsRepository
    {
        #region Variables
        // constants for checkIfPossibleChar, this checks one possible char only (does not compare to another char)
        readonly int MIN_PIXEL_WIDTH = 2;
        readonly int MIN_PIXEL_HEIGHT = 8;

        readonly double MIN_ASPECT_RATIO = 0.25;
        readonly double MAX_ASPECT_RATIO = 1.0;

        readonly int MIN_RECT_AREA = 25;

        // constants for comparing two chars
        readonly double MIN_DIAG_SIZE_MULTIPLE_AWAY = 0.3;
        readonly double MAX_DIAG_SIZE_MULTIPLE_AWAY = 5.0;

        readonly double MAX_CHANGE_IN_AREA = 0.5;

        readonly double MAX_CHANGE_IN_WIDTH = 0.8;
        readonly double MAX_CHANGE_IN_HEIGHT = 0.2;

        readonly double MAX_ANGLE_BETWEEN_CHARS = 12.0;

        // other constants
        readonly int MIN_NUMBER_OF_MATCHING_CHARS = 3;
        readonly int RESIZED_CHAR_IMAGE_WIDTH = 40;
        readonly int RESIZED_CHAR_IMAGE_HEIGHT = 50;
        Scalar SCALAR_GREEN = new Scalar(0.0, 255.0, 0.0);
        readonly IPreprocessRepository Preprocess;

        static readonly KNearest kNearest = KNearest.Create();
        #endregion

        #region Constractor
        public DetectCharsRepository(IPreprocessRepository preprocess) => Preprocess = preprocess;
        #endregion

        public bool LoadKNNDataAndTrainKNN()
        {
            if (kNearest.IsTrained())
            {
                return true;
            }
            // note: we effectively have to read the first XML file twice
            // first, we read the file to get the number of rows (which is the same as the number of samples)
            // the first time reading the file we can't get the data yet, since we don't know how many rows of data there are
            // next, reinstantiate our classifications Matrix and training images Matrix with the correct number of rows
            // then, read the file again and this time read the data into our resized classifications Matrix and training images Matrix

            Mat<float> mtxClassifications = Preprocess.Readfile("./wwwroot/ANPR/classifications.json");
            Mat<float> mtxTrainingImages = Preprocess.Readfile( "./wwwroot/ANPR/images.json");

            // close the training images json file
            // train '''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''

            kNearest.DefaultK = 3;
            kNearest.AlgorithmType = KNearest.Types.BruteForce;
            kNearest.IsClassifier = true;

            kNearest.Train(mtxTrainingImages, SampleTypes.RowSample, mtxClassifications);

            // if we got here training was successful so return true
            return true;
        }

        public List<PossiblePlate> DetectCharsInPlates(List<PossiblePlate> listOfPossiblePlates)
        {
            // this is only for showing steps
            Random random = new Random();

            if (listOfPossiblePlates == null)
            {
                return listOfPossiblePlates;
            }
            else if (listOfPossiblePlates.Count == 0)
            {
                // at this point we can be sure list of possible plates has at least one plate
                return listOfPossiblePlates;
            }

            // for each possible plate, this is a big for loop that takes up most of the function
            foreach (PossiblePlate possiblePlate in listOfPossiblePlates)
            {
                // preprocess to get grayscale and threshold images
                Preprocess.Start(possiblePlate.ImgPlate, ref possiblePlate.ImgGrayscale, ref possiblePlate.ImgThresh);

                // upscale size by 60% for better viewing and character recognition
                Cv2.Resize(possiblePlate.ImgThresh, possiblePlate.ImgThresh, new Size(), 1.6, 1.6);

                // threshold again to eliminate any gray areas
                Cv2.Threshold(possiblePlate.ImgThresh, possiblePlate.ImgThresh, 0.0, 255.0, ThresholdTypes.Binary);

                // find all possible chars in the plate,
                // this function first finds all contours, then only includes contours that could be chars (without comparison to other chars yet)
                List<PossibleChar> listOfPossibleCharsInPlate = FindPossibleCharsInPlate(possiblePlate.ImgThresh);

                // given a list of all possible chars, find groups of matching chars within the plate
                List<List<PossibleChar>> listOfListsOfMatchingCharsInPlate = FindListOfListsOfMatchingChars(listOfPossibleCharsInPlate);


                if ((listOfListsOfMatchingCharsInPlate == null))
                {
                    // set plate string member variable to empty string
                    possiblePlate.StrChars = "";
                    // and jump back to top of big for loop
                    continue;
                }
                else if (listOfListsOfMatchingCharsInPlate.Count == 0)
                {
                    // set plate string member variable to empty string
                    possiblePlate.StrChars = "";
                    // and jump back to top of big for loop
                    continue;
                }

                // for each group of chars within the plate
                for (int i = 0; i <= listOfListsOfMatchingCharsInPlate.Count - 1; i++)
                {
                    // sort chars from left to right
                    listOfListsOfMatchingCharsInPlate[i].Sort((oneChar, otherChar) => oneChar.BoundingRect.X.CompareTo(otherChar.BoundingRect.X));

                    // remove inner overlapping chars
                    listOfListsOfMatchingCharsInPlate[i] = RemoveInnerOverlappingChars(listOfListsOfMatchingCharsInPlate[i]);
                }

                // within each possible plate, suppose the longest list of potential matching chars is the actual list of chars
                int intLenOfLongestListOfChars = 0;
                int intIndexOfLongestListOfChars = 0;
                // loop through all the lists of matching chars, get the index of the one with the most chars
                for (int i = 0; i <= listOfListsOfMatchingCharsInPlate.Count - 1; i++)
                {
                    if ((listOfListsOfMatchingCharsInPlate[i].Count > intLenOfLongestListOfChars))
                    {
                        intLenOfLongestListOfChars = listOfListsOfMatchingCharsInPlate[i].Count;
                        intIndexOfLongestListOfChars = i;
                    }
                }

                // suppose that the longest list of matching chars within the plate is the actual list of chars
                List<PossibleChar> longestListOfMatchingCharsInPlate = listOfListsOfMatchingCharsInPlate[intIndexOfLongestListOfChars];

                // perform char recognition on the longest list of matching chars in the plate
                possiblePlate.StrChars = RecognizeCharsInPlate(possiblePlate.ImgThresh, longestListOfMatchingCharsInPlate);
            }

            return listOfPossiblePlates;
        }

        public bool CheckIfPossibleChar(PossibleChar possibleChar)
        {
            // this function is a 'first pass' that does a rough check on a contour to see if it could be a char,
            // note that we are not (yet) comparing the char to other chars to look for a group
            if ((possibleChar.IntRectArea > MIN_RECT_AREA & possibleChar.BoundingRect.Width > MIN_PIXEL_WIDTH & possibleChar.BoundingRect.Height > MIN_PIXEL_HEIGHT & MIN_ASPECT_RATIO < possibleChar.DblAspectRatio & possibleChar.DblAspectRatio < MAX_ASPECT_RATIO))
                return true;
            else
                return false;
        }

        public List<List<PossibleChar>> FindListOfListsOfMatchingChars(List<PossibleChar> listOfPossibleChars)
        {
            // with this function, we start off with all the possible chars in one big list
            // the purpose of this function is to re-arrange the one big list of chars into a list of lists of matching chars,
            // note that chars that are not found to be in a group of matches do not need to be considered further
            // this will be the return value
            List<List<PossibleChar>> listOfListsOfMatchingChars = new List<List<PossibleChar>>();

            // for each possible char in the one big list of chars
            foreach (PossibleChar possibleChar in listOfPossibleChars)
            {
                // find all chars in the big list that match the current char
                List<PossibleChar> listOfMatchingChars = FindListOfMatchingChars(possibleChar, listOfPossibleChars);
                // also add the current char to current possible list of matching chars
                listOfMatchingChars.Add(possibleChar);

                // if current possible list of matching chars is not long enough to constitute a possible plate
                if (listOfMatchingChars.Count < MIN_NUMBER_OF_MATCHING_CHARS)
                {
                    // jump back to the top of the for loop and try again with next char, note that it's not necessary
                    // if we get here, the current list passed test as a "group" or "cluster" of matching chars
                    continue;
                }

                // so add to our list of lists of matching chars
                listOfListsOfMatchingChars.Add(listOfMatchingChars);

                // remove the current list of matching chars from the big list so we don't use those same chars twice,
                // make sure to make a new big list for this since we don't want to change the original big list
                List<PossibleChar> listOfPossibleCharsWithCurrentMatchesRemoved = listOfPossibleChars.Except(listOfMatchingChars).ToList();

                // declare new list of lists of chars to get result from recursive call
                List<List<PossibleChar>> recursiveListOfListsOfMatchingChars = new List<List<PossibleChar>>();

                recursiveListOfListsOfMatchingChars = FindListOfListsOfMatchingChars(listOfPossibleCharsWithCurrentMatchesRemoved);      // recursive call

                foreach (List<PossibleChar> recursiveListOfMatchingChars in recursiveListOfListsOfMatchingChars)
                {
                    // for each list of matching chars found by recursive call
                    // add to our original list of lists of matching chars
                    // jump out of for loop
                    listOfListsOfMatchingChars.Add(recursiveListOfMatchingChars);
                }
                break;
            }

            return listOfListsOfMatchingChars;
        }

        // use Pythagorean theorem to calculate distance between two chars
        public double DistanceBetweenChars(PossibleChar firstChar, PossibleChar secondChar)
        {
            int intX = Math.Abs(firstChar.IntCenterX - secondChar.IntCenterX);
            int intY = Math.Abs(firstChar.IntCenterY - secondChar.IntCenterY);

            return Math.Sqrt((Math.Pow(intX, 2)) + (Math.Pow(intY, 2)));
        }

        #region Private Methods
        List<PossibleChar> FindPossibleCharsInPlate(Mat imgThresh)
        {
            // this will be the return value
            List<PossibleChar> listOfPossibleChars = new List<PossibleChar>();
            var imgThreshCopy = imgThresh.Clone();

            /* TODO Change to default(_) if this is not a reference type */
            // find all contours in plate
            Cv2.FindContours(imgThreshCopy, out Point[][] contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            // for each contour
            for (int i = 0; i < contours.Length; i++)
            {
                PossibleChar possibleChar = new PossibleChar(contours[i]);

                if (CheckIfPossibleChar(possibleChar))
                {
                    // add to list of possible chars
                    listOfPossibleChars.Add(possibleChar);
                }
            }

            return listOfPossibleChars;
        }

        List<PossibleChar> FindListOfMatchingChars(PossibleChar possibleChar, List<PossibleChar> listOfChars)
        {
            // the purpose of this function is, given a possible char and a big list of possible chars,
            // find all chars in the big list that are a match for the single possible char, and return those matching chars
            // this will be the return value as a list
            List<PossibleChar> listOfMatchingChars = new List<PossibleChar>();

            // for each char in big list
            foreach (PossibleChar possibleMatchingChar in listOfChars)
            {
                // if the char we attempting to find matches for is the exact same char as the char in the big list we are currently checking
                // then we should not include it in the list of matches b/c that would end up double including the current char
                // so do not add to list of matches and jump back to top of for loop
                // compute stuff to see if chars are a match
                if (possibleMatchingChar.Equals(possibleChar))
                    continue;

                double dblDistanceBetweenChars = DistanceBetweenChars(possibleChar, possibleMatchingChar);

                double dblAngleBetweenChars = AngleBetweenChars(possibleChar, possibleMatchingChar);

                double dblChangeInArea = Math.Abs(possibleMatchingChar.IntRectArea - possibleChar.IntRectArea) / (double)possibleChar.IntRectArea;

                double dblChangeInWidth = Math.Abs(possibleMatchingChar.BoundingRect.Width - possibleChar.BoundingRect.Width) / (double)possibleChar.BoundingRect.Width;
                double dblChangeInHeight = Math.Abs(possibleMatchingChar.BoundingRect.Height - possibleChar.BoundingRect.Height) / (double)possibleChar.BoundingRect.Height;

                // check if chars match
                if (dblDistanceBetweenChars < (possibleChar.DblDiagonalSize * MAX_DIAG_SIZE_MULTIPLE_AWAY) & dblAngleBetweenChars < MAX_ANGLE_BETWEEN_CHARS & dblChangeInArea < MAX_CHANGE_IN_AREA & dblChangeInWidth < MAX_CHANGE_IN_WIDTH & dblChangeInHeight < MAX_CHANGE_IN_HEIGHT)
                {
                    listOfMatchingChars.Add(possibleMatchingChar);// if the chars are a match, add the current char to list of matching chars
                }
            }

            return listOfMatchingChars;
        }

        // use basic trigonometry (SOH CAH TOA) to calculate angle between chars
        static double AngleBetweenChars(PossibleChar firstChar, PossibleChar secondChar)
        {
            double dblAdj = Convert.ToDouble(Math.Abs(firstChar.IntCenterX - secondChar.IntCenterX));
            double dblOpp = Convert.ToDouble(Math.Abs(firstChar.IntCenterY - secondChar.IntCenterY));

            double dblAngleInRad = Math.Atan(dblOpp / dblAdj);

            double dblAngleInDeg = dblAngleInRad * (180.0 / Math.PI);

            return dblAngleInDeg;
        }

        // if we have two chars overlapping or to close to each other to possibly be separate chars, remove the inner (smaller) char,
        // this is to prevent including the same char twice if two contours are found for the same char,
        // for example for the letter 'O' both the inner ring and the outer ring may be found as contours, but we should only include the char once
        List<PossibleChar> RemoveInnerOverlappingChars(List<PossibleChar> listOfMatchingChars)
        {
            List<PossibleChar> listOfMatchingCharsWithInnerCharRemoved = new List<PossibleChar>(listOfMatchingChars);

            foreach (PossibleChar currentChar in listOfMatchingChars)
            {
                foreach (PossibleChar otherChar in listOfMatchingChars)
                {
                    if (!currentChar.Equals(otherChar))
                    {
                        // if current char and other char have center points at almost the same location . . .
                        if (DistanceBetweenChars(currentChar, otherChar) < (currentChar.DblDiagonalSize * MIN_DIAG_SIZE_MULTIPLE_AWAY))
                        {
                            // if we get in here we have found overlapping chars
                            // next we identify which char is smaller, then if that char was not already removed on a previous pass, remove it
                            if (currentChar.IntRectArea < otherChar.IntRectArea)
                            {
                                if (listOfMatchingCharsWithInnerCharRemoved.Contains(currentChar))
                                {
                                    // then remove current char
                                    listOfMatchingCharsWithInnerCharRemoved.Remove(currentChar);
                                }
                            }
                            else if (listOfMatchingCharsWithInnerCharRemoved.Contains(otherChar))
                            {
                                // then remove other char
                                listOfMatchingCharsWithInnerCharRemoved.Remove(otherChar);
                            }
                        }
                    }
                }
            }

            return listOfMatchingCharsWithInnerCharRemoved;
        }

        // this is where we apply the actual char recognition
        string RecognizeCharsInPlate(Mat imgThresh, List<PossibleChar> listOfMatchingChars)
        {
            // this will be the return value, the chars in the lic plate
            string strChars = "";

            Mat imgThreshColor = new Mat();

            listOfMatchingChars.Sort((oneChar, otherChar) => oneChar.BoundingRect.X.CompareTo(otherChar.BoundingRect.X));   // sort chars from left to right

            Cv2.CvtColor(imgThresh, imgThreshColor, ColorConversionCodes.GRAY2BGR);

            // for each char in plate
            foreach (PossibleChar currentChar in listOfMatchingChars)
            {
                // draw green box around the char
                Cv2.Rectangle(imgThreshColor, currentChar.BoundingRect, SCALAR_GREEN, 2);

                // get ROI image of bounding rect
                Mat imgROItoBeCloned = new Mat(imgThresh, currentChar.BoundingRect);

                // clone ROI image so we don't change original when we resize
                Mat imgROI = imgROItoBeCloned.Clone();

                Mat imgROIResized = new Mat();

                // resize image, this is necessary for char recognition
                Cv2.Resize(imgROI, imgROIResized, new Size(RESIZED_CHAR_IMAGE_WIDTH, RESIZED_CHAR_IMAGE_HEIGHT));

                // declare a Matrix of the same dimensions as the Image we are adding to the data structure of training images
                Mat<float> mtxTemp = new Mat<float>(imgROIResized.Size());

                // declare a flattened (only 1 row) matrix of the same total size
                Mat<float> mtxTempReshaped = new Mat<float>(1, RESIZED_CHAR_IMAGE_WIDTH * RESIZED_CHAR_IMAGE_HEIGHT);

                // convert Image to a Matrix of Singles with the same dimensions
                imgROIResized.ConvertTo(mtxTemp, MatType.CV_32F);

                // flatten Matrix into one row by RESIZED_IMAGE_WIDTH * RESIZED_IMAGE_HEIGHT number of columns
                for (int intRow = 0; intRow <= RESIZED_CHAR_IMAGE_HEIGHT - 1; intRow++)
                {
                    for (int intCol = 0; intCol <= RESIZED_CHAR_IMAGE_WIDTH - 1; intCol++)
                    {
                        var value = mtxTemp.Get<float>(intRow, intCol);
                        mtxTempReshaped.Set(0, (intRow * RESIZED_CHAR_IMAGE_WIDTH) + intCol, value);
                    }
                }

                float sngCurrentChar = kNearest.Predict(mtxTempReshaped);      // finally we can call Predict !!!

                strChars += Strings.ChrW(Convert.ToInt32(sngCurrentChar));      // append current char to full string of chars
            }

            return strChars;
        }
        #endregion
    }
}