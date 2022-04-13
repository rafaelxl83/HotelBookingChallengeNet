using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BookingChallenge.Providers;
using Microsoft.EntityFrameworkCore;

namespace BookingChallenge.Models
{
    public class BookingContext : DbContext
    {
        public DbSet<Booking> BookingItems { get; set; }

        public BookingContext()
             : this(new DbContextOptionsBuilder<BookingContext>()
                    .UseInMemoryDatabase("Booking")
                    .Options)
        {
        }

        public BookingContext(DbContextOptions<BookingContext> options)
            : base(options)
        {
        }

        public Booking Select(string id)
        {
            return BookingItems.FindAsync(id).Result;
        }

        public Booking Create(Booking book)
        {
            BookingItems.Add(book);
            SaveChangesAsync();

            return Select(book.id);
        }

        public Booking Update(Booking book)
        {
            var b = Select(book.id);
            if (b != null)
            {
                Entry(b).State = EntityState.Modified;
                b.startDate = book.startDate;
                b.endDate = book.endDate;
                b.numberOfBeds = book.numberOfBeds;
                SaveChangesAsync();

                return Select(b.id);
            }

            return null;
        }

        public Booking Delete(Booking book)
        {
            var b = Select(book.id);
            if (b != null)
            {
                Entry(b).State = EntityState.Deleted;
                SaveChangesAsync();

                return b;
            }

            return null;
        }
    }
}