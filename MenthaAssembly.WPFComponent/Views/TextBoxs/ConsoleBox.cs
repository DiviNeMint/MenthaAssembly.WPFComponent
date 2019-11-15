using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MenthaAssembly.Views
{
    public class ConsoleBox : TextBox
    {
        protected ConsoleGrabber Grabber { get; }

        static ConsoleBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ConsoleBox), new FrameworkPropertyMetadata(typeof(ConsoleBox)));
        }

        public ConsoleBox()
        {
            Grabber = new ConsoleGrabber(this);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.GetTemplateChild("PART_ContentHost") is ScrollViewer PART_ContentHost)
                ScrollViewerHelper.SetAutoScrollToEnd(PART_ContentHost, true);
        }

        ~ConsoleBox()
        {
            Grabber?.Dispose();
        }

        protected class ConsoleGrabber : StringWriter
        {
            private TextBox TextBox { get; }
            public ConsoleGrabber(TextBox TextBox) : base()
            {
                Console.SetOut(this);
                this.TextBox = TextBox;
            }

            public override void Write(string value)
                => TextBox.Text += value;

            public override void WriteLine()
                => TextBox.Text += NewLine;

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                Console.SetOut(new StreamWriter(Console.OpenStandardOutput())
                {
                    AutoFlush = true
                });
            }

        }
    }
}
