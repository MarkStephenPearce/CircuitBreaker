using System;
using System.Threading;
using System.Threading.Tasks;

using Sleeksoft.CB.Exceptions;

namespace Sleeksoft.CB.States
{
    // The circuit is half-open. The first call to be 
    // attempted will switch the circuit state depending
    // on the call's success or failure.
    internal class StateHalfOpen : ICircuitState
    {
        private const int CALL_RUNNING = 1;
        private const int CALL_NOT_RUNNING = 0;

        private readonly ICircuit m_Circuit;
        private readonly ICommand m_Command;

        private int m_IsCallRunning;

        public StateHalfOpen(ICircuit circuit, TimeSpan commandTimeout)
        {
            m_Circuit = circuit;
            m_Command = new Command(commandTimeout);
        }

        public void Enter()
        {
            m_IsCallRunning = CALL_NOT_RUNNING;
        }

        public bool IsOpen
        {
            get { return false; }
        }

        public bool IsHalfOpen
        {
            get { return true; }
        }

        public bool IsClosed
        {
            get { return false; }
        }

        public void CommandFailed()
        {
            m_Circuit.Open();
        }

        public void CommandSucceeded()
        {
            m_Circuit.Close();
        }

        // Execute synchronous command without result.
        public void ExecuteSync(Action command)
        {
            if ( Interlocked.CompareExchange(ref m_IsCallRunning, CALL_RUNNING, CALL_NOT_RUNNING) == CALL_NOT_RUNNING )
            {
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
            else
            {
                throw new CircuitBreakerOpenException();
            }
        }

        // Execute synchronous command with result.
        public T ExecuteSync<T>(Func<T> command)
        {
            if ( Interlocked.CompareExchange(ref m_IsCallRunning, CALL_RUNNING, CALL_NOT_RUNNING) == CALL_NOT_RUNNING )
            {
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
            else
            {
                throw new CircuitBreakerOpenException();
            }
        }

        // Execute synchronous command with result
        // Then execute fallback command if first command failed.
        // NB Only first command affects the circuit breaker.
        public T ExecuteSync<T>(Func<T> command, Func<T> fallbackCommand)
        {
            if ( Interlocked.CompareExchange(ref m_IsCallRunning, CALL_RUNNING, CALL_NOT_RUNNING) == CALL_NOT_RUNNING )
            {
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
            else
            {
                throw new CircuitBreakerOpenException();
            }
        }

        // Execute asynchronous command without result.
        public async Task ExecuteAsync(Func<Task> command)
        {
            if ( Interlocked.CompareExchange(ref m_IsCallRunning, CALL_RUNNING, CALL_NOT_RUNNING) == CALL_NOT_RUNNING )
            {
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
            else
            {
                throw new CircuitBreakerOpenException();
            }
        }

        // Execute asynchronous command with result.
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command)
        {
            if ( Interlocked.CompareExchange(ref m_IsCallRunning, CALL_RUNNING, CALL_NOT_RUNNING) == CALL_NOT_RUNNING )
            {
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
            else
            {
                throw new CircuitBreakerOpenException();
            }
        }

        // Execute asynchronous command with result
        // Then execute fallback command if first command failed.
        // NB Only first command affects the circuit breaker.
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command, Func<Task<T>> fallbackCommand)
        {
            if ( Interlocked.CompareExchange(ref m_IsCallRunning, CALL_RUNNING, CALL_NOT_RUNNING) == CALL_NOT_RUNNING )
            {
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
            else
            {
                throw new CircuitBreakerOpenException();
            }
        }
    }
}