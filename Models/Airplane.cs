using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Air.Models
{
    public class Airplane
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Airport Name")]
        public string AirportName { get; set; }

        [Required]
        [Display(Name = "Airplane Name")]
        public string AirplaneName { get; set; }

        [Required]
        [Display(Name = "Seat Rows")]
        [Range(1, 100)]
        public int SeatRows { get; set; }

        [Required]
        [Display(Name = "Seat Columns")]
        [Range(1, 10)]
        public int SeatColumns { get; set; }

        [Required]
        [Display(Name = "Departure Time")]
        public DateTime DepartureTime { get; set; }

        [Required]
        [Display(Name = "Starting Point")]
        public string StartingPoint { get; set; }

        [Required]
        [Display(Name = "Destination")]
        public string Destination { get; set; }

        [Required]
        [Display(Name = "Ticket Price")]
        [Range(1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal TicketPrice { get; set; }

        // Navigation property for related bookings
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

        // ✅ Mapped fields (stored in DB)
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }

        // ✅ Helper method to update seat counts
        public void UpdateSeats()
        {
            TotalSeats = SeatRows * SeatColumns;
            AvailableSeats = TotalSeats - (Bookings?.Count ?? 0);
        }
    }
}
