using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using BookingChallenge.Models;
using BookingChallenge.Providers;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace BookingChallenge.Controllers
{
    #region Helpers
    public enum BookingStatus
    {
        Ok = 0,
        ErrInvalidDate,
        ErrDaysInvalid,
        ErrStayLimitExceeded,
        ErrDaysAdvanceLimitExceeded
    }

    public class BookingException : Exception
    {
        public BookingException()
        {
        }

        public BookingException(string message)
            : base(message)
        {
        }

        public BookingException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
    #endregion

    [RoutePrefix("booking")]
    public class BookingController : ApiController
    {
        private readonly BookingContext context;

        public BookingController()
            : this (ApplicationDbContextProvider.Instance.DBBookingContext)
        {
        }

        public BookingController(BookingContext aContext)
        {
            context = aContext;
        }

        /// <summary>
        /// GET: api/booking
        /// </summary>
        /// <returns></returns>
        public IHttpActionResult Get()
        {
            if (context.BookingItems.Count() <= 0)
                return BadRequest();
            return Ok(context.BookingItems.ToList());
        }

        /// <summary>
        /// GET: api/booking?id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Swashbuckle.Swagger.Annotations.SwaggerOperation("GetByBookID")]
        public IHttpActionResult Get(string id)
        {
            Booking b = context.Select(id);
            //Booking b = context.BookingItems.FindAsync(bookt_id).Result;

            if (b == null)
                return NotFound();

            return Ok(b);
        }

        /// <summary>
        /// POST: booking
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IHttpActionResult Post([FromBody] Booking value)
        {
            var result = CheckBooking(value) as NegotiatedContentResult<BookingException>;

            if (result == null)
            {
                if (context.Select(value.id) == null)
                    return Created(nameof(BookingController), context.Create(value));
                else
                    return Conflict();
            }

            return result;
        }

        /// <summary>
        /// PUT: api/booking/5
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IHttpActionResult Put([FromBody] Booking value)
        {
            var result = CheckBooking(value) as NegotiatedContentResult<BookingException>;

            if (result == null)
            {
                if (context.Select(value.id) != null)
                    return Ok(context.Update(value));
                else
                    return NotFound();
            }

            return result;
        }

        /// <summary>
        /// DELETE: booking/5
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IHttpActionResult Delete(string id)
        {
            Booking b = context.Select(id);
            if(b != null)
                return Ok(context.Delete(b));

            return NotFound();
        }

        [Route("check")]
        public IHttpActionResult CheckAvailability(string id, DateTime start, DateTime end)
        {
            if (start == null || end == null)
                return BadRequest("Invalid dates entry.");

            if ((end - start).Days <= 0)
                return BadRequest("Invalid dates entry.");

            if (context.BookingItems.Count() == 0)
                return Ok("The room is available");

            foreach (Booking reservation in context.BookingItems)
            {
                if (reservation.id != id)
                {
                    if (reservation.startDate.CompareTo(start) <= 0 && 
                        reservation.endDate.CompareTo(start) >= 0)
                        return Conflict();

                    if (reservation.startDate.CompareTo(end) <= 0 && 
                        reservation.endDate.CompareTo(end) >= 0)
                        return Conflict();
                }
            }

            return Ok("The room is available");
        }

        private BookingStatus ValidateDates(string id, DateTime start, DateTime end)
        {
            if(start == null || end == null)
                return BookingStatus.ErrDaysInvalid;

            var days = (end - start).Days;
            if (days <= 0)
                return BookingStatus.ErrDaysInvalid;

            if (days > 3)
                return BookingStatus.ErrStayLimitExceeded;

            days = (start - DateTime.Now).Days;
            if (days <= 0)
                return BookingStatus.ErrInvalidDate;

            var available = CheckAvailability(id, start, end) as OkNegotiatedContentResult<string>;

            if (available == null)
                return BookingStatus.ErrInvalidDate;

            if (days > 30)
                return BookingStatus.ErrDaysAdvanceLimitExceeded;

            return BookingStatus.Ok;
        }

        private IHttpActionResult CheckBooking(Booking value)
        {
            if(value.id.Length <= 1)
                return Content(HttpStatusCode.BadRequest,
                        new BookingException(
                            "The identification is mandatory."));

            IHttpActionResult result;
            var status = ValidateDates(value.id, value.startDate, value.endDate);

            switch (status)
            {
                case BookingStatus.Ok:
                    result = Content(HttpStatusCode.OK, 
                        "This book is OK");
                    break;

                case BookingStatus.ErrInvalidDate:
                    result = 
                        Content(HttpStatusCode.UnsupportedMediaType, 
                        new BookingException(
                            "The requested dates are unavailable"));
                    break;

                case BookingStatus.ErrDaysInvalid:
                    result =
                        Content(HttpStatusCode.UnsupportedMediaType,
                        new BookingException(
                            "Bookings must be for at least one day."));
                    break;

                case BookingStatus.ErrStayLimitExceeded:
                    result =
                        Content(HttpStatusCode.UnsupportedMediaType,
                        new BookingException(
                            "Due to the high demand for this hotel, " +
                            "it's not possible to book a room to stay " +
                            "more than 3 days"));
                    break;

                case BookingStatus.ErrDaysAdvanceLimitExceeded:
                    result =
                        Content(HttpStatusCode.UnsupportedMediaType,
                        new BookingException(
                            "Due to the high demand for this hotel, " +
                            "it's not possible to book a room more " +
                            "than 30 days in advance"));
                    break;

                default:
                    result = result =
                        Content(HttpStatusCode.NotFound,
                        new BookingException("Unknown status received."));
                    break;
            }

            return result;
        }
    }
}