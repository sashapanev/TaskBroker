using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskCoordinator.Database
{
    public interface IConnectionErrorHandler
    {
        Task Handle(Exception ex, CancellationToken cancelation);
    }
}