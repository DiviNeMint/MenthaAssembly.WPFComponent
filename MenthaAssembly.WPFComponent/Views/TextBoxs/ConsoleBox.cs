using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MenthaAssembly.Views
{
    public class ConsoleBox : TextBox
    {

        public static readonly DependencyProperty IsMonitorProperty =
            DependencyProperty.Register("IsMonitor", typeof(bool), typeof(ConsoleBox), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is ConsoleBox This)
                        if (e.NewValue is true)
                            This.Start();
                        else
                            This.Stop();
                }));
        public bool IsMonitor
        {
            get => (bool)GetValue(IsMonitorProperty);
            set => SetValue(IsMonitorProperty, value);
        }


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
            Console.SetOut(Grabber);        // Console
            // Debug
            // .Net Core's Listeners use together.
            //https://stackoverflow.com/questions/54342622/it-seems-that-debug-listeners-does-not-exist-in-net-core
            //Debug.Listeners.Add(Listener)   
            Trace.Listeners.Add(Listener);  // Debug、Trace
        }
        public void Stop()
        {
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput())   // Console
            {
                AutoFlush = true
            });
            Trace.Listeners.Remove(Listener);                               //Debug、Trace
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.GetTemplateChild("PART_ContentHost") is ScrollViewer PART_ContentHost)
                ScrollViewerHelper.SetAutoScrollToEnd(PART_ContentHost, true);
        }

        ~ConsoleBox()
        {
            if (IsMonitor)
                Stop();

            Grabber?.Dispose();
            Listener?.Dispose();
        }

        protected class ConsoleGrabber : StringWriter
        {
            private TextBox TextBox { get; }
            public ConsoleGrabber(TextBox TextBox)
                => this.TextBox = TextBox;

            public override void Write(string value)
                => TextBox.Text += value;

            public override void WriteLine()
                => TextBox.Text += NewLine;
        }
        protected class ConsoleListener : TraceListener
        {
            private TextBox TextBox { get; }
            public ConsoleListener(TextBox TextBox)
                => this.TextBox = TextBox;

            public override void Write(string message)
                => TextBox.Text += message;

            public override void WriteLine(string message)
                => TextBox.Text += $"{ message}{Environment.NewLine}";

        }

    }
}
