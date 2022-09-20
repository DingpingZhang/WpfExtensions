using System;
using System.Threading;

namespace WpfExtensions.Infrastructure;

public class DefaultLogger : ILogger
{
    internal static ILogger Get(Type type) => Configurations.LoggerFactory?.Invoke(type) ?? new DefaultLogger(type);

    private readonly Type _type;

    protected DefaultLogger(Type type) => _type = type;

    public void Error(string message, Exception e = null)
    {
        Write(nameof(Error), message, e);
    }

    public void Warning(string message, Exception e = null)
    {
        Write(nameof(Warning), message, e);
    }

    public void Info(string message, Exception e = null)
    {
        Write(nameof(Info), message, e);
    }

    public void Debug(string message, Exception e = null)
    {
        Write(nameof(Debug), message, e);
    }

    private void Write(string category, string message, Exception e)
    {
        message = $"{message}{Environment.NewLine}";

        if (e != null)
        {
            message = $"{message}{e.Message}{Environment.NewLine}" +
                      $"{e.StackTrace}{Environment.NewLine}";
        }

        message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} " +
                  $"[{Thread.CurrentThread.ManagedThreadId}] " +
                  $"{category.ToUpper()} " +
                  $"{_type.FullName}{Environment.NewLine}" +
                  $"{message}{Environment.NewLine}";

        System.Diagnostics.Debug.Write(message);
    }
}
