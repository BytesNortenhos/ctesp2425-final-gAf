using System;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using ctesp2425_final_gAf.Models;
using ctesp2425_final_gAf.Controllers;
using Xunit.Abstractions;

namespace XUnit_Test
{
    public class ReservationsControllerTests
    {
        private readonly AppDbContext _mockContext;
        private readonly ReservationsController _controller;

        public ReservationsControllerTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestReservationsDb")
                .Options;

            _mockContext = new AppDbContext(options);
            _mockContext.Reservations.AddRange(
                new Reservation
                {
                    Id = 1,
                    CustomerName = "João Costa",
                    ReservationDate = new DateOnly(2024, 1, 15),
                    ReservationTime = new TimeOnly(19, 0),
                    TableNumber = 1,
                    NumberOfPeople = 2,
                    StatusReservation = 0
                },
                new Reservation
                {
                    Id = 2,
                    CustomerName = "Roberto Valente",
                    ReservationDate = new DateOnly(2024, 1, 16),
                    ReservationTime = new TimeOnly(20, 0),
                    TableNumber = 2,
                    NumberOfPeople = 4,
                    StatusReservation = 1
                },
                new Reservation
                {
                    Id = 3,
                    CustomerName = "Dinis Faryna",
                    ReservationDate = new DateOnly(2024, 1, 17),
                    ReservationTime = new TimeOnly(19, 0),
                    TableNumber = 3,
                    NumberOfPeople = 2,
                    StatusReservation = 0
                },
                new Reservation
                {
                    Id = 4,
                    CustomerName = "Pedro Barbosa",
                    ReservationDate = new DateOnly(2024, 3, 30),
                    ReservationTime = new TimeOnly(19, 0),
                    TableNumber = 4,
                    NumberOfPeople = 5,
                    StatusReservation = 0
                }
            );
            _mockContext.SaveChanges();

            _controller = new ReservationsController(_mockContext);
        }

        [Fact]
        public async Task GetReservations_WithoutDate_ReturnsActiveReservations()
        {
            var result = await _controller.GetReservations(null);
            Assert.NotNull(result);
            
            //Verifica se o resultado é do tipo OkObjectResult
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
           
            // Verifica se o valor retornado é uma lista de reservas e se a mesma tem valores
            var reservations = Assert.IsAssignableFrom<IEnumerable<Reservation>>(okResult.Value);
            Assert.NotNull(reservations);
            
            // Verifica se todas as reservas retornadas têm status ativo (StatusReservation == 0)
            Assert.All(reservations, r => Assert.Equal(0, r.StatusReservation));

            // Verifica se o número de reservas retornadas é exatamente 2
            Assert.Equal(2, reservations.Count());
        }


        [Fact]
        public async Task GetReservations_WithSpecificDate_ReturnsCorrectReservations()
        {
            var result = await _controller.GetReservations(new DateOnly(2024, 1, 17));
            Assert.NotNull(result);
           
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);

            var reservations = Assert.IsAssignableFrom<IEnumerable<Reservation>>(okResult.Value);
            Assert.NotNull(reservations);
           
            //Verifica se a reserva retornada tem como nome "Dinis Faryna"
            var firstReservation = reservations.FirstOrDefault();
            Assert.NotNull(firstReservation);
            Assert.Equal("Dinis Faryna", firstReservation.CustomerName);
        }



        [Fact]
        public async Task GetReservation_ExistingActiveReservation_ReturnsReservation()
        {
            var result = await _controller.GetReservation(1);
            Assert.NotNull(result);
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult);
            
            //Verifica se o status da reserva é ativo e se o nome da mesma é "João Costa"
            var reservation = Assert.IsAssignableFrom<Reservation>(okResult.Value);
            Assert.Equal(0, reservation.StatusReservation);
            Assert.Equal("João Costa", reservation.CustomerName);
        }

        [Fact]
        public async Task GetReservation_NonExistentReservation_ReturnsNotFound()
        {
            //Tenta retornar uma reserva que é enexistente
            var result = await _controller.GetReservation(999);

            // Verifica se retorna NotFound (404)
            Assert.IsType<NotFoundResult>(result);
        }

        
                

        [Fact]
        public async Task CreateReservation_ValidData_ReturnsOkAndStoresCorrectly()
        {
            var customerName = "João Silva";
            var resDate = new DateOnly(2025, 8, 10);
            var resTime = new TimeOnly(18, 30);
            var tableNumber = 1;
            var numOfPeople = 4;

            //Cria a reserva
            var createResult = await _controller.CreateReservation(
                customerName, resDate, resTime, tableNumber, numOfPeople);

            //Verifica se a criação foi bem sucedida
            var okCreateResult = Assert.IsType<OkObjectResult>(createResult);
            
            var newReservation = Assert.IsType<Reservation>(okCreateResult.Value);

            //Uso novamente do método GetReservation a passar o novo id gerado para retornar a reserva criada
            var getResult = await _controller.GetReservation(newReservation.Id);

            var okGetResult = Assert.IsType<OkObjectResult>(getResult);
           
            var storedReservation = Assert.IsType<Reservation>(okGetResult.Value);

            //Compara todos os campos criados com os retornados
            Assert.Equal(customerName, storedReservation.CustomerName);
            Assert.Equal(resDate, storedReservation.ReservationDate);
            Assert.Equal(resTime, storedReservation.ReservationTime);
            Assert.Equal(tableNumber, storedReservation.TableNumber);
            Assert.Equal(numOfPeople, storedReservation.NumberOfPeople);
            Assert.Equal(0, storedReservation.StatusReservation);
        }

        [Fact]
        public async Task CreateReservation_PastDateTime_ReturnsBadRequest()
        {
            //Cria uma reserva cujo a data é "anterior" a do dia de hoje de forma a dar erro
            var result = await _controller.CreateReservation(
                "Past Reservation",
                new DateOnly(2023, 1, 1),
                new TimeOnly(12, 0),
                4,
                2
            );

            // Verifica se o resultado da operação é do tipo BadRequestObjectResult, o que indica que a reserva falhou devido à data inválida
             Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateReservation_PartialUpdate_UpdatesSuccessfully()
        {
            //Faz o update dos valores de cada atributo na reserva com id = 1
            var result = await _controller.UpdateReservation(
                1,
                "Updated Name",
                new DateOnly(2025, 8, 10),
                new TimeOnly(18, 30),
                2,
                3
            );

            var okResult = Assert.IsType<OkObjectResult>(result);
            var reservation = Assert.IsAssignableFrom<Reservation>(okResult.Value);

            var getResult = await _controller.GetReservation(reservation.Id);
            var okGetResult = Assert.IsType<OkObjectResult>(getResult);
        
            var storedReservation = Assert.IsType<Reservation>(okGetResult.Value);

            //Compara todos os campos criados com os retornados
            Assert.Equal("Updated Name", storedReservation.CustomerName);
            Assert.Equal(new DateOnly(2025, 8, 10), reservation.ReservationDate);
            Assert.Equal(new TimeOnly(18, 30), reservation.ReservationTime);
            Assert.Equal(2, reservation.TableNumber);
            Assert.Equal(3, reservation.NumberOfPeople);
        }

        [Fact]
        public async Task DeleteReservation_ActiveReservation_SetsStatusToCancelled()
        {
            //Eliminamos a reserva 1
            var result = await _controller.DeleteReservation(4);
            var okResult = Assert.IsType<OkObjectResult>(result);
            var reservation = Assert.IsAssignableFrom<Reservation>(okResult.Value);

            // Verifica se o status da reserva foi atualizado para "Cancelado" (=1)
            Assert.Equal(1, reservation.StatusReservation);
        }
    }
}