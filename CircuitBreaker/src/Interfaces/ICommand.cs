using System;
using System.Threading.Tasks;

namespace Sleeksoft.CB
{
    public interface ICommand
    {
        void ExecuteScheduled(Action command, TimeSpan waitInterval);
        void ExecuteSync(Action command);
        T ExecuteSync<T>(Func<T> command);
        Task ExecuteAsync(Func<Task> command);
        Task<T> ExecuteAsync<T>(Func<Task<T>> command);
        void Dispose();
    }
}