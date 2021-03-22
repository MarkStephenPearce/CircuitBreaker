using System;
using System.Threading.Tasks;

using Sleeksoft.CB.Exceptions;
using Sleeksoft.CB.Commands;

namespace Sleeksoft.CB.States
{
    #pragma warning disable CS1998 
    // The circuit is open. Therefore any call must fail-fast
    // until the circuit reset interval has elapsed.    
    internal class StateOpen : ICircuitState, IDisposable
    {
        private const string TYPE_NAME = "StateOpen";

        private readonly ICircuit m_Circuit;
        private readonly ICommand m_Command;
        private readonly TimeSpan m_CircuitResetInterval;

        public StateOpen(ICircuit circuit, TimeSpan circuitResetInterval)
        {
            m_Circuit = circuit;
            m_CircuitResetInterval = circuitResetInterval;
            m_Command = new Command(TimeSpan.MaxValue);
        }

        /// <summary>
        /// Has this type been disposed already?
        /// </summary>
        internal bool Disposed { get; private set; }

        public void Enter()
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

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
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            throw new CircuitBreakerOpenException();
        }

        public T ExecuteSync<T>(Func<T> command)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            throw new CircuitBreakerOpenException();
        }

        public T ExecuteSync<T>(Func<T> command, Func<T> fallbackCommand)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            throw new CircuitBreakerOpenException();
        }

        public async Task ExecuteAsync(Func<Task> command)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            throw new CircuitBreakerOpenException();
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            throw new CircuitBreakerOpenException();
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command, Func<Task<T>> fallbackCommand)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            throw new CircuitBreakerOpenException();
        }

        /// <summary>Cleans up state related to this type.</summary>
        /// <remarks>
        /// Don't make this method virtual. A derived type should 
        /// not be able to override this method.
        /// Because this type only disposes managed resources, it 
        /// don't need a finaliser. A finaliser isn't allowed to 
        /// dispose managed resources.
        /// Without a finaliser, this type doesn't need an internal 
        /// implementation of Dispose() and doesn't need to suppress 
        /// finalisation to avoid race conditions. So the full 
        /// IDisposable code pattern isn't required.
        /// </remarks>
        public void Dispose()
        {
            if (!this.Disposed)
            {
                this.Disposed = true;

                if (m_Command != null)
                {
                    m_Command.Dispose();
                }
            }
        }
    }
}