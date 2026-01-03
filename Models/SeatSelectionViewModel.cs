using System.Collections.Generic;

namespace Air.Models
{
    public class SeatSelectionViewModel
    {
        public int FlightId { get; set; }
        public string FlightName { get; set; }
        public decimal TicketPrice { get; set; }
        public int SeatRows { get; set; }
        public int SeatCols { get; set; }

        // List of booked seat labels (e.g. "1A", "2B")
        public List<string> BookedSeats { get; set; } = new List<string>();

        // ✅ Helper properties
        public int TotalSeats => SeatRows * SeatCols;

        public int AvailableSeats => TotalSeats - (BookedSeats?.Count ?? 0);
    }
}