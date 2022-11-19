using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace MenthaAssembly
{
    public abstract class DispatcherQueue<T>
    {
        public Dispatcher Dispatcher { get; }

        public DispatcherPriority Priority { get; }

        private readonly Queue<T> Queue = new();
        protected DispatcherQueue(Dispatcher Dispatcher, DispatcherPriority Priority)
        {
            this.Dispatcher = Dispatcher;
            this.Dispatcher.ShutdownFinished += OnDispatcherShutdownFinished;

            this.Priority = Priority;
        }

        protected virtual void OnDispatcherShutdownFinished(object sender, EventArgs e)
        {
            Dispatcher.ShutdownFinished -= OnDispatcherShutdownFinished;
            Queue.Clear();
        }

        public void Add(T Key)
        {
            Queue.Enqueue(Key);
            Run();
        }

        public void Run()
        {
            if (IsUpdating)
                return;

#if NET462
            Dispatcher.BeginInvoke(new Action(RunAction), Priority);
#else
            Dispatcher.BeginInvoke(RunAction, Priority);
#endif
        }

        private void RunAction()
        {
            try
            {
                IsUpdating = true;
                InternalRun(Queue);
            }
            finally
            {

                IsUpdating = false;
            }
        }

        private bool IsUpdating = false;
        protected abstract void InternalRun(Queue<T> Queue);

    }
}