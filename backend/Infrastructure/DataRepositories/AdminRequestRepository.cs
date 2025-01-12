using Domain.Common;
using Infrastructure.DataContext;
using Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataRepositories
{
    public class AdminRequestRepository(DomainDataContext context) : Repository<AdminRequest>(context), IAdminRequestRepository{}
}
