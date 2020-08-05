using System;
using System.Windows.Threading;

namespace MenthaAssembly
{
    public static class TimerHelper
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
