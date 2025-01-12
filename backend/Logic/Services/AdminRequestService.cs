using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Common;
using Domain.DTOs;
using Domain.Users;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;
using Logic.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class AdminRequestService(IDataRepository repository, UserManager<User> userManager) : IAdminRequestService
{
    private readonly IDataRepository _repository = repository;
    private readonly UserManager<User> _userManager = userManager;
    public async Task<List<AdminRequest>> GetPendingRequestsAsync()
    {
       var result =  await _repository.AdminRequestRepository.GetByConditionAsync(r => r.isResolved == false);
       
        if(result == null) { throw new NullReferenceException(nameof(result)); }

        return result;
    }

    public async Task<bool> CreateRequestAsync(AdminRequestDTO request)
    {
        // Sprawdzenie, czy użytkownik istnieje
        var user = await _userManager.FindByIdAsync(request.UserID);
        if (user == null)
        {
            throw new ArgumentException("User does not exist.");
        }

        // Sprawdzenie, czy użytkownik już jest adminem
        var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
        if (isAdmin)
        {
            throw new InvalidOperationException("You are already an admin.");
        }

        // Sprawdzenie, czy istnieje nierozstrzygnięte zapytanie
        var unresolvedRequest = await _repository.AdminRequestRepository.GetByConditionAsync(
            r => r.UserID == request.UserID && r.isResolved == false);

        if (unresolvedRequest.Count > 0)
        {
            throw new InvalidOperationException("You already have a pending admin request.");
        }

        // Tworzenie nowego zapytania (jeśli brak nierozstrzygniętego wniosku)
        AdminRequest adminRequest = new AdminRequest
        {
            UserName = user.UserName!,
            UserID = request.UserID,
            RequestDate = DateTime.UtcNow,
            isResolved = false,
            Reason = request.Reason,
        };

        var result = await _repository.AdminRequestRepository.AddAsync(adminRequest);

        if (result == -1)
        {
            throw new ArgumentException("Error while adding your request.");
        }

        return true;
    }


    public async Task<bool> UpdateRequestStatusAsync(int ID)
    {
        var request = await _repository.AdminRequestRepository.GetByIDAsync(ID);
        if (request == null) return false;

        request.isResolved = true;
        var result = await _repository.AdminRequestRepository.UpdateAsync(request);

        return result;
    }
}
