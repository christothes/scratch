using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FoodTruckApi.Controllers;
using FoodTruckApi.DataRepos;
using FoodTruckApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace FoodTruckApi.Tests
{
    [TestClass]
    public class FoodTruckControllerTests
    {
        private Mock<ILogger<FoodTruckController>> logMock;
        private Mock<ICosmosDbRepository<FoodTruck>> repoMock;
        private FoodTruckController target;
        private CosmosResult<FoodTruck> items;
        private double invalidLongitude = 181;
        private double invalidLatitude = 91;
        private int invaidDistance = -1;

        [TestInitialize]
        public void TestInit()
        {
            logMock = new Mock<ILogger<FoodTruckController>>();
            logMock.Setup(m => m.Log<object>(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<FoodTruckController>(), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
            repoMock = new Mock<ICosmosDbRepository<FoodTruck>>();
            target = new FoodTruckController(repoMock.Object, logMock.Object);
        }

        [TestMethod]
        public void ValidatesArgs()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new FoodTruckController(null, Mock.Of<ILogger<FoodTruckController>>()));
            Assert.ThrowsException<ArgumentNullException>(() => new FoodTruckController(Mock.Of<ICosmosDbRepository<FoodTruck>>(), null));
        }

        [TestMethod]
        public async Task Get_Success()
        {
            //Arrange
            items = new CosmosResult<FoodTruck>(123, new List<FoodTruck> { new FoodTruck { locationid = "1" }, new FoodTruck { locationid = "2" } });
            repoMock.Setup(m => m.GetItemsAsync(It.IsAny<Expression<Func<FoodTruck, bool>>>()))
                .ReturnsAsync(items);

            //Act
            var response = (await target.Get(0, 0, 1000)).Result as OkObjectResult;
            var result = response.Value as IEnumerable<FoodTruck>;

            //Assert
            Assert.AreEqual(StatusCodes.Status200OK, response.StatusCode);
            CollectionAssert.AreEqual(items.Items, result.ToList());
            logMock.Verify(m => m.Log<object>(LogLevel.Information, It.IsAny<EventId>(), It.Is<object>(s => ((FormattedLogValues)s).ToString().Contains(items.RequestCharge.ToString())), It.IsAny<Exception>(), It.IsAny<Func<object, Exception, string>>()));
        }

        [TestMethod]
        public async Task Get_Failed()
        {
            //Arrange
            var ex = new Exception("foo");
            items = new CosmosResult<FoodTruck>(123, new List<FoodTruck> { new FoodTruck { locationid = "1" }, new FoodTruck { locationid = "2" } });
            repoMock.Setup(m => m.GetItemsAsync(It.IsAny<Expression<Func<FoodTruck, bool>>>()))
                .ThrowsAsync(ex);

            //Act
            var response = (await target.Get(0, 0, 1000)).Result as StatusCodeResult;

            //Assert
            Assert.AreEqual(StatusCodes.Status500InternalServerError, response.StatusCode);
            logMock.Verify(m => m.Log<object>(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<object>(), ex, It.IsAny<Func<object, Exception, string>>()));
        }

        [TestMethod]
        public async Task Get_ArgumentValidation()
        {
            //Arrange

            repoMock.Setup(m => m.GetItemsAsync(It.IsAny<Expression<Func<FoodTruck, bool>>>()))
                .ReturnsAsync(new CosmosResult<FoodTruck>());

            //Act
            var response1 = (await target.Get(invalidLongitude, 0, 1000)).Result as BadRequestObjectResult;
            var response2 = (await target.Get(0, invalidLatitude, 1000)).Result as BadRequestObjectResult;
            var response3 = (await target.Get(0, 0, invaidDistance)).Result as BadRequestObjectResult;

            //Assert
            Assert.AreEqual(StatusCodes.Status400BadRequest, response1.StatusCode);
            Assert.AreEqual(StatusCodes.Status400BadRequest, response2.StatusCode);
            Assert.AreEqual(StatusCodes.Status400BadRequest, response3.StatusCode);
        }
    }
}
