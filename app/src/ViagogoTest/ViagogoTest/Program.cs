using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Viagogo
{
    public class Program
    {
        // I made below "Tasks" methods public and returning the IEnumerable<Events> just to be testable from UnitTests project
        // I also included a stopwatch just to evidence in the Console that Task3 is performing better than Task2 (of course it wouldn't be there in a real world context)
        // I have also changed the methods to be async, so we don't freeze the main thread
        // There's no method for Task4, because its implementation is the same as Task3, but errors are being handled in Main and retries in GetDistanceOptmized

        public static List<Event> Events { get; set; } = new List<Event>{
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

        private static ConcurrentDictionary<string, int> cacheDistances = new ConcurrentDictionary<string, int>();

        public static async Task Main(string[] args)
        {
            var customer = new Customer { Name = "Mr. Fake", City = "New York" };
            Stopwatch sw = new Stopwatch();

            try
            {
                Console.WriteLine("***** TASK 1 ****");
                sw.Start();

                await Task1Async(customer);

                sw.Stop();
                Console.WriteLine($"***** ELAPSED TIME: {sw.Elapsed} ****");
                Console.WriteLine();


                Console.WriteLine("***** TASK 2 ****");
                sw.Reset();
                sw.Start();

                await Task2Async(customer);

                sw.Stop();
                Console.WriteLine($"***** ELAPSED TIME: {sw.Elapsed} ****");
                Console.WriteLine();


                Console.WriteLine("***** TASK 3 ****");
                sw.Reset();
                sw.Start();

                await Task3Async(customer);

                sw.Stop();
                Console.WriteLine($"***** ELAPSED TIME: {sw.Elapsed} ****");
                Console.WriteLine();

                Console.WriteLine("***** TASK 5 ****");
                sw.Reset();
                sw.Start();

                await Task5Async(customer);

                sw.Stop();
                Console.WriteLine($"***** ELAPSED TIME: {sw.Elapsed} ****");
                Console.WriteLine("Press any key to end...");

                Console.ReadKey();
            }
            catch (Exception)
            {
                // Log the error
                Console.WriteLine("An unexpected error happened");
            }            
        }

        public async static Task<IEnumerable<Event>> Task1Async(Customer customer)
        {
            var customerEvents = Events.Where(e => e.City == customer.City);

            // Parallel may perform better here, as it's unordered and also has no limits of events
            await Parallel.ForEachAsync(customerEvents, async (e, c) =>
            {
                await AddToEmailAsync(customer, e);
            });

            return customerEvents.AsEnumerable();
        }

        public async static Task<IEnumerable<Event>> Task2Async(Customer customer)
        {
            // Parallel may increase performance here, as we depend on GetDistance() response
            var customerEvents = Events.AsParallel()
                                        .OrderBy(e => GetDistance(customer.City, e.City))
                                        .Take(5);

            foreach (var e in customerEvents)
            {
                await AddToEmailAsync(customer, e);
            }

            return customerEvents.AsEnumerable();
        }

        public static async Task<IEnumerable<Event>> Task3Async(Customer customer)
        {
            // Parallel may increase performance here, as we depend on GetDistance
            var customerEvents = Events.AsParallel()
                                        .Select(async e => new CustomerEvent
                                        {
                                            Event = new Event { Name = e.Name, City = e.City },
                                            Customer = customer,
                                            Distance = await GetDistanceOptimizedAsync(customer.City, e.City)
                                        })
                                        .Select(e => e.Result)
                                        .OrderBy(e => e.Distance)
                                        .Take(5);

            foreach (var e in customerEvents)
            {
                await AddToEmailAsync(customer, e.Event);
            }

            return customerEvents.Select(e => e.Event).AsEnumerable();
        }

        public async static Task<IEnumerable<Event>> Task5Async(Customer customer)
        {
            // As Price and Distance can have delays on their response, Parallel should perform better
            // Adding Price to returned object avoid to call the method again in the foreach in Add email and increase performance
            var customerEvents = Events.AsParallel() 
                                        .Select(async e => new CustomerEvent
                                        {
                                            Event = new Event { Name = e.Name, City = e.City, Price = await GetPriceAsync(e) },
                                            Customer = customer,
                                            Distance = await GetDistanceOptimizedAsync(customer.City, e.City)
                                        })
                                        .Select(e => e.Result)
                                        .OrderBy(e => e.Distance)
                                        .ThenBy(e => e.Event.Price)
                                        .Take(5);

            foreach (var ce in customerEvents)
            {
                await AddToEmailAsync(customer, ce.Event);
            }

            return customerEvents.Select(e => e.Event).AsEnumerable();
        }

        private static async Task<int> GetDistanceOptimizedAsync(string cityA, string cityB)
        {
            // Of course in a real world, these parameters would come from a config file
            int numberOfRetries = 5;
            int waitTimeMilliseconds = 1000;

            string key = GetCacheKey(cityA, cityB);
            int distance = -1;

            if (cacheDistances.TryGetValue(key, out distance))
                return distance;
            else
            {
                for (int i = 0; i < numberOfRetries; i++)
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            distance = GetDistance(cityA, cityB);
                        });

                        cacheDistances.TryAdd(key, distance);

                        return distance;
                    }
                    catch (TimeoutException)
                    {
                        Thread.Sleep(waitTimeMilliseconds);
                    }
                }
            }

            throw new Exception($"Unable to get distances after {numberOfRetries} retries");
        }

        static async Task<int> GetPriceAsync(Event e)
        {
            int price = 0;

            await Task.Run(() =>
            {
                price = GetPrice(e);
            });

            return price;
        }

        private static string GetCacheKey(string cityA, string cityB)
        {
            // Generating a key by putting cities in alphabetical order gurantee we don't have two keys for the same distance
            // Ex: "Chicago_New York" and "New York_Chicago" will always be "Chicago_New York"

            if (string.Compare(cityA, cityB) < 0)
                return cityA + "_" + cityB;
            else
                return cityB + "_" + cityA;
        }

        private static async Task AddToEmailAsync(Customer c, Event e)
        {
            var distance = await GetDistanceOptimizedAsync(c.City, e.City);
            Console.Out.WriteLine($"{c.Name}: {e.Name} in {e.City}"
            + (distance > 0 ? $" ({distance} miles away)" : "")
            + (e.Price.HasValue ? $" for ${e.Price}" : ""));
        }

        #region "Codebase Methods"
        static void AddToEmail(Customer c, Event e, int? price = null)
        {
            var distance = GetDistance(c.City, e.City);
            Console.Out.WriteLine($"{c.Name}: {e.Name} in {e.City}"
            + (distance > 0 ? $" ({distance} miles away)" : "")
            + (price.HasValue ? $" for ${price}" : ""));
        }

        // Adding delay to responses to make it realistic
        static int GetPrice(Event e)
        {
            Thread.Sleep(100);

            return (AlphebiticalDistance(e.City, "") + AlphebiticalDistance(e.Name, "")) / 10;
        }

        // Adding delay to responses to make it realistic
        static int GetDistance(string fromCity, string toCity)
        {
            Thread.Sleep(100);
            
            return AlphebiticalDistance(fromCity, toCity);
        }

        private static int AlphebiticalDistance(string s, string t)
        {
            var result = 0;
            var i = 0;
            for (i = 0; i < Math.Min(s.Length, t.Length); i++)
            {
                // Console.Out.WriteLine($"loop 1 i={i} {s.Length} {t.Length}");
                result += Math.Abs(s[i] - t[i]);
            }
            for (; i < Math.Max(s.Length, t.Length); i++)
            {
                // Console.Out.WriteLine($"loop 2 i={i} {s.Length} {t.Length}");
                result += s.Length > t.Length ? s[i] : t[i];
            }
            return result;
        }

        #endregion
    }
}
