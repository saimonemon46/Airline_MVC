using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Air.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string UserName { get; set; }

        [Required]
        public int AirplaneId { get; set; }  // FK column in DB

        [Required]
        [Range(1, 100)]
        public int SeatRow { get; set; }

        [Required]
        public char SeatColumn { get; set; }

        [NotMapped]
        public string SeatLabel => $"{SeatRow}{SeatColumn}";

        // Navigation properties
        public User User { get; set; }

        [ForeignKey("AirplaneId")]  // tells EF Core which FK to use
        public Airplane Airplane { get; set; }  // renamed for clarity
    }
}
