using System;

namespace WpfExtensions.Infrastructure;

public interface IContainerProvider
{
    object Resolve(Type type);
}
