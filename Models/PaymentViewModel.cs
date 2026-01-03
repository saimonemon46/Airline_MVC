using System.ComponentModel.DataAnnotations;

namespace Air.Models
{
    public class PaymentViewModel
    {
        public int AirplaneId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }

        public int SeatRow { get; set; }
        public char SeatColumn { get; set; }
        public string Seat => $"{SeatRow}{SeatColumn}";

        [Display(Name = "Amount to Pay")]
        public decimal Amount { get; set; }

        // Stripe
        public string PublishableKey { get; set; }
        public string ClientSecret { get; set; }
    }
}
