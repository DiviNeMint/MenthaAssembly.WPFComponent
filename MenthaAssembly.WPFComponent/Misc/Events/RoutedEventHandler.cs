namespace System.Windows
{
    /// <summary>
    /// Represents the method that will handle various routed events that do not have specific event data beyond the data that is common for all routed events.
    /// </summary>
    /// <typeparam name="T">The specify type of event data.</typeparam>
    /// <param name="sender">The object where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    public delegate void RoutedEventHandler<T>(object sender, T e)
        where T : RoutedEventArgs;

}