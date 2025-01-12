using Chess.net.Services;
using Domain.Common;
using Domain.DTOs;
using Logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("/admin-requests")]
public class AdminRequestsController : ControllerBase
{
    private readonly IAdminRequestService _adminRequestService;

    public AdminRequestsController(IAdminRequestService adminRequestService)
    {
        _adminRequestService = adminRequestService;
    }
    [Authorize(Roles ="ADMIN")]
    [HttpGet]
    public async Task<IActionResult> GetAdminRequests()
    {
        var requests = await _adminRequestService.GetPendingRequestsAsync();
        return Ok(requests);
    }
    [HttpPost]
    public async Task<IActionResult> CreateAdminRequest([FromBody] AdminRequestDTO request)
    {
        try
        {
            var result = await _adminRequestService.CreateRequestAsync(request);
            return Ok(new { success = result, message = "Request created successfully." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "An unexpected error occurred.", details = ex.Message });
        }
    }
    [HttpPatch("/admin-requests/{requestID}")]
    public async Task<IActionResult> ResoloveAdminRequest(int requestID)
    {
        var result = await _adminRequestService.UpdateRequestStatusAsync(requestID);

        if(!result) return BadRequest();

        return Ok(result);
    }
}
