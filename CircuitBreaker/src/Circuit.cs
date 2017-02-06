using System;
using System.Threading;
using System.Threading.Tasks;

using Sleeksoft.CB.States;

namespace Sleeksoft.CB
{
    public class Circuit : ICircuit
    {
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
            this.Trip(m_CurrentState, m_StateOpen);
        }

        public void Close()
        {
            this.Trip(m_CurrentState, m_StateClosed);
        }

        public void AttemptToClose()
        {
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
    }
}