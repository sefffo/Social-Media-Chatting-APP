using System.Linq.Expressions;
using Social_Media_Chatting_APP_Domain.Entities;

namespace Social_Media_Chatting_APP_Domain.Specifications;

public interface ISpecification<TEntity>
{
    public Expression<Func<TEntity, bool>>? Criteria { get; } // can be null as we don't usually have a criteria 
    public List<Expression<Func<TEntity, object>>> Includes { get; }
    public List<string> IncludesStrings { get; } 
    public Expression<Func<TEntity, object>>? OrderBy { get; } //same as criteria
    public Expression<Func<TEntity, object>>? OrderByDescending { get; } //same as criteria
    int Take { get; }
    int Skip { get; }
    public bool IsPagingEnabled { get; }
}