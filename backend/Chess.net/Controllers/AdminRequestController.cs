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
    public async Task<IActionResult> CreateRequest([FromBody] AdminRequestDTO request)
    {
        var createdRequest = await _adminRequestService.CreateRequestAsync(request);
        if (!createdRequest) return BadRequest();
        return Ok(createdRequest);
    }
}
