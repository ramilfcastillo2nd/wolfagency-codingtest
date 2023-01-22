using Infrastructure.Data.Repositories.Interfaces;
using Infrastructure.Data.Specifications;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly WolfAgencyCodingTestContext _context;
        public GenericRepository(WolfAgencyCodingTestContext context)
        {
            _context = context;
        }

        #region Plain Generic
        public async Task<T> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);

        }

        public async Task<IReadOnlyList<T>> ListAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        //this is just mine.. Sonny.
        public async Task<int> InsertAsync(T obj)
        {
            _context.Set<T>().Add(obj);
            return await _context.SaveChangesAsync();
        }

        #endregion

        # region Specification pattern. 
        //The ISpecification handles the where clauses (prop=Criteria) then handles what to include (prop=Includes)
        public async Task<T> GetEntityWithSpec(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).ToListAsync();
        }

        public async Task<int> CountAsync(ISpecification<T> spec)
        {
            return await ApplySpecification(spec).CountAsync();
        }

        private IQueryable<T> ApplySpecification(ISpecification<T> spec)
        {
            //we need 2 params here, the entity dbset which needs to be iqueryable, then the where clauses -> spec
            //T gets replaced by dbset e.g. Product, then converted to Queryable <= this is the inputQuery
            return SpecificationEvaluator<T>.GetQuery(_context.Set<T>().AsQueryable(), spec);
        }



        #endregion

        #region Basic Methods
        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public void AddRange(List<T> entities)
        {
            _context.Set<T>().AddRange(entities);
        }
        #endregion
    }
}
