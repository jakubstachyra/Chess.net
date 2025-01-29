using Domain;
using Domain.Common;
using Domain.Users;
using Infrastructure.Interfaces;
using Logic.Interfaces;
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

            var ranking = await _repository.RankingsUserRepository.GetAllAsync();
            var rankingId = ranking.Where(ranking => ranking.Id == rankingID && ranking.UserID==userID).ToList().First();

            if (ranking == null)
            {
                return false;
            }


            rankingId.Points = rankingId.Points + pointsDelta;

            var result = await _repository.RankingsUserRepository.UpdateAsync(rankingId);

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

        public async Task<IEnumerable<UserRankingDto>> getUserRankingsByID(string userID)
        {
            if (string.IsNullOrEmpty(userID))
            {
                throw new ArgumentNullException(nameof(userID), "User ID cannot be null or empty.");
            }

            var user = await _userManager.FindByIdAsync(userID);
            if (user == null)
            {
                throw new KeyNotFoundException($"Not found user with ID: {userID}");
            }

            var rankings = await _repository.RankingsUserRepository.GetAllRankingsAsync();
            foreach (var ranking in rankings)
            {
                Console.WriteLine($"UserID: {ranking.UserID}, Ranking: {ranking.Ranking}, Points: {ranking.Points}");
            }
            return rankings
                .Where(r => r.UserID == userID)
                .OrderBy(r => r.Ranking.Id)
                .Select(r => new UserRankingDto
                {
                    Ranking = r.Ranking?.Name ?? "No Name",
                    RankingInfo = r.Ranking?.Description ?? "No Description",
                    Points = r.Points
                });



        }

        public async Task CalculateDeltaAndUpdateRanking(string userId1, string userId2, string result, string mode)
        {
          
            var user1 = await _userManager.FindByIdAsync(userId1);
            var user2 = await _userManager.FindByIdAsync(userId2);

            if (user1 == null || user2 == null)
            {
                return;
            }


            var rankings = await _repository.RankingRepository.GetAllAsync();
            var rankingId = rankings.FirstOrDefault(ranking => ranking.Name == mode);


            if (rankingId == null)
                return;

            var rankingUsers = await _repository.RankingsUserRepository.GetAllAsync();

            var rankingUser1 = rankingUsers.FirstOrDefault(ranking => ranking.RankingID == rankingId.Id && ranking.UserID == userId1);
            var rankingUser2 = rankingUsers.FirstOrDefault(ranking => ranking.RankingID == rankingId.Id && ranking.UserID == userId2);


            if (rankingUser1 == null || rankingUser2 == null)
                return;

            var points1 = rankingUser1.Points;
            var points2 = rankingUser2.Points;

            const int kFactor = 32; 

            double expected1 = 1 / (1 + Math.Pow(10, (points2 - points1) / 400.0));
            double expected2 = 1 / (1 + Math.Pow(10, (points1 - points2) / 400.0));

            double actual1;
            double actual2;

            if (result == "1-0")
            {
                actual1 = 1.0;
                actual2 = 0.0;
            }
            else if (result == "0-1")
            {
                actual1 = 0.0;
                actual2 = 1.0;
            }
            else if (result == "1/2-1/2")
            {
                actual1 = 0.5;
                actual2 = 0.5;
            }
            else
            {
                return; 
            }

            var newPoints1 = points1 + kFactor * (actual1 - expected1);
            var newPoints2 = points2 + kFactor * (actual2 - expected2);


            rankingUser1.Points = (int)newPoints1;
            rankingUser2.Points = (int)newPoints2;

            await _repository.RankingsUserRepository.UpdateAsync(rankingUser1);
            await _repository.RankingsUserRepository.UpdateAsync(rankingUser2);
        }

    }
}
