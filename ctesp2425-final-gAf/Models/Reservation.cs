namespace ctesp2425_final_gAf.Models;

public class Reservation
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public DateOnly ReservationDate { get; set; }
    public TimeOnly ReservationTime { get; set; }
    public int TableNumber { get; set; }
    public int NumberOfPeople { get; set; }
    public DateTime? CreatedAt { get; set; }
    public int StatusReservation { get; set; }

    /**
     * Status Reservation:
     * > 0: Reservado
     * > 1: Cancelado
     */

}
