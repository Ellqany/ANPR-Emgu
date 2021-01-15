using System;
using ANPR.AppServices.Service;
using ANPR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ANPR.Controllers
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

        [HttpGet, Route("Detect")]
        public IActionResult Detect(string ImageUrl, LoadType type)
        {
            try
            {
                ANPRService.LoadModule();
                return Ok(ANPRService.DetectPlate(ImageUrl, type));
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet, Route("GetViolations")]
        public IActionResult GetViolations(string Image) => Ok(OCRService.ReadHeatMap(Image));
    }
}