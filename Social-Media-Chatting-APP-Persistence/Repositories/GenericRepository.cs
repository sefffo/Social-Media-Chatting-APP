using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Interfaces;
using Social_Media_Chatting_APP_Domain.Specifications;
using Social_Media_Chatting_APP_Persistence.DbContext;
using Social_Media_Chatting_APP_Persistence.Specifications;

namespace Social_Media_Chatting_APP_Persistence.Repositories
{
    public class GenericRepository<TEntity, TKey>(Social_Media_Chatting_APP_DbContext context)
        : IGenericRepository<TEntity, TKey> where TEntity : BaseEntity<TKey>
    {
        private IGenericRepository<TEntity, TKey> _genericRepositoryImplementation;

        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            //bad Practice

            //if (condition != null)
            //{
            //    return await context.Set<TEntity>().ToListAsync();
            //}
            //if (Includes != null)
            //{
            //    IQueryable<TEntity> EntryPoint = context.Set<TEntity>();
            //    foreach (var includeExp in Includes)
            //    {
            //        EntryPoint =  EntryPoint.Include(includeExp);
            //    }
            //    return await EntryPoint.ToListAsync();
            //}

            return await context.Set<TEntity>().ToListAsync();
        }

        public async Task<TEntity> GetByIdAsync(TKey id)
            => await context.Set<TEntity>().FindAsync(id);
        

        public async Task AddAsync(TEntity entity)
            => await context.Set<TEntity>().AddAsync(entity);


        public void Remove(TEntity entity)
            => context.Set<TEntity>().Remove(entity);

        public void Update(TEntity entity)
            => context.Set<TEntity>().Update(entity);

        public async Task<TEntity?> FindAsync(ISpecification<TEntity> specifications)
            => await SpecificationEvaluator<TEntity>.GetQuery(context.Set<TEntity>().AsQueryable(), specifications).FirstOrDefaultAsync();
        public async Task<TEntity?> FindAsync(Expression<Func<TEntity, bool>> predicate)
            => await context.Set<TEntity>().FirstOrDefaultAsync(predicate);


        public async Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate)
            =>  await context.Set<TEntity>().Where(predicate).ToListAsync();

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specifications)
           =>  await SpecificationEvaluator<TEntity>.GetQuery(context.Set<TEntity>().AsQueryable(),specifications).ToListAsync();
        
        public Task<int> CountAsync(ISpecification<TEntity> specifications)
            => SpecificationEvaluator<TEntity>.GetQuery(context.Set<TEntity>().AsQueryable(), specifications).CountAsync();

        public async Task<TEntity?> GetByIdAsync(ISpecification<TEntity> specifications)
            => await SpecificationEvaluator<TEntity>.GetQuery(context.Set<TEntity>().AsQueryable(), specifications).FirstOrDefaultAsync();
    }
}