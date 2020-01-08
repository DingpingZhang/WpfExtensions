using System;
using System.Diagnostics;
using System.Threading;
using Prism.Logging;

namespace WpfExtensions.Infrastructure
{
    public class DefaultLogger : ILoggerFacade
    {
        public static Func<Type, ILoggerFacade> Factory { get; set; }

        public static ILoggerFacade Get(Type type) => Factory?.Invoke(type) ?? new DefaultLogger(type);

        private readonly Type _type;

        protected DefaultLogger(Type type) => _type = type;

        void ILoggerFacade.Log(string message, Category category, Priority priority)
        {
            message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} " +
                      $"[{Thread.CurrentThread.ManagedThreadId}] " +
                      $"{category.ToString().ToUpper()} " +
                      $"{_type.FullName}{Environment.NewLine}" +
                      $"{message}{Environment.NewLine}";

            Write(message);
        }

        protected virtual void Write(string message) => Debug.Write(message);
    }
}
