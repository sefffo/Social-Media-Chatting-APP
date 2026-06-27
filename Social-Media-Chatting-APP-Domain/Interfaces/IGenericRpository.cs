using System.Linq.Expressions;
using Social_Media_Chatting_APP_Domain.Entities;
using Social_Media_Chatting_APP_Domain.Specifications;

namespace Social_Media_Chatting_APP_Domain.Interfaces
{
    public interface IGenericRepository<TEntity, T> where TEntity : BaseEntity<T>
    {
        Task<IEnumerable<TEntity>> GetAllAsync();


        //Task<IEnumerable<TEntity>> GetAllAsync(ISpecifications<TEntity, Tkey> specifications);

        //to get it with the specifications and the specifications will be used to filter the data and
        //return only the data that we need and also to include the related data if we need it and also to order the data
        //if we need it and also to paginate the data if we need it
        //Task<int> CountAsync(ISpecifications<TEntity, Tkey> specifications);

        //Task<TEntity> GetByIdAsync(ISpecifications<TEntity, Tkey> specifications);


        Task<TEntity> GetByIdAsync(T id);

        Task AddAsync(TEntity entity);

        void Remove(TEntity entity);

        void Update(TEntity entity);
        
        Task<TEntity?> FindAsync(ISpecification<TEntity> specifications);
        
        Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specifications);
        Task<int> CountAsync(ISpecification<TEntity> specifications); //to get the count of the data that we need as it's gonna be used to paginate the data
        Task<TEntity?> GetByIdAsync(ISpecification<TEntity> specifications);
        
        
    }
}
