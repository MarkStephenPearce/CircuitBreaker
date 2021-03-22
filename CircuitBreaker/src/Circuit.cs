using System;
using System.Threading;
using System.Threading.Tasks;

using Sleeksoft.CB.States;

namespace Sleeksoft.CB
{
    public class Circuit : ICircuit, IDisposable
    {
        private const string TYPE_NAME = "Circuit";

        private ICircuitState m_StateClosed;
        private ICircuitState m_StateHalfOpen;
        private ICircuitState m_StateOpen;

        private ICircuitState m_CurrentState;

        public Circuit(int maxFailuresBeforeTrip, TimeSpan commandTimeout, TimeSpan resetInterval)
        {
            m_StateClosed = new StateClosed(this, commandTimeout, maxFailuresBeforeTrip);
            m_StateHalfOpen = new StateHalfOpen(this, commandTimeout);
            m_StateOpen = new StateOpen(this, resetInterval);

            m_CurrentState = m_StateClosed;
        }

        /// <summary>
        /// Has this type been disposed already?
        /// </summary>
        internal bool Disposed { get; private set; }

        public ICircuitState CurrentState
        {
            get { return m_CurrentState; }
        }

        public bool IsOpen
        {
            get { return m_CurrentState.IsOpen; }
        }

        public bool IsHalfOpen
        {
            get { return m_CurrentState.IsHalfOpen; }
        }

        public bool IsClosed
        {
            get { return m_CurrentState.IsClosed; }
        }

        public void Open()
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            this.Trip(m_CurrentState, m_StateOpen);
        }

        public void Close()
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            this.Trip(m_CurrentState, m_StateClosed);
        }

        public void AttemptToClose()
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            this.Trip(m_CurrentState, m_StateHalfOpen);
        }

        private void Trip(ICircuitState stateFrom, ICircuitState stateTo)
        {
            if ( Interlocked.CompareExchange(ref m_CurrentState, stateTo, stateFrom) == stateFrom )
            {
                stateTo.Enter();
            }
        }

        public void ExecuteSync(Action command)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            if ( command == null )
            {
                throw new ArgumentNullException("command");
            }
            else
            {
                m_CurrentState.ExecuteSync(command);
            }
        }

        public T ExecuteSync<T>(Func<T> command)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            if ( command == null )
            {
                throw new ArgumentNullException("command");
            }
            else
            {
                return m_CurrentState.ExecuteSync(command);
            }
        }

        public T ExecuteSync<T>(Func<T> command, Func<T> fallbackCommand)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            if ( command == null )
            {
                throw new ArgumentNullException("command");
            }
            else if ( fallbackCommand == null )
            {
                throw new ArgumentNullException("fallbackCommand");
            }
            else
            {
                return m_CurrentState.ExecuteSync(command, fallbackCommand);
            }
        }

        public async Task ExecuteAsync(Func<Task> command)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            if ( command == null )
            {
                throw new ArgumentNullException("command");
            }
            else
            {
                await m_CurrentState.ExecuteAsync(command);
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            if ( command == null )
            {
                throw new ArgumentNullException("command");
            }
            else
            {
                return await m_CurrentState.ExecuteAsync(command);
            }
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> command, Func<Task<T>> fallbackCommand)
        {
            if (this.Disposed)
            {
                throw new ObjectDisposedException(TYPE_NAME);
            }

            if ( command == null )
            {
                throw new ArgumentNullException("command");
            }
            else if ( fallbackCommand == null )
            {
                throw new ArgumentNullException("fallbackCommand");
            }
            else
            {
                return await m_CurrentState.ExecuteAsync(command, fallbackCommand);
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
            if (!this.Disposed)
            {
                this.Disposed = true;

                if (m_StateClosed != null)
                {
                    m_StateClosed.Dispose();
                }
                if (m_StateOpen != null)
                {
                    m_StateOpen.Dispose();
                }
                if (m_StateHalfOpen != null)
                {
                    m_StateHalfOpen.Dispose();
                }
            }
        }
    }
}