﻿using Domain.Common;
using Domain.Users;
using Infrastructure.Interfaces;
using Logic.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Logic.Services
{
    public class ReportService(IDataRepository repository, UserManager<User> userManager) : IReportService
    {
        private readonly IDataRepository _repository = repository;
        private readonly UserManager<User> _userManager = userManager;

        public async Task<bool> ReportUserAsync(string userID, int gameID)
        {
            var suspect = await _userManager.FindByIdAsync(userID);
            if (suspect == null) { throw new ArgumentNullException(nameof(suspect)); }

            var game = await _repository.GameRepository.GetByIDAsync(gameID);

            if(game == null) { throw new ArgumentNullException($"{gameID} does not exist"); }

            var result = await _repository.ReportsRepository.AddAsync(new Report
            {
                SuspectID = suspect.Id,
                Suspect = suspect,
                GameID = gameID,
                Game = game,
            });
            return result >= 1;
        }

        public async Task<IEnumerable<Report>> GetAllActiveReports()
        {
            var result = await _repository.ReportsRepository.GetByConditionAsync(r => r.isResolved == false);
            return result.ToList() ?? Enumerable.Empty<Report>(); 
        }

        public async Task<bool> MakeReportResolved(int reportID)
        {
            var report = await _repository.ReportsRepository.GetByIDAsync(reportID);
            if(report == null) { throw new ArgumentNullException($"No report with ID: {reportID}"); }

            report.isResolved = true;

            var result = await _repository.ReportsRepository.UpdateAsync(report);

            return result;
        }
    }
}
