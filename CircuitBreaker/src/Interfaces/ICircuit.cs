using System;
using System.Threading.Tasks;

namespace Sleeksoft.CB
{
    internal interface ICircuit
    {
        ICircuitState CurrentState { get; }
        bool IsOpen { get; }
        bool IsHalfOpen { get; }
        bool IsClosed { get; }
        void Close();
        void AttemptToClose();
        void Open();
        void ExecuteSync(Action command);
        T ExecuteSync<T>(Func<T> command);
        Task ExecuteAsync(Func<Task> command);
        Task<T> ExecuteAsync<T>(Func<Task<T>> command);
    }
}