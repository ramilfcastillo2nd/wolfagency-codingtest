using System.Linq.Expressions;

namespace Infrastructure.Data.Specifications
{
    public interface ISpecification<T>
    {
        //Generic Methods
        Expression<Func<T, bool>> Criteria { get; }   //Expression takes a function, function takes a Type(T) and returns a boolean
        List<Expression<Func<T, object>>> Includes { get; }

        //Sorting
        Expression<Func<T, object>> OrderBy { get; }
        Expression<Func<T, object>> OrderByDescending { get; }

        //pagination
        int Take { get; }
        int Skip { get; }
        bool IsPagingEnabled { get; }
    }
}
