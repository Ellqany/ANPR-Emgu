using System;
using ANPRCV.AppServices.Service;
using ANPRCV.Models;
using Microsoft.AspNetCore.Mvc;

namespace ANPRCV.Controllers
{
    [Route("api/ANPR")]
    // [Authorize]
    public class ANPRController : Controller
    {
        #region Private Variables
        readonly IANPRService ANPRService;
        readonly IOCRService OCRService;
        #endregion

        #region Controller
        public ANPRController(IANPRService aNPRService, IOCRService oCRService)
        {
            ANPRService = aNPRService;
            OCRService = oCRService;
        }
        #endregion

        [HttpPost, Route("Detect")]
        public IActionResult Detect([FromBody] PlateDetectionRequest plate)
        {
            try
            {
                ANPRService.LoadModule();
                return Ok(ANPRService.DetectPlate(plate.ImageUrl, plate.Type));
            }
            catch (Exception e)
            {
                PlateDetectionResult result = new PlateDetectionResult()
                {
                    FoundPlate = false,
                    Plate = e.Message
                };
                return BadRequest(result);
            }
        }

        [HttpPost, Route("GetViolations")]
        public IActionResult GetViolations([FromBody] string Image) => Ok(OCRService.ReadHeatMap(Image));
    }
}