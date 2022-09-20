using System;
using System.Threading;

namespace WpfExtensions.Infrastructure;

public static class ProcessController
{
    private static readonly ILogger Logger = DefaultLogger.Get(typeof(ProcessController));

    private static volatile EventWaitHandle _keepAliveEvent;

    public static bool IsDuplicateInstance(string processName)
    {
        bool createdNew;
        try
        {
            _keepAliveEvent = new EventWaitHandle(false, EventResetMode.ManualReset, processName, out createdNew);
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
        catch (Exception e)
        {
            Logger.Warning("Can't open the communication event.", e);
            return false;
        }

        if (createdNew) return false;

        Logger.Warning("The killed event has been opened. So there is another process has been initialized.");
        _keepAliveEvent.Close();

        return true;
    }

    public static void Clear()
    {
        if (_keepAliveEvent != null && !_keepAliveEvent.SafeWaitHandle.IsClosed)
        {
            _keepAliveEvent?.Set();
            _keepAliveEvent?.Close();
        }
    }
}
