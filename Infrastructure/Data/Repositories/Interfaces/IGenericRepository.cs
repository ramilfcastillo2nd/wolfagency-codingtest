using Infrastructure.Data.Specifications;
using Infrastructure.Entities;

namespace Infrastructure.Data.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : BaseEntity   //T should be of Type (inherits from) BaseEntity
    {
        Task<T> GetByIdAsync(int id);
        Task<IReadOnlyList<T>> ListAllAsync();
        Task<T> GetEntityWithSpec(ISpecification<T> spec);
        Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec);
        Task<int> CountAsync(ISpecification<T> spec);
        void Add(T entity);
        void AddRange(List<T> entities);
        void Update(T entity);
        void Delete(T entity);
    }
}
