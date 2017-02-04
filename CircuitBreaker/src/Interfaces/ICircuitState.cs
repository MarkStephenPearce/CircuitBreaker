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
        Task ExecuteAsync(Func<Task> command);
        Task<T> ExecuteAsync<T>(Func<Task<T>> command);
    }
}