using System.Collections.Generic;
using System.Threading.Tasks;
using ANPR.Models;

namespace ANPR.AppServices.Repository
{
    public interface IDetectCharsRepository
    {
        Task<bool> LoadKNNDataAndTrainKNN();
        Task<List<PossiblePlate>> DetectCharsInPlates(List<PossiblePlate> listOfPossiblePlates);
        bool CheckIfPossibleChar(PossibleChar possibleChar);
        Task<List<List<PossibleChar>>> FindListOfListsOfMatchingChars(List<PossibleChar> listOfPossibleChars);
        double DistanceBetweenChars(PossibleChar firstChar, PossibleChar secondChar);
    }
}