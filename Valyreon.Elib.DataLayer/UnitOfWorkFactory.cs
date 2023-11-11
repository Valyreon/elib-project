using System.Threading;
using System.Threading.Tasks;
using Valyreon.Elib.DataLayer.Interfaces;

namespace Valyreon.Elib.DataLayer
{
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private static readonly SemaphoreSlim semaphore = new(1, 1);
        private readonly string connection;

        public UnitOfWorkFactory(string connection)
        {
            this.connection = connection;
        }

        public async Task<IUnitOfWork> CreateAsync()
        {
            await semaphore.WaitAsync();
            return new UnitOfWork(connection);
        }

        internal static void ReleaseSemaphore()
        {
            semaphore.Release();
        }
    }
}
