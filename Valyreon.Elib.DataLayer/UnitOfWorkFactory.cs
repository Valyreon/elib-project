using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer.Interfaces;

namespace Valyreon.Elib.DataLayer
{
    public class UnitOfWorkFactory
    {
        private readonly string connection;

        private static readonly SemaphoreSlim semaphore = new(1, 1);

        public UnitOfWorkFactory(string connection)
        {
            this.connection = connection;
        }

        public async Task<IUnitOfWork> CreateAsync()
        {
#if DEBUG
            LogEntry();
#endif
            await semaphore.WaitAsync();
            return new UnitOfWork(connection);
        }

        internal static void ReleaseSemaphore()
        {
            semaphore.Release();
        }

#if DEBUG
        private static void LogEntry()
        {
            var holderCallstack = Environment.StackTrace;
            Console.WriteLine(DateTime.Now.ToString("s"));
            Console.WriteLine(string.Join("\n", holderCallstack.Split("\n").Take(10)));
            Console.WriteLine("++++++++++++++++++++++++");
        }
#endif
    }
}
