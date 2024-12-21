using Domain.Common;
using Domain.Users;
using Infrastructure.Interfaces;
using Logic.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.Data.Common;

namespace Logic.Services
{
    public class RankingService(IDataRepository repository, UserManager<User> userManager): IRankingService
    {
        private UserManager<User> _userManager = userManager;
        private IDataRepository _repository = repository;
        public async Task<bool> updateRankingByID(string userID, int rankingID, int pointsDelta)
        {
            var user = await _userManager.FindByEmailAsync(userID);

            if(user == null)
            {
                return false;
            }

            var ranking = await _repository.RankingsUserRepository.GetByIDAsync(rankingID);

            if(ranking == null)
            {
                return false;
            }


            ranking.Points = ranking.Points + pointsDelta;

            var result = await _repository.RankingsUserRepository.UpdateAsync(ranking);

            return result;
        }
        public async Task<RankingsUser> getUserRankingByID(string userID, int rankingID)
        {
            if (userID == null) { throw new ArgumentNullException(); }

            var user = await _userManager.FindByIdAsync(userID);

            if(user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var ranking = await _repository.RankingsUserRepository.GetByIDAsync(rankingID);

            if (ranking == null)
            {
                throw new KeyNotFoundException($"Ranking with ID: {rankingID} not found");
            }

            return ranking;
        }

        public async Task<IEnumerable<(Ranking Ranking, int Points)>> getUserRankingsByID(string userID)
        {
            if(userID == null) { throw new ArgumentNullException(); }
            
            var user = _userManager.FindByIdAsync(userID);

            if(user == null) { throw new KeyNotFoundException($"Not found user with ID: {userID}"); }

            var rankings = await _repository.RankingsUserRepository.GetAllAsync();

            if(!rankings.Any()) {
                throw new InvalidOperationException("No rankings found," +
                " but they were expected to exists.");
            }

            var result = rankings
                .Where(r => r.UserID == userID)
                .Select(r => (r.Ranking, r.Points));

            return result;
        }
    }
}
