using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Viagogo.UnitTests
{
    public class TaskTests
    {
        [Fact]
        public async Task Task1_Success_AllEventsInCustomerCity()
        {
            // Arrange
            Program.Events = new List<Event>
                                {
                                    new Event { Name = "Phantom of the Opera", City = "New York" },
                                    new Event { Name = "Metallica", City = "Los Angeles" },
                                    new Event { Name = "Metallica", City = "New York" },
                                    new Event { Name = "Metallica", City = "Boston" },
                                    new Event { Name = "LadyGaGa", City = "New York" },
                                    new Event { Name = "LadyGaGa", City = "Boston" },
                                    new Event { Name = "LadyGaGa", City = "Chicago" },
                                    new Event { Name = "LadyGaGa", City = "San Francisco" },
                                    new Event { Name = "LadyGaGa", City = "Washington" }
                                };


            // Act
            var customer = new Customer { Name = "Mr. Fake", City = "New York" };
            var events = Program.Task1(customer);

            // Assert

            Assert.Collection<Event>(events,
                                        e => Assert.Equal(e.City, customer.City),
                                        e => Assert.Equal(e.City, customer.City),
                                        e => Assert.Equal(e.City, customer.City));
        }

        [Fact]
        public async Task Task2_Success_Top5NearbyEvents()
        {
            // Arrange
            Program.Events = new List<Event>
                                {
                                    new Event { Name = "Phantom of the Opera", City = "New York" },
                                    new Event { Name = "Metallica", City = "Los Angeles" },
                                    new Event { Name = "Metallica", City = "New York" },
                                    new Event { Name = "Metallica", City = "Boston" },
                                    new Event { Name = "LadyGaGa", City = "New York" },
                                    new Event { Name = "LadyGaGa", City = "Boston" },
                                    new Event { Name = "LadyGaGa", City = "Chicago" },
                                    new Event { Name = "LadyGaGa", City = "San Francisco" },
                                    new Event { Name = "LadyGaGa", City = "Washington" }
                                };


            // Act
            var customer = new Customer { Name = "Mr. Fake", City = "New York" };
            var events = await Program.Task2Async(customer);

            // Assert
            Assert.Collection<Event>(events,
                                        e => Assert.Equal("Phantom of the Opera", e.Name),
                                        e => Assert.Equal("Metallica", e.Name),
                                        e => Assert.Equal("LadyGaGa", e.Name),
                                        e => Assert.Equal("LadyGaGa", e.Name),
                                        e => Assert.Equal("LadyGaGa", e.Name));

            Assert.Collection<Event>(events,
                                        e => Assert.Equal("New York", e.City),
                                        e => Assert.Equal("New York", e.City),
                                        e => Assert.Equal("New York", e.City),
                                        e => Assert.Equal("Chicago", e.City),
                                        e => Assert.Equal("Washington", e.City));
        }

        [Fact]
        public async Task Task3_Success_Top5NearbyEventsOptmized()
        {
            // Arrange
            Program.Events = new List<Event>
                                {
                                    new Event { Name = "Phantom of the Opera", City = "New York" },
                                    new Event { Name = "Metallica", City = "Los Angeles" },
                                    new Event { Name = "Metallica", City = "New York" },
                                    new Event { Name = "Metallica", City = "Boston" },
                                    new Event { Name = "LadyGaGa", City = "New York" },
                                    new Event { Name = "LadyGaGa", City = "Boston" },
                                    new Event { Name = "LadyGaGa", City = "Chicago" },
                                    new Event { Name = "LadyGaGa", City = "San Francisco" },
                                    new Event { Name = "LadyGaGa", City = "Washington" }
                                };


            // Act
            var customer = new Customer { Name = "Mr. Fake", City = "New York" };

            Stopwatch sw = new Stopwatch();
            sw.Start();

            var events = await Program.Task2Async(customer);

            sw.Stop();
            long elapsedTimeTask2 = sw.ElapsedMilliseconds;

            sw.Reset();
            sw.Start();

            events = await Program.Task3Async(customer);

            sw.Stop();
            long elapsedTimeTask3 = sw.ElapsedMilliseconds;

            // Assert
            Assert.True(elapsedTimeTask2 > elapsedTimeTask3);

            Assert.Collection<Event>(events,
                                        e => Assert.Equal("Phantom of the Opera", e.Name),
                                        e => Assert.Equal("Metallica", e.Name),
                                        e => Assert.Equal("LadyGaGa", e.Name),
                                        e => Assert.Equal("LadyGaGa", e.Name),
                                        e => Assert.Equal("LadyGaGa", e.Name));

            Assert.Collection<Event>(events,
                                        e => Assert.Equal("New York", e.City),
                                        e => Assert.Equal("New York", e.City),
                                        e => Assert.Equal("New York", e.City),
                                        e => Assert.Equal("Chicago", e.City),
                                        e => Assert.Equal("Washington", e.City));
        }

        [Fact]
        public async Task Task4_Main_ShouldNotFail_WhenTaskFails()
        {
            // Arrange
            Program.Events = new List<Event> { new Event { Name = "Phantom of the Opera", City = null } };

            // Act
            await Program.Main(new string[] { });
        }

        [Fact]
        public async Task Task5_Success_Top5NearbyEventsOptimizedOrderedByPrice()
        {
            Program.Events = new List<Event>
                                {
                                    new Event { Name = "Phantom of the Opera", City = "New York" },
                                    new Event { Name = "Metallica", City = "Los Angeles" },
                                    new Event { Name = "Metallica", City = "New York" },
                                    new Event { Name = "Metallica", City = "Boston" },
                                    new Event { Name = "LadyGaGa", City = "New York" },
                                    new Event { Name = "LadyGaGa", City = "Boston" },
                                    new Event { Name = "LadyGaGa", City = "Chicago" },
                                    new Event { Name = "LadyGaGa", City = "San Francisco" },
                                    new Event { Name = "LadyGaGa", City = "Washington" }
                                };

            // Act
            var customer = new Customer { Name = "Mr. Fake", City = "New York" };
            var events = await Program.Task5Async(customer);

            // Assert
            Assert.Collection<Event>(events,
                e => Assert.Equal("LadyGaGa", e.Name),
                e => Assert.Equal("Metallica", e.Name),
                e => Assert.Equal("Phantom of the Opera", e.Name),
                e => Assert.Equal("LadyGaGa", e.Name),
                e => Assert.Equal("LadyGaGa", e.Name));

            Assert.Collection<Event>(events,
                e => Assert.Equal("New York", e.City),
                e => Assert.Equal("New York", e.City),
                e => Assert.Equal("New York", e.City),
                e => Assert.Equal("Chicago", e.City),
                e => Assert.Equal("Washington", e.City));

            Assert.Collection<Event>(events,
                e => Assert.Equal(148, e.Price),
                e => Assert.Equal(165, e.Price),
                e => Assert.Equal(261, e.Price),
                e => Assert.Equal(141, e.Price),
                e => Assert.Equal(178, e.Price));
        }

    }
}
