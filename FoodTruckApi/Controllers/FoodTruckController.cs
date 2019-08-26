using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FoodTruckApi.DataRepos;
using FoodTruckApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Spatial;
using Microsoft.Extensions.Logging;

namespace FoodTruckApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodTruckController : ControllerBase
    {
        private readonly ICosmosDbRepository<FoodTruck> _repo;
        private readonly ILogger<FoodTruckController> _logger;

        public FoodTruckController(ICosmosDbRepository<FoodTruck> repo, ILogger<FoodTruckController> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }

        // GET api/foodtruck
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FoodTruck>>> Get(double longitude, double latitude, int distance)
        {
            if (longitude > 180 || longitude < -180)
            {
                return BadRequest("longitude invalid");
            }
            if (latitude > 90 || latitude < -90)
            {
                return BadRequest("latitude invalid");
            }
            if (distance <= 0)
            {
                return BadRequest("distance must be positive");
            }

            CosmosResult<FoodTruck> result = new CosmosResult<FoodTruck>();
            try
            {
                result = await _repo.GetItemsAsync(t =>
                    t.Location.Distance(new Point(longitude, latitude)) < distance);
                _logger.LogInformation($"Get: ItemCount={result.Items.Count}, RequestChage={result.RequestCharge}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get Failed");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            return Ok(result.Items);
        }
    }
}
