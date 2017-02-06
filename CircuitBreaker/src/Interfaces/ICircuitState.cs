using System;
using System.Threading.Tasks;

namespace Sleeksoft.CB
{
    public interface ICircuitState
    {
        void Enter();
        bool IsOpen { get; }
        bool IsHalfOpen { get; }
        bool IsClosed { get; }
        void ExecuteSync(Action command);
        T ExecuteSync<T>(Func<T> command);
        T ExecuteSync<T>(Func<T> command, Func<T> fallbackCommand);
        Task ExecuteAsync(Func<Task> command);
        Task<T> ExecuteAsync<T>(Func<Task<T>> command);
        Task<T> ExecuteAsync<T>(Func<Task<T>> command, Func<Task<T>> fallbackCommand);
    }
}