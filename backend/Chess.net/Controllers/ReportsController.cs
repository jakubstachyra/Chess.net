﻿using Logic.Interfaces;
using Logic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chess.net.Controllers
{
    [ApiController]
    public class ReportsController(IReportService reportService, IHistoryService historyService) : Controller
    {
        private readonly IReportService _reportService = reportService;

        [HttpPost("reports/users/{userID}")]
        public async Task<IActionResult> ReportUser(string userID, int gameID)
        {
            var result = await _reportService.ReportUserAsync(userID, gameID);
            if (result)
            {
                return Ok();
            }
            return BadRequest();
        }
        [HttpGet("reports/active/first")]
        [Authorize(Roles ="ADMIN")]
        public async Task<IActionResult> GetFirstActiveReport()
        {
            var result = await _reportService.GetAllActiveReports();
            if (result == null || !result.Any())
            {
                return NoContent();
            }

            return Ok(result.First());
        }

        [HttpPatch("reports/{reportID}")]
        [Authorize(Roles ="ADMIN")]
        public async Task<IActionResult> MakeReportResolved(int reportID)
        {
            var result = await _reportService.MakeReportResolved(reportID);

            if(!result)
            {
                return BadRequest();
            }

            return Ok("Report marked as resolved.");
        }
    }
}
