using BookingChallenge.Controllers;
using BookingChallenge.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Http.Routing;

namespace BookingChallenge.Tests.Controllers
{
    [TestClass]
    public class BookingControllerTest
    {
        private BookingController Initialize()
        {
            BookingController controller = new BookingController();

            controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri("http://localhost/api/booking")
            };
            controller.Configuration = new HttpConfiguration();
            controller.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            controller.RequestContext.RouteData = new HttpRouteData(
                route: new HttpRoute(),
                values: new HttpRouteValueDictionary { { "controller", "booking" } });

            return controller;
        }

        private void Clear(BookingController controller, Booking[] list)
        {
            foreach(Booking b in list)
                controller.Delete(b.id);
        }

        private Booking[] CreateDummyBooks(int amount)
        {
            var list = new Booking[amount];
            var start = DateTime.Now.AddDays(2);

            list[0] =
                new Booking()
                {
                    id = "book1",
                    startDate = DateTime.Now.AddDays(2),
                    endDate = DateTime.Now.AddDays(4),
                    numberOfBeds = 1
                };

            for (int i = 1; i < amount; i++)
            {
                
                list[i] =
                    new Booking()
                    {
                        id = "book" + (i + 1),
                        startDate = list[i - 1].endDate.AddDays(1),
                        endDate = list[i - 1].endDate.AddDays(3),
                        numberOfBeds = 1
                    };
            }

            return list;
        }

        [TestMethod]
        public void CheckAvailabilitySuccessTest()
        {
            var controller = new BookingController();

            var result = controller.CheckAvailability("book01", DateTime.Now.AddDays(2), DateTime.Now.AddDays(3));
            var response = result as OkNegotiatedContentResult<string>;

            Assert.AreNotEqual(response, null);
        }

        [TestMethod]
        public void ValidateDatesSuccessTest()
        {
            var controller = new BookingController();
            var b = CreateDummyBooks(1)[0];
            Type t = typeof(BookingController);

            var result = t.InvokeMember("ValidateDates",
                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null, controller, new object[] { b.id, b.startDate, b.endDate });

            Assert.AreEqual(result, BookingStatus.Ok);
        }

        [TestMethod]
        public void CheckBookingSuccessTest()
        {
            var controller = new BookingController();
            var b = CreateDummyBooks(1)[0];
            Type t = typeof(BookingController);

            var result = t.InvokeMember("CheckBooking",
                BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                null, controller, new object[] { b });

            var response = result as NegotiatedContentResult<BookingException>;

            Assert.AreEqual(response, null);
        }

        [TestMethod]
        public void PostBookingTest()
        {
            // Arrange
            var controller = Initialize();

            // Act
            Booking[] b = CreateDummyBooks(1);
            string booking = JObject.FromObject(b[0]).ToString();

            var result = controller.Post(b[0]);
            var response = result as CreatedNegotiatedContentResult<Booking>;

            // Assert
            Assert.AreEqual(response.Content.id, b[0].id);
            Clear(controller, b);
        }

        [TestMethod]
        public void PutBookingTest()
        {
            // Arrange
            var controller = Initialize();

            // Act
            Booking[] book = CreateDummyBooks(2);
            foreach (Booking b in book)
            {
                var res = controller.Post(b);
                var response = res as CreatedNegotiatedContentResult<Booking>;
            }

            book[0].startDate = DateTime.Now.AddDays(8);
            book[0].endDate = DateTime.Now.AddDays(9);

            var result = controller.Put(book[0]);
            var response2 = result as OkNegotiatedContentResult<Booking>;

            // Assert
            Assert.AreEqual(response2.Content.id, book[0].id);
            Clear(controller, book);
        }

        [TestMethod]
        public void GetAllBookingTest()
        {
            var controller = Initialize();

            // Act
            Booking[] book = CreateDummyBooks(4);

            foreach(Booking b in book)
            {
                var res = controller.Post(b);
                var response = res as CreatedNegotiatedContentResult<Booking>;
            }

            var result = controller.Get();
            var response2 = result as OkNegotiatedContentResult<List<Booking>>;

            // Assert
            Assert.AreEqual(response2.Content.Count, book.Count());
            Clear(controller, book);
        }

        [TestMethod]
        public void GetBookingTest()
        {
            var controller = Initialize();

            // Act
            Booking[] book = CreateDummyBooks(4);

            foreach (Booking b in book)
            {
                var res = controller.Post(b);
                var response = res as CreatedNegotiatedContentResult<Booking>;
            }

            var result = controller.Get(book[1].id);
            var response2 = result as OkNegotiatedContentResult<Booking>;

            // Assert
            Assert.AreEqual(response2.Content.id, book[1].id);
            Clear(controller, book);
        }

        [TestMethod]
        public void DeleteBookingTest()
        {
            var controller = Initialize();

            // Act
            Booking[] book = CreateDummyBooks(4);

            foreach (Booking b in book)
            {
                var res = controller.Post(b);
                var response = res as CreatedNegotiatedContentResult<Booking>;
            }

            var result = controller.Delete(book[2].id);
            var response2 = result as OkNegotiatedContentResult<Booking>;

            // Assert
            Assert.AreEqual(response2.Content.id, book[2].id);
            Clear(controller, book);
        }
    }
}
