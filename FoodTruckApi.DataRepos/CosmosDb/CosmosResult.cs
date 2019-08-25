using System.Collections.Generic;
using System.Linq;

namespace FoodTruckApi.DataRepos
{
    public class CosmosResult<T> {
        public CosmosResult()
        {
            RequestCharge = 0;
            Items = new List<T>();
        }
        public CosmosResult(double requestCharge, List<T> items)
        {
            RequestCharge = requestCharge;
            Items = items;
        }
        public double RequestCharge { get; private set; }
        public List<T> Items { get; private set; }
    }
}