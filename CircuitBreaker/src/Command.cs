using System;
using System.Threading;
using System.Threading.Tasks;

using Sleeksoft.CB.Exceptions;

namespace Sleeksoft.CB
{
    class Command : ICommand, IDisposable
    {
        private Timer m_Timer;
        private readonly TimeSpan m_CommandTimeout;

        public Command(TimeSpan commandTimeout)
        {
            m_CommandTimeout = commandTimeout;
        }

        /// <summary>
        /// Has this type been disposed already?
        /// </summary>
        internal bool Disposed { get; private set; }

        /// <summary>
        /// Currently only called to switch circuit from open to 
        /// half-open after circuit reset interval has elapsed.
        /// </summary>
        public void ExecuteScheduled(Action command, TimeSpan waitInterval)
        {
            if ( this.Disposed )
            {
                throw new ObjectDisposedException("Command");
            }

            m_Timer = new Timer(_ => command(), null, (int) waitInterval.TotalMilliseconds, Timeout.Infinite);
        }

        // Implementation of synchronous call without result.
        public void ExecuteSync(Action command)
        {
            if ( this.Disposed )
            {
                throw new ObjectDisposedException("Command");
            }

            var task = Task.Run(command);
            try
            {
                if ( task.IsCompleted || task.Wait((int) m_CommandTimeout.TotalMilliseconds) )
                {
                    return;
                }
            }
            catch ( AggregateException ae )
            {
                // We want to see the real exception.
                throw ae.InnerException;
            }

            throw new CircuitBreakerTimeoutException();
        }

        // Implementation of synchronous call with result.
        public T ExecuteSync<T>(Func<T> command)
        {
            if ( this.Disposed )
            {
                throw new ObjectDisposedException("Command");
            }

            var task = Task.Run(command);
            try
            {
                if ( task.IsCompleted || task.Wait((int) m_CommandTimeout.TotalMilliseconds) )
                {
                    return task.Result;
                }
            }
            catch ( AggregateException ae )
            {
                // We want to see the real exception.
                throw ae.InnerException;
            }

            throw new CircuitBreakerTimeoutException();
        }

        // Implementation of asynchronous call without result.
        public async Task ExecuteAsync(Func<Task> command)
        {
            if ( this.Disposed )
            {
                throw new ObjectDisposedException("Command");
            }

            try
            {
                await Task.Run(command).TimeoutAfter(m_CommandTimeout);
            }
            catch ( AggregateException ae )
            {
                // We want to see the real exception.
                throw ae.InnerException;
            }
        }

        // Implementation of asynchronous call with a result.
        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command)
        {
            if ( this.Disposed )
            {
                throw new ObjectDisposedException("Command");
            }

            try
            {
                return await Task<T>.Run(command).TimeoutAfter(m_CommandTimeout);
            }
            catch ( AggregateException ae )
            {
                // We want to see the real exception.
                throw ae.InnerException;
            }
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
            if ( !this.Disposed )
            {
                this.Disposed = true;

                if ( m_Timer != null )
                {
                    m_Timer.Dispose();
                }
            }
        }
    }
}