using System.Windows;

namespace MenthaAssembly.Views.Primitives
{
    public sealed class BitChangedEventArgs(int Index, bool Status, RoutedEvent RoutedEvent, object Source) : RoutedEventArgs(RoutedEvent, Source)
    {
        public int Index { get; } = Index;

        public bool Status { get; } = Status;

    }
}