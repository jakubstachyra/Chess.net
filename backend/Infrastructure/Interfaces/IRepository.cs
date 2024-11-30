using Domain.Common;

namespace Infrastructure.Interfaces
{
    public interface IRepository<T> where T : Base
    {
        int Add(T entity);
        bool Delete(T entity);
        IEnumerable<T> GetAll();
        T? GetByID(int id);
        bool Update(T entity);
    }
}
