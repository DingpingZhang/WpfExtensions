using System;

namespace WpfExtensions.Infrastructure;

public static class Configurations
{
    public static Func<Type, ILogger> LoggerFactory { get; set; }
}
