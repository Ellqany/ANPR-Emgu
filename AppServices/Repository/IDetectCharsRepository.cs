using System.Collections.Generic;
using ANPR.Models;

namespace ANPR.AppServices.Repository
{
    public interface IDetectCharsRepository
    {
        bool LoadKNNDataAndTrainKNN();
        List<PossiblePlate> DetectCharsInPlates(List<PossiblePlate> listOfPossiblePlates);
        bool CheckIfPossibleChar(PossibleChar possibleChar);
        List<List<PossibleChar>> FindListOfListsOfMatchingChars(List<PossibleChar> listOfPossibleChars);
        double DistanceBetweenChars(PossibleChar firstChar, PossibleChar secondChar);
    }
}