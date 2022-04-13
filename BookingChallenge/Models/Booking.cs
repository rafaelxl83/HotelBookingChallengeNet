using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingChallenge.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Booking
    {
        public string id { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
        public int numberOfBeds { get; set; }
    }
}