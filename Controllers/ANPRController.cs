using System;
using System.Threading.Tasks;
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
        public async Task<IActionResult> Detect(string ImageUrl, LoadType type)
        {
            try
            {
                await ANPRService.LoadModule();
                return Ok(await ANPRService.DetectPlate(ImageUrl, type));
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