using MenthaAssembly.MarkupExtensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MenthaAssembly.Views
{
    public class ConsoleBox : TextBox
    {
        public static readonly DependencyPropertyKey IsMonitoringPropertyKey =
            DependencyProperty.RegisterReadOnly("IsMonitoring", typeof(bool), typeof(ConsoleBox), new PropertyMetadata(false));
        public bool IsMonitoring
            => (bool)GetValue(IsMonitoringPropertyKey.DependencyProperty);

        protected ConsoleGrabber Grabber { get; }

        protected TraceListener Listener { get; }

        static ConsoleBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ConsoleBox), new FrameworkPropertyMetadata(typeof(ConsoleBox)));
        }

        public ConsoleBox()
        {
            Grabber = new ConsoleGrabber(this);
            Listener = new ConsoleListener(this);
        }

        public void Start()
        {
            InternalStart();
            SetValue(IsMonitoringPropertyKey, true);
        }
        protected virtual void InternalStart()
        {
            // Console
            Console.SetOut(Grabber);

            // Debug
            // .Net Core's Listeners use together.
            //https://stackoverflow.com/questions/54342622/it-seems-that-debug-listeners-does-not-exist-in-net-core
            //Debug.Listeners.Add(Listener)   

            // Trace
            Trace.Listeners.Add(Listener);
        }

        public void Stop()
        {
            InternalStop();
            SetValue(IsMonitoringPropertyKey, false);
        }
        protected virtual void InternalStop()
        {
            // Console
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

            // Debug、Trace
            Trace.Listeners.Remove(Listener);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_ContentHost") is ScrollViewer PART_ContentHost)
                ScrollViewerEx.SetAutoScrollToEnd(PART_ContentHost, true);
        }

        ~ConsoleBox()
        {
            InternalStop();
            Grabber.Dispose();
            Listener.Dispose();
        }

        protected class ConsoleGrabber : StringWriter
        {
            private TextBox TextBox { get; }
            public ConsoleGrabber(TextBox TextBox)
            {
                this.TextBox = TextBox;
            }

            public override void Write(string Value)
                => TextBox.Dispatcher.BeginInvoke(new Action(() => TextBox.Text += Value));

            public override void WriteLine()
                => TextBox.Dispatcher.BeginInvoke(new Action(() => TextBox.Text += NewLine));

        }
        protected class ConsoleListener : TraceListener
        {
            private TextBox TextBox { get; }
            public ConsoleListener(TextBox TextBox)
            {
                this.TextBox = TextBox;
            }

            public override void Write(string Message)
                => TextBox.Dispatcher.BeginInvoke(new Action(() => TextBox.Text += Message));

            public override void WriteLine(string Message)
                => TextBox.Dispatcher.BeginInvoke(new Action(() => TextBox.Text += $"{Message}{Environment.NewLine}"));

        }

    }
}