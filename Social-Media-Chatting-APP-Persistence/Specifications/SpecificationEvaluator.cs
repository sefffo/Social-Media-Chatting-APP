using Microsoft.EntityFrameworkCore;
using Social_Media_Chatting_APP_Domain.Specifications;

namespace Social_Media_Chatting_APP_Persistence.Specifications;

public class SpecificationEvaluator<T> where T : class
{
    /// <summary>
    ///IQueryable<T>      →  the raw ingredients (the DB table)
    ///ISpecification<T>  →  the recipe (what to do with them)
    ///Evaluator          →  the chef who follows the recipe
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="specification"></param>
    /// <returns></returns>
    public static IQueryable<T> GetQuery(IQueryable<T> queryable, ISpecification<T> specification)
    {
        // we implement the logic of the specification here with this order as we must filter first before we paginate anything,
        // as that the EF core order 
        if (specification.Criteria != null)
        {
            queryable = queryable.Where(specification.Criteria);
        }

        if (specification.Includes != null)
        {
            foreach (var include in specification.Includes)
            {
                queryable = queryable.Include(include);
            }
        }

        if (specification.IncludesStrings != null)
        {
            foreach (var IncludeString in specification.IncludesStrings)
            {
                queryable = queryable.Include(IncludeString);
            }
        }

        if (specification.OrderBy != null)
        {
            queryable = queryable.OrderBy(specification.OrderBy);
        }

        if (specification.OrderByDescending != null)
        {
            queryable = queryable.OrderByDescending(specification.OrderByDescending);
        }

        if (specification.IsPagingEnabled)
        {
            queryable = queryable.Skip(specification.Skip).Take(specification.Take);
        }


        return queryable;
    }
}