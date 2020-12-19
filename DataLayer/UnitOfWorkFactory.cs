using System.Threading;
using System.Threading.Tasks;

namespace DataLayer
{
    public class UnitOfWorkFactory
    {
        private readonly string connection;

        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public UnitOfWorkFactory(string connection)
        {
            this.connection = connection;
        }

        public async Task<UnitOfWork> CreateAsync()
        {
            await semaphore.WaitAsync();
            return new UnitOfWork(connection);
        }

        public UnitOfWork Create()
        {
            semaphore.Wait();
            return new UnitOfWork(connection);
        }

        internal static void ReleaseSemaphore()
        {
            semaphore.Release();
        }
    }
}
