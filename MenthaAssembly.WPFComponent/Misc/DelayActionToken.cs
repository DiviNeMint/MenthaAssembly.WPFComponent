using System;
using System.Windows.Threading;

namespace MenthaAssembly
{
    public class DelayActionToken
    {
        public DispatcherTimer Timer { get; private set; }

        private readonly Action<DispatcherTimer> CancelAction;
        internal DelayActionToken(DispatcherTimer Timer, Action<DispatcherTimer> CancelAction)
        {
            this.Timer = Timer;
            this.CancelAction = CancelAction;
        }

        public void Cancel()
        {
            if (Timer != null)
            {
                CancelAction?.Invoke(Timer);
                Timer = null;
            }
        }
    }

}
