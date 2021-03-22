using System;
using System.Threading;
using System.Threading.Tasks;

using Sleeksoft.CB.Commands;

namespace Sleeksoft.CB.States
{
    // The circuit is closed. Therefore any call will be attempted.
    internal class StateClosed : ICircuitState
    {
        private const string TYPE_NAME = "StateClosed";

        private readonly ICircuit m_Circuit;
        private readonly ICommand m_Command;
        private readonly int m_MaxFailuresBeforeTrip;

        private int m_FailureCount;

        public StateClosed(ICircuit circuit, TimeSpan commandTimeout, int maxFailuresBeforeTrip)
        {
            m_Circuit = circuit;
            m_Command = new Command(commandTimeout);
            m_MaxFailuresBeforeTrip = maxFailuresBeforeTrip;
        }

        /// <summary>
        /// Has this type been disposed already?
        /// </summary>
        internal bool Disposed { get; private set; }

        public void Enter()
        {
            m_FailureCount = 0;
        }

        public bool IsOpen
        {
            get { return false; }
        }

        public bool IsHalfOpen
        {
            get { return false; }
        }

        public bool IsClosed
        {
            get { return true; }
        }

        // Execute synchronous command without result.
        public void ExecuteSync(Action command)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            bool exceptionHappened = true;

            try
            {
                m_Command.ExecuteSync(command);
                exceptionHappened = false;
            }
            finally
            {
                if ( exceptionHappened )
                {
                    this.CommandFailed();
                }
                else
                {
                    this.CommandSucceeded();
                }
            }
        }

        // Execute synchronous command with result.
        public T ExecuteSync<T>(Func<T> command)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            T result = default(T);
            bool exceptionHappened = true;

            try
            {
                result = m_Command.ExecuteSync(command);
                exceptionHappened = false;
            }
            finally
            {
                if ( exceptionHappened )
                {
                    this.CommandFailed();
                }
                else
                {
                    this.CommandSucceeded();
                }
            }

            return result;
        }

        // Execute synchronous command with result
        // Then execute fallback command if first command failed.
        // NB Only first command affects the circuit breaker.
        public T ExecuteSync<T>(Func<T> command, Func<T> fallbackCommand)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            T result = default(T);

            try
            {
                result = m_Command.ExecuteSync(command);
                this.CommandSucceeded();
            }
            catch ( Exception )
            {
                this.CommandFailed();
                result = m_Command.ExecuteSync(fallbackCommand);
            }

            return result;
        }

        // Execute asynchronous command without result.
        public async Task ExecuteAsync(Func<Task> command)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            bool exceptionHappened = true;

            try
            {
                await m_Command.ExecuteAsync(command);
                exceptionHappened = false;
            }
            finally
            {
                if ( exceptionHappened )
                {
                    this.CommandFailed();
                }
                else
                {
                    this.CommandSucceeded();
                }
            }
        }

        // Execute asynchronous command with result.
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            Task<T> task = default(Task<T>);
            bool exceptionHappened = true;

            try
            {
                task = m_Command.ExecuteAsync(command);
                await task;
                exceptionHappened = false;
            }
            finally
            {
                if ( exceptionHappened )
                {
                    this.CommandFailed();
                }
                else
                {
                    this.CommandSucceeded();
                }
            }

            return await task;
        }

        // Execute asynchronous command with result
        // Then execute fallback command if first command failed.
        // NB Only first command affects the circuit breaker.
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command, Func<Task<T>> fallbackCommand)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            Task<T> task = default(Task<T>);

            try
            {
                task = m_Command.ExecuteAsync(command);
                await task;
                this.CommandSucceeded();
            }
            catch ( Exception )
            {
                this.CommandFailed();
                task = m_Command.ExecuteAsync(fallbackCommand);
                await task;
            }

            return await task;
        }

        private void CommandFailed()
        {
            if ( Interlocked.Increment(ref m_FailureCount) == m_MaxFailuresBeforeTrip )
            {
                m_Circuit.Open();
            }
        }

        private void CommandSucceeded()
        {
            m_FailureCount = 0;
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