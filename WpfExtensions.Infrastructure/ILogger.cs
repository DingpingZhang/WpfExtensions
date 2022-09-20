using System;

namespace WpfExtensions.Infrastructure;

public interface ILogger
{
    void Error(string message, Exception e = null);

    void Warning(string message, Exception e = null);

    void Info(string message, Exception e = null);

    void Debug(string message, Exception e = null);
}
