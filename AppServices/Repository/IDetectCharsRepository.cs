using ANPRCV.Models;
using System.Collections.Generic;

namespace ANPRCV.AppServices.Repository
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