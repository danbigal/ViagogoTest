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
        public static List<Event> Events { get; set; }

        private static ConcurrentDictionary<string, int> cacheDistances = new ConcurrentDictionary<string, int>();

        static void Main(string[] args)
        {
            Events = new List<Event>{
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

            var customer = new Customer { Name = "Mr. Fake", City = "New York" };

            Console.WriteLine("***** TASK 1 ****");
            Task1(customer);

            Console.WriteLine("***** TASK 2 ****");
            Task2(customer);

            Console.WriteLine("***** TASK 3 ****");
            Task3(customer);

            Console.WriteLine("***** TASK 4 ****");
            Task4(customer);

            Console.WriteLine("***** TASK 5 ****");
            Task5(customer);

            Console.ReadKey();
        }

        public static IEnumerable<Event> Task1(Customer customer)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            var customerEvents = Events.Where(e => e.City == customer.City);

            // Parallel may perform better here, as it's unordered and also has no limits of events
            Parallel.ForEach(customerEvents, e =>
            {
                AddToEmail(customer, e);
            });

            sw.Stop();
            Console.WriteLine("Elapsed time: " + sw.Elapsed.ToString());

            return customerEvents.AsEnumerable();
        }

        public static IEnumerable<Event> Task2(Customer customer)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Parallel may increase performance here, as we depend on GetDistance() response
            var customerEvents = Events.AsParallel()
                                        .OrderBy(e => GetDistance(customer.City, e.City))
                                        .Take(5);

            foreach (var e in customerEvents)
            {
                AddToEmail(customer, e);
            }

            sw.Stop();
            Console.WriteLine("Elapsed time: " + sw.Elapsed.ToString());

            return customerEvents.AsEnumerable();
        }

        public static IEnumerable<Event> Task3(Customer customer)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Parallel may increase performance here, as we depend on GetDistance
            var customerEvents = Events.AsParallel()
                                        .OrderBy(e => GetDistanceOptimized(customer.City, e.City))
                                        .Take(5);

            foreach (var e in customerEvents)
            {
                AddToEmail(customer, e);
            }

            sw.Stop();
            Console.WriteLine("Elapsed time: " + sw.Elapsed.ToString());

            return customerEvents.AsEnumerable();
        }

        public static IEnumerable<Event> Task4(Customer customer)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // Parallel may increase performance here, as we depend on GetDistance
            var customerEvents = Events.AsParallel()
                                        .OrderBy(e => GetDistanceOptimized(customer.City, e.City))
                                        .Take(5);

            foreach (var e in customerEvents)
            {
                AddToEmail(customer, e);
            }

            sw.Stop();
            Console.WriteLine("Elapsed time: " + sw.Elapsed.ToString());

            return customerEvents.AsEnumerable();
        }

        public static IEnumerable<Event> Task5(Customer customer)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            // As Price and Distance can have delays on their response, Parallel should perform better
            // Adding Price to returned object avoid to call the method again in the foreach (makes it o log(n)) in Add email and increase performance
            var customerEvents = Events.AsParallel() 
                                        .Select(e => new Event { Name = e.Name, City = e.City, Price = GetPrice(e) })
                                        .OrderBy(e => GetDistanceOptimized(customer.City, e.City))
                                        .ThenBy(e => e.Price)
                                        .Take(10);

            foreach (var ce in customerEvents)
            {
                AddToEmail(customer, ce, ce.Price);
            }

            sw.Stop();
            Console.WriteLine("Elapsed time: " + sw.Elapsed.ToString());

            return customerEvents.AsEnumerable();
        }


        private static int GetDistanceOptimized(string cityA, string cityB)
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
                        distance = GetDistance(cityA, cityB);
                        cacheDistances.TryAdd(key, distance);

                        return distance;
                    }
                    catch (Exception)
                    {
                        Thread.Sleep(waitTimeMilliseconds);
                    }
                }
            }

            throw new Exception($"Unable to get distances after {numberOfRetries} retries");
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


        #region "Codebase Methods"
        static void AddToEmail(Customer c, Event e, int? price = null)
        {
            // Changing GetDistance to optmized version using cache 

            var distance = GetDistanceOptimized(c.City, e.City);
            Console.Out.WriteLine($"{c.Name}: {e.Name} in {e.City}"
            + (distance > 0 ? $" ({distance} miles away)" : "")
            + (price.HasValue ? $" for ${price}" : ""));
        }

        static int GetPrice(Event e)
        {
            // Adding delay in some responses
            Thread.Sleep(100);

            return (AlphebiticalDistance(e.City, "") + AlphebiticalDistance(e.Name, "")) / 10;
        }

        static int GetDistance(string fromCity, string toCity)
        {
            // Adding delay in some responses
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
