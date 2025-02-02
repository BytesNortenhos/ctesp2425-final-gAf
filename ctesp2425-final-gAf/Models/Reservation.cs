namespace ctesp2425_final_gAf.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public DateTime ReservationDateTime { get; set; }
        public Table Table { get; set; }
        public int NumberOfPeople { get; set; }
        public DateTime CreatedAt { get; set; }
        public int StatusReservation { get; set; }

    }
}
