using System.Windows;

namespace Synpower4Net.Views.Primitives
{
    public sealed class BitChangedEventArgs(int Index, bool Status, RoutedEvent RoutedEvent, object Source) : RoutedEventArgs(RoutedEvent, Source)
    {
        public int Index { get; } = Index;

        public bool Status { get; } = Status;

    }
}