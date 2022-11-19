using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace MenthaAssembly
{
    public static class DispatcherHelper
    {
        public static DelayActionToken DelayAction(double Milliseconds, Action Action)
            => DelayAction(Milliseconds, Action, DispatcherPriority.Normal);
        public static DelayActionToken DelayAction(double Milliseconds, Action Action, DispatcherPriority Priority)
        {
            DispatcherTimer Timer = new DispatcherTimer(Priority)
            {
                Interval = TimeSpan.FromMilliseconds(Milliseconds),
                Tag = Action
            };

            Timer.Tick += OnTimerTick;
            Timer.Start();
            return new DelayActionToken(Timer, (t) =>
            {
                t.Stop();
                t.Tick -= OnTimerTick;
            });
        }
        public static DelayActionToken DelayAction(double Milliseconds, Action Action, Action CancelAction)
            => DelayAction(Milliseconds, Action, CancelAction, DispatcherPriority.Normal);
        public static DelayActionToken DelayAction(double Milliseconds, Action Action, Action CancelAction, DispatcherPriority Priority)
        {
            DispatcherTimer Timer = new DispatcherTimer(Priority)
            {
                Interval = TimeSpan.FromMilliseconds(Milliseconds),
                Tag = Action
            };

            Timer.Tick += OnTimerTick;
            Timer.Start();
            return new DelayActionToken(Timer, (t) =>
            {
                t.Stop();
                t.Tick -= OnTimerTick;

                CancelAction?.Invoke();
            });
        }

        public static double Timing(Action Action, bool WriteLine = true)
        {
            if (Action is null)
                return -1;

            Stopwatch Sw = new Stopwatch();
            try
            {
                Sw.Start();
                Action.Invoke();
            }
            finally
            {
                Sw.Stop();

                if (WriteLine)
                    Debug.WriteLine($"Total : {Sw.Elapsed.TotalMilliseconds} ms.");
            }

            return Sw.Elapsed.TotalMilliseconds;
        }

        public static void InvokeSync(this Dispatcher This, Action Action)
        {
            if (This.CheckAccess())
                Action.Invoke();
            else
                This.Invoke(Action);
        }

        private static void OnTimerTick(object sender, EventArgs e)
        {
            if (sender is DispatcherTimer Timer)
            {
                Timer.Stop();
                Timer.Tick -= OnTimerTick;

                if (Timer.Tag is Action Action)
                    Action();

                Timer = null;
            }
        }

    }

}