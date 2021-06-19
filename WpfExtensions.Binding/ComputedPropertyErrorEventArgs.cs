using System;

namespace WpfExtensions.Binding
{
    public class ComputedPropertyErrorEventArgs
    {
        public string PropertyName { get; }

        public Exception Error { get; }

        public ComputedPropertyErrorEventArgs(string propertyName, Exception error)
        {
            PropertyName = propertyName;
            Error = error;
        }
    }
}
