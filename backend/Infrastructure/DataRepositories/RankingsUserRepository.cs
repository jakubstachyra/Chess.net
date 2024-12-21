using Domain.Common;
using Infrastructure.DataContext;
using Infrastructure.DataRepositories;
using Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RankingsUserRepository(DomainDataContext context) : Repository<RankingsUser>(context), IRankingsUserRepository
{
    private readonly DomainDataContext _context = context;


    public  async Task<IEnumerable<RankingsUser>> GetAllRankingsAsync()
    {
        return await _context.Set<RankingsUser>()
            .Include(ru => ru.Ranking) 
            .ToListAsync();
    }
}
