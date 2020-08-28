using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace MenthaAssembly
{
    public sealed class Animation : FreezableCollection<Timeline>
    {
        public event EventHandler Completed;

        internal void OnCompleted()
            => Completed?.Invoke(this, EventArgs.Empty);

    }
}
