using System;

namespace WpfExtensions.Binding
{
    public class PropertyObserverErrorEventArgs
    {
        public string OwnerPropertyName { get; }

        public string Expression { get; }

        public Exception Exception { get; }

        public PropertyObserverErrorEventArgs(string ownerPropertyName, string expression, Exception exception)
        {
            OwnerPropertyName = ownerPropertyName;
            Expression = expression;
            Exception = exception;
        }
    }
}
