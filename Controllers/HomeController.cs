using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyPerfectApp.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace MyPerfectApp.Controllers
{
    public class HomeController : Controller
    {

        object o1 = new object();
        object o2 = new object();

        private static Processor p = new Processor();
        int[] differentSizes = new int[] { 1028, 512, 5012, 12345, 12348 };

        ArrayList objectList = new ArrayList();

        private readonly Random _random = new Random();

        private Array SaveObjects(int objectId)
        {
            int numbers = objectId / 8;
            int[] arr = new int[numbers];

            for (int i = 0; i < numbers; i++)
            {
                arr[i] = i;
            }

            return arr;
        }

        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            int milliseconds = 6000;
            var watch = new Stopwatch();
            watch.Start();

            while (true)
            {
                watch.Stop();
                if (watch.ElapsedMilliseconds > milliseconds)
                    break;
                watch.Start();
            }


            return View();
        }

        public IActionResult Report()
        {
            (new System.Threading.Thread(() =>
            {
                DeadlockFunc();
            })).Start();

            Thread.Sleep(5000);

            var threads = new Thread[300];
            for (int i = 0; i < 300; i++)
            {
                (threads[i] = new Thread(() =>
                {
                    lock (o1) { Thread.Sleep(100); }
                })).Start();
            }

            foreach (Thread thread in threads)
            {
                thread.Join();
            }


            throw new Exception("bad, bad code");
            return View();
        }

        public IActionResult Privacy()
        {
            var id = 1000 * 1024;
            var objectId = SaveObjects(id);
            objectList.AddRange(objectId);

            int seconds = 60;
            var watch = new Stopwatch();
            watch.Start();

            while (true)
            {
                p = new Processor();
                watch.Stop();
                if (watch.ElapsedMilliseconds > seconds * 1000)
                    break;
                watch.Start();

                int it = (2000 * 1000);
                for (int i = 0; i < it; i++)
                {
                    p.ProcessTransaction(new Customer(Guid.NewGuid().ToString()));
                }

                Thread.Sleep(5000); // Sleep for 5 seconds before cleaning up

                // Cleanup
                p = null;

                // GC
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Thread.Sleep(5000); // Sleep for 5 seconds before spiking memory again

                Random random = new Random();
                int kb = random.Next(0, differentSizes.Length);

                int iteration = (differentSizes[kb] * 1000) / 100;
                for (int i = 0; i < iteration; i++)
                {
                    p.ProcessTransaction(new Customer(Guid.NewGuid().ToString()));
                }


            }

            return View();
        }


        public IActionResult About()
        {
            Random rnd = new Random();
            int sizeInMB = rnd.Next(100, 500);

            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

            foreach (var file in dir.EnumerateFiles("*.txt"))
            {
                file.Delete();
            }

            string filename = Directory.GetCurrentDirectory() + @"\"+ RandomString(10, true) + ".txt";

            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                fs.SetLength(sizeInMB * 1024 * 1024);
            }

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        #region HelperMethods

        private void DeadlockFunc()
        {
            lock (o1)
            {
                (new Thread(() =>
                {
                    lock (o2) { Monitor.Enter(o1); }
                })).Start();

                Thread.Sleep(2000);
                Monitor.Enter(o2);
            }
        }

        public int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        public string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length=26  

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }


        #endregion HelperMethods
    }

    #region HelperClasses
    class Customer
        {
            private string id;

            public Customer(string id)
            {
                this.id = id;
            }
        }

        class CustomerCache
        {
            private List<Customer> cache = new List<Customer>();

            public void AddCustomer(Customer c)
            {
                cache.Add(c);
            }
        }

        class Processor
        {
            private CustomerCache cache = new CustomerCache();

            public void ProcessTransaction(Customer customer)
            {
                cache.AddCustomer(customer);
            }
        }
    }

#endregion HelperClasses
