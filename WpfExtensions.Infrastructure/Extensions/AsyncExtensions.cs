﻿using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WpfExtensions.Infrastructure.Extensions;

public static class TimeAwaitExtensions
{
    public static TaskAwaiter GetAwaiter(this TimeSpan timeSpan)
    {
        return Task.Delay(timeSpan).GetAwaiter();
    }
}

public static class ProcessAwaitExtensions
{
    public static TaskAwaiter<int> GetAwaiter(this Process process)
    {
        var tcs = new TaskCompletionSource<int>();
        process.EnableRaisingEvents = true;
        process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode);
        if (process.HasExited) tcs.TrySetResult(process.ExitCode);
        return tcs.Task.GetAwaiter();
    }
}