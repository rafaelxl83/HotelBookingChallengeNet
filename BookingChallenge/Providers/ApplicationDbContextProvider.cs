using BookingChallenge.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingChallenge.Providers
{
    /// <summary>
    /// Application DB Context provider
    /// </summary>
    public class ApplicationDbContextProvider
    {
        #region Singleton
        /// <summary>
        /// Application Database Context Provider Singleton
        /// </summary>
        public static ApplicationDbContextProvider Instance { get { return instance; } }
        private static readonly ApplicationDbContextProvider instance = new ApplicationDbContextProvider();
        private ApplicationDbContextProvider() { }

        private static readonly BookingContext bContext = new BookingContext();
        #endregion

        /// <summary>
        /// Booking DB Context
        /// </summary>
        public BookingContext DBBookingContext { get => bContext; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<BookingContext>(opt =>
                opt.UseInMemoryDatabase("Booking"));

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }
    }
}