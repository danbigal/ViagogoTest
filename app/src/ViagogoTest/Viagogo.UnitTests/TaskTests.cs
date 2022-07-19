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
        public void Task1_Success_AllEventsInCustomerCity()
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
        public void Task2_Success_Top5NearbyEvents()
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
            var events = Program.Task2(customer);

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
        public void Task3_Success_Top5NearbyEventsOptmized()
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

            var events = Program.Task2(customer);

            sw.Stop();
            long elapsedTimeTask2 = sw.ElapsedMilliseconds;

            sw.Reset();
            sw.Start();

            events = Program.Task3(customer);

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
        public void Task4_Main_Task3ShouldNotFail_WhenDistanceError()
        {
            // Arrange
            Program.Events = new List<Event> { 
                                                new Event { Name = "Iron Maiden", City = null }, 
                                                new Event { Name = "Metallica", City = "New York" } 
                                            };

            var customer = new Customer { Name = "Mr. Fake", City = "New York" };

            // Act
            var events = Program.Task3(customer);

            // Assert
            Assert.Collection<Event>(events,
                e => Assert.Equal("Metallica", e.Name),
                e => Assert.Equal("Iron Maiden", e.Name));
        }

        [Fact]
        public void Task5_Success_Top5NearbyEventsOptimizedOrderedByPrice()
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
            var events = Program.Task5(customer);

            // Assert
            Assert.Collection<EventEnhanced>(events,
                e => Assert.Equal("LadyGaGa", e.Name),
                e => Assert.Equal("Metallica", e.Name),
                e => Assert.Equal("Phantom of the Opera", e.Name),
                e => Assert.Equal("LadyGaGa", e.Name),
                e => Assert.Equal("LadyGaGa", e.Name));

            Assert.Collection<EventEnhanced>(events,
                e => Assert.Equal("New York", e.City),
                e => Assert.Equal("New York", e.City),
                e => Assert.Equal("New York", e.City),
                e => Assert.Equal("Chicago", e.City),
                e => Assert.Equal("Washington", e.City));

            Assert.Collection<EventEnhanced>(events,
                e => Assert.Equal(148, e.Price),
                e => Assert.Equal(165, e.Price),
                e => Assert.Equal(261, e.Price),
                e => Assert.Equal(141, e.Price),
                e => Assert.Equal(178, e.Price));
        }

    }
}
