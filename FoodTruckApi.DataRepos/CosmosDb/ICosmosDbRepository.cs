using System;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace FoodTruckApi.DataRepos
{
    public interface ICosmosDbRepository<T>
    {
        Task<CosmosResult<T>> GetItemsAsync(Expression<Func<T, bool>> filter);
    }
}
