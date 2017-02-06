using System;
using System.Threading.Tasks;

using Sleeksoft.CB.Exceptions;

namespace Sleeksoft.CB.States
{
    #pragma warning disable CS1998 
    // The circuit is open. Therefore any call must fail-fast
    // until the circuit reset interval has elapsed.    
    internal class StateOpen : ICircuitState
    {
        private readonly ICircuit m_Circuit;
        private readonly ICommand m_Command;
        private readonly TimeSpan m_CircuitResetInterval;

        public StateOpen(ICircuit circuit, TimeSpan circuitResetInterval)
        {
            m_Circuit = circuit;
            m_CircuitResetInterval = circuitResetInterval;
            m_Command = new Command(TimeSpan.MaxValue);
        }

        public void Enter()
        {
            // Switch circuit state to half-open once the reset interval has elapsed.
            m_Command.ExecuteScheduled( () => m_Circuit.AttemptToClose(), m_CircuitResetInterval);
        }

        public bool IsOpen
        {
            get { return true; }
        }

        public bool IsHalfOpen
        {
            get { return false; }
        }

        public bool IsClosed
        {
            get { return false; }
        }

        private void CommandFailed()
        {
        }

        private void CommandSucceeded()
        {
        }

        public void ExecuteSync(Action command)
        {
            throw new CircuitBreakerOpenException();
        }

        public T ExecuteSync<T>(Func<T> command)
        {
            throw new CircuitBreakerOpenException();
        }

        public T ExecuteSync<T>(Func<T> command, Func<T> fallbackCommand)
        {
            throw new CircuitBreakerOpenException();
        }

        public async Task ExecuteAsync(Func<Task> command)
        {
            throw new CircuitBreakerOpenException();
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command)
        {
            throw new CircuitBreakerOpenException();
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command, Func<Task<T>> fallbackCommand)
        {
            throw new CircuitBreakerOpenException();
        }
    }
}