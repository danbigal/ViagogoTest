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

        public static void Main(string[] args)
        {
            var customer = new Customer { Name = "Mr. Fake", City = "New York" };
            Stopwatch sw = new Stopwatch();

            try
            {
                Console.WriteLine("***** TASK 1 ****");
                sw.Start();

                Task1(customer);

                sw.Stop();
                Console.WriteLine($"***** ELAPSED TIME: {sw.Elapsed} ****");
                Console.WriteLine();


                Console.WriteLine("***** TASK 2 ****");
                sw.Reset();
                sw.Start();

                Task2(customer);

                sw.Stop();
                Console.WriteLine($"***** ELAPSED TIME: {sw.Elapsed} ****");
                Console.WriteLine();


                Console.WriteLine("***** TASK 3 ****");
                sw.Reset();
                sw.Start();

                Task3(customer);

                sw.Stop();
                Console.WriteLine($"***** ELAPSED TIME: {sw.Elapsed} ****");
                Console.WriteLine();

                Console.WriteLine("***** TASK 5 ****");
                sw.Reset();
                sw.Start();

                Task5(customer);

                sw.Stop();
                Console.WriteLine($"***** ELAPSED TIME: {sw.Elapsed} ****");

                Console.WriteLine("***** TASK 6 ****");
                sw.Reset();
                sw.Start();

                Task6(customer);

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

        public static IEnumerable<Event> Task1(Customer customer)
        {
            var customerEvents = Events.Where(e => e.City == customer.City);

            // Parallel may perform better here, as it's unordered and also has no limits of events
            Parallel.ForEach(customerEvents, e =>
            {
                AddToEmail(customer, e);
            });

            return customerEvents;
        }

        // Is this GetDistance a call to API?? What if it fails??
        public static IEnumerable<Event> Task2(Customer customer)
        {
            // Parallel may increase performance here, as we depend on GetDistance() response
            var customerEvents = Events.AsParallel()
                                        .OrderBy(e => GetDistance(customer.City, e.City))
                                        .Take(5);

            foreach (var e in customerEvents)
            {
                AddToEmail(customer, e);
            }

            return customerEvents;
        }

        public static IEnumerable<Event> Task3(Customer customer)
        {
            // Parallel may increase performance here, as we depend on GetDistance
            var customerEvents = Events.AsParallel()
                                        .Select(async e => new EventEnhanced
                                        {
                                            Name = e.Name, 
                                            City = e.City,
                                            Distance = await GetDistanceOptimizedAsync(customer.City, e.City)
                                        })
                                        .Select(e => e.Result)
                                        .OrderBy(e => e.Distance)
                                        //It fails: async awaitn not permitted in OrderBy
                                        //.OrderBy(async e => await GetDistanceOptimizedAsync(customer.City, e.City)
                                        .Take(5);

            foreach (var e in customerEvents)
            {
                AddToEmailOptmized(customer, e);
            }

            return customerEvents;
        }

        public static IEnumerable<EventEnhanced> Task5(Customer customer)
        {
            // As Price and Distance can have delays on their response, Parallel should perform better
            // Adding Price to returned object avoid to call the method again in the foreach in Add email and increase performance
            var customEvents = Events.AsParallel()
                                        .Select(async e => new EventEnhanced
                                        {
                                            Name = e.Name,
                                            City = e.City,
                                            Distance = await GetDistanceOptimizedAsync(customer.City, e.City),
                                            Price = await GetPriceAsync(e),
                                        })
                                        .Select(e => e.Result)
                                        .OrderBy(e => e.Distance)
                                        .ThenBy(e => e.Price)
                                        .Take(5);

            foreach (var ce in customEvents)
            {
                AddToEmailOptmized(customer, ce);
            }

            return customEvents;
        }

        // What's the maximum difference days we will consider?
        // Should we return only the eventos closer to the birthday?? or if there's no event near, we should complete the TOP5?
        //      Ex: if there are only 2 events close to the birthday, should we return only these 2, or should wee return these 2 first and then the closer ones until complete the TOP5?
        // Should we keep ordering by distance, price and taking only the TOP5?
        public static IEnumerable<EventEnhanced> Task6 (Customer customer)
        {
            // As Price and Distance can have delays on their response, Parallel should perform better
            // Adding Price to returned object avoid to call the method again in the foreach in Add email and increase performance
            var customerEvents = Events.AsParallel()
                                        .Select(async e => new EventEnhanced
                                        {
                                            Name = e.Name,
                                            City = e.City,
                                            Distance = await GetDistanceOptimizedAsync(customer.City, e.City),
                                            Price = await GetPriceAsync(e),
                                        })
                                        .Select(e => e.Result)
                                        .Where(e =>
                                        {
                                            int maxDiffAllowed = 10;

                                            var eventDate = GetEventDate(e);
                                            var customerBirthday = GetBirthdayDate(customer);

                                            int daysDiff = Math.Abs((eventDate - new DateTime(eventDate.Year, customerBirthday.Month, customerBirthday.Day)).Days);
                                            daysDiff = Math.Min(daysDiff, Math.Abs((eventDate - new DateTime(eventDate.Year + 1, customerBirthday.Month, customerBirthday.Day)).Days));
                                            daysDiff = Math.Min(daysDiff, Math.Abs((eventDate - new DateTime(eventDate.Year -1 , customerBirthday.Month, customerBirthday.Day)).Days));

                                            return daysDiff <= maxDiffAllowed;

                                            // In case of OrderBy
                                            //return daysDiff;
                                        })
                                        .OrderBy(e => e.Distance)
                                        .ThenBy(e => e.Price)
                                        .Take(5);

            foreach (var e in customerEvents)
            {
                AddToEmailOptmized(customer, e);
            }

            return customerEvents;
        }

        private static DateTime GetBirthdayDate(Customer customer)
        {
            return new DateTime(1990, 01, 01);
        }

        private static DateTime GetEventDate(Event e)
        {
            return new DateTime(2022, 12, 29);
        }

        // Optmizing GetDistance to Get cache from Dictionary
        // In case of failure, is it OK to ignore the failed data and keep processing the other events? In this case, the customer may receive an email with his or her best fit missing. Is that acceptable?
        // Would a retry help??
        private static async Task<int> GetDistanceOptimizedAsync(string cityA, string cityB)
        {
            // Of course in a real world, these parameters would come from a config file
            //int numberOfRetries = 5;
            //int waitTimeMilliseconds = 1000;

            string key = cityA + "_" + cityB; //GetCacheKey(cityA, cityB);

            if (cacheDistances.TryGetValue(key, out int cacheDistance))
                return cacheDistance;
            else
            {
                var distance = Int32.MaxValue;

                //for (int i = 0; i < numberOfRetries; i++)
                //{
                try
                    {
                        distance = await Task.Run(() =>
                                    {
                                        return GetDistance(cityA, cityB);
                                    });

                        cacheDistances.TryAdd(key, distance);

                        return distance;
                    }
                    catch (Exception ex)
                    {
                        //Thread.Sleep(waitTimeMilliseconds);

                        // Log the error
                        Console.WriteLine("Error on getting event distance: " + ex.Message);
                    }
                //}

                return distance;
            }

            // throw new Exception($"Unable to get distances after {numberOfRetries} retries");
        }

        // As price can be slow, I would change it to async
        static async Task<int> GetPriceAsync(Event e)
        {
            int price = 0;

            await Task.Run(() =>
            {
                price = GetPrice(e);
            });

            return price;
        }

        // Are distances between 2 cities will be the same both sides trip?
        // I mean: New_York > Chicago can be considered same as Chicago > New York
        private static string GetCacheKey(string cityA, string cityB)
        {
            // Generating a key by putting cities in alphabetical order gurantee we don't have two keys for the same distance
            // Ex: "Chicago_New York" and "New York_Chicago" will always be "Chicago_New York"

            if (string.Compare(cityA, cityB) < 0)
                return cityA + "_" + cityB;
            else
                return cityB + "_" + cityA;
        }

        private static void AddToEmailOptmized(Customer c, EventEnhanced e)
        {
            Console.Out.WriteLine($"{c.Name}: {e.Name} in {e.City}"
            + (e.Distance > 0 ? $" ({e.Distance} miles away)" : "")
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
