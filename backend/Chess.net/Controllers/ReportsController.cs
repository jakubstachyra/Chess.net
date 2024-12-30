using Logic.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Chess.net.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportsController(IReportService reportService) : Controller
    {
        private readonly IReportService _reportService = reportService;

        [HttpPost("reportPlayer/{userID}")]
        public  async Task<IActionResult> ReportUser(string userID, int gameID)
        {
            var result = await _reportService.ReportUserAsync(userID, gameID);
            if(result)
            {
                return Ok();
            }
            return BadRequest();
        }
        [HttpGet("getFirstActiveReport")]
        public async Task<IActionResult> GetFirstActiveReport()
        {
            var result = await _reportService.GetAllActiveReports();

            if (result == null || !result.Any())
            {
                return NotFound("No active reports found.");
            }

            return Ok(result.First()); 
        }

    }
}
