using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ctesp2425_final_gAf.Models;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Globalization;

namespace ctesp2425_final_gAf.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReservationsController(AppDbContext context)
    {
        _context = context;
    }


    [HttpGet]
    public async Task<IActionResult> GetReservations(DateOnly? date)
    {
        if(date != null) return Ok(await _context.Reservations.Where(r => r.ReservationDate == date && r.StatusReservation == 0).ToListAsync());

        return Ok(await _context.Reservations.Where(r => r.StatusReservation == 0).ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReservation(int id)
    {
        var reservation = _context.Reservations.FirstOrDefault(r => r.Id == id && r.StatusReservation == 0);
        return (reservation == null) ? NotFound() : Ok(reservation);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReservation([Required] String customerName, [Required] DateOnly resDate, [Required] TimeOnly resTime, [Required] int tableNumber, [Required] int numOfPeople)
    {
        DateTime resDateTime = new DateTime(resDate.Year, resDate.Month, resDate.Day, resTime.Hour, resTime.Minute, resTime.Second);
        if (resDateTime < DateTime.Now) return BadRequest("A data/hora de reserva deve ser superior à atual!");

        bool isTableReserved = await _context.Reservations.AnyAsync(r => r.TableNumber == tableNumber && r.ReservationDate == resDate && r.ReservationTime == resTime && r.StatusReservation == 0);
        if (isTableReserved) return BadRequest("A mesa já está reservada para essa data/hora!");

        Reservation newReservation = new Reservation
        {
            CustomerName = customerName,
            ReservationDate = resDate,
            ReservationTime = resTime,
            TableNumber = tableNumber,
            NumberOfPeople = numOfPeople,
            CreatedAt = DateTime.Now,
            StatusReservation = 0
        };

        _context.Reservations.Add(newReservation);
        await _context.SaveChangesAsync();
        return Ok(newReservation);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReservation(int id, String? customerName, DateOnly? resDate, TimeOnly? resTime, int? tableNumber, int? numOfPeople)
    {
        if(customerName == null && resDate == null && resTime == null && tableNumber == null && numOfPeople == null) return BadRequest("Pelo menos 1 campo deve ser atualizado!");

        var reservation = _context.Reservations.FirstOrDefault(r => r.Id == id && r.StatusReservation == 0);
        if (reservation == null) return NotFound();

        reservation.CustomerName = (customerName != null) ? customerName : reservation.CustomerName;
        reservation.NumberOfPeople = (numOfPeople != null) ? (int) numOfPeople : reservation.NumberOfPeople;
        
        var wishedTable = (tableNumber != null) ? (int) tableNumber : reservation.TableNumber;
        if (resDate != null && resTime != null) {
            DateTime resDateTime = new DateTime(resDate.Value.Year, resDate.Value.Month, resDate.Value.Day, resTime.Value.Hour, resTime.Value.Minute, resTime.Value.Second);
            if (resDateTime < DateTime.Now) return BadRequest("A data/hora de reserva deve ser superior à atual!");

            bool isTableReserved = await _context.Reservations.AnyAsync(r => r.TableNumber == wishedTable && r.ReservationDate == resDate && r.ReservationTime == resTime && r.StatusReservation == 0);
            if (isTableReserved) return BadRequest("A mesa desejada já está reservada para essa data e hora!");

            reservation.ReservationDate = resDate.Value;
            reservation.ReservationTime = resTime.Value;
            reservation.TableNumber = wishedTable;
        } else if(resDate != null && resTime == null) {
            DateTime resDateTime = new DateTime(resDate.Value.Year, resDate.Value.Month, resDate.Value.Day, reservation.ReservationTime.Hour, reservation.ReservationTime.Minute, reservation.ReservationTime.Second);
            if (resDateTime < DateTime.Now) return BadRequest("A data/hora de reserva deve ser superior à atual!");

            bool isTableReserved = await _context.Reservations.AnyAsync(r => r.TableNumber == wishedTable && r.ReservationDate == resDate && r.ReservationTime == reservation.ReservationTime && r.StatusReservation == 0);
            if (isTableReserved) return BadRequest("A mesa desejada já está reservada para essa data e hora!");

            reservation.ReservationDate = resDate.Value;
            reservation.TableNumber = wishedTable;
        }
        else if (resDate == null && resTime != null) {
            DateTime dateTime = new DateTime(reservation.ReservationDate.Year, reservation.ReservationDate.Month, reservation.ReservationDate.Day, resTime.Value.Hour, resTime.Value.Minute, resTime.Value.Second);
            if (dateTime < DateTime.Now) return BadRequest("A data/hora de reserva deve ser superior à atual!");

            bool isTableReserved = await _context.Reservations.AnyAsync(r => r.TableNumber == wishedTable && r.ReservationDate == reservation.ReservationDate && r.ReservationTime == resTime && r.StatusReservation == 0);
            if (isTableReserved) return BadRequest("A mesa desejada já está reservada para essa data e hora!");

            reservation.ReservationTime = resTime.Value;
            reservation.TableNumber = wishedTable;
        } else if(resDate == null && resTime == null && tableNumber !=  null) {
            bool isTableReserved = await _context.Reservations.AnyAsync(r => r.TableNumber == wishedTable && r.ReservationDate == reservation.ReservationDate && r.ReservationTime == reservation.ReservationTime && r.StatusReservation == 0);
            if (isTableReserved) return BadRequest("A mesa desejada já está reservada para essa data e hora!");
            
            reservation.TableNumber = wishedTable;
        }

        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
        return Ok(reservation);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        var reservation = _context.Reservations.FirstOrDefault(r => r.Id == id && r.StatusReservation == 0);
        if (reservation == null) return NotFound();

        reservation.StatusReservation = 1;

        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();
        return Ok(reservation);
    }

    
}
