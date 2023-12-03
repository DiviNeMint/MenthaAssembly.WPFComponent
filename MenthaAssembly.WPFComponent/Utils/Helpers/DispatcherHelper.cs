using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace MenthaAssembly
{
    public static class DispatcherHelper
    {
        public static DelayActionToken DelayAction(double Milliseconds, Action Action)
            => DelayAction(Milliseconds, Action, DispatcherPriority.Normal);
        public static DelayActionToken DelayAction(double Milliseconds, Action Action, DispatcherPriority Priority)
        {
            DispatcherTimer Timer = new(Priority)
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
            DispatcherTimer Timer = new(Priority)
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

            Stopwatch Sw = new();
            try
            {
                Sw.Start();
                Action.Invoke();
            }
            finally
            {
                Sw.Stop();

                if (WriteLine)
#if DEBUG
                    Debug.WriteLine($"Total : {Sw.Elapsed.TotalMilliseconds} ms.");
#else
                    Trace.WriteLine($"Total : {Sw.Elapsed.TotalMilliseconds} ms.");
#endif
            }

            return Sw.Elapsed.TotalMilliseconds;
        }

        public static void Invoke(this DispatcherObject This, Action Action)
        {
            if (This.CheckAccess())
                Action.Invoke();
            else
                This.Dispatcher.Invoke(Action);
        }
        public static void Invoke(this DispatcherObject This, Action Action, DispatcherPriority Priority)
        {
            if (This.CheckAccess())
                Action.Invoke();
            else
                This.Dispatcher.Invoke(Action, Priority);
        }
        public static T Invoke<T>(this DispatcherObject This, Func<T> Function)
            => This.CheckAccess() ? Function.Invoke() : This.Dispatcher.Invoke(Function);
        public static T Invoke<T>(this DispatcherObject This, Func<T> Function, DispatcherPriority Priority)
            => This.CheckAccess() ? Function.Invoke() : This.Dispatcher.Invoke(Function, Priority);

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