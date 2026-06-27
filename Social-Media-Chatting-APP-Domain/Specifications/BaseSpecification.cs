using System.Linq.Expressions;

namespace Social_Media_Chatting_APP_Domain.Specifications;

public class BaseSpecification<TEntity> : ISpecification<TEntity>
{
    public Expression<Func<TEntity, bool>>? Criteria { get; private set; }
    public List<Expression<Func<TEntity, object>>> Includes { get; } = new();
    public List<string> IncludesStrings { get; } = new();
    public Expression<Func<TEntity, object>>? OrderBy { get; private set;}
    public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }
    public int Take { get; private set;}
    public int Skip { get; private set; }
    public bool IsPagingEnabled { get; private set;}
    
    //we must implement 2 different constructors for the base specification
    protected BaseSpecification(){}
    
    protected BaseSpecification(Expression<Func<TEntity, bool>> criteria)
    {
        this.Criteria = criteria;
    }

    public void ApplyPagination(int pageSize, int pageIndex)
    {
        Skip = (pageIndex - 1) * pageSize;
        Take = pageSize;
        IsPagingEnabled = true;
    }

    
    
    public void ApplyTake (int take) => Take = take;
    

    public void AddIncludes(Expression<Func<TEntity, object>> includes)
    {
        Includes.Add(includes);
    }
    public void AddIncludes(string includes)
    {
        IncludesStrings.Add(includes);
    }
    
    public void ApplyOrderBy(Expression<Func<TEntity, object>> orderBy)
    {
        OrderBy = orderBy;
    }
    
    public void ApplyOrderByDescending(Expression<Func<TEntity, object>> orderByDescending)
    {
        OrderByDescending = orderByDescending;
    }
    
  
    
    
}