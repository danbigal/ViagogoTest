using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Xunit;

namespace Viagogo.UnitTests
{
    public class TaskTests
    {
        [Fact]
        public void Task1_Success()
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
        public void Task2_Success()
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
        public void Task3_Success()
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

    }
}
