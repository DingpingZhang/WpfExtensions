using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Threading;
using System.Windows;

namespace XamlExtensions
{
    public class CurrentUICultureChangedEventArgs : EventArgs
    {
        public CultureInfo OldUICulture { get; }

        public CultureInfo NewUICulture { get; }

        public CurrentUICultureChangedEventArgs(CultureInfo oldUiCulture, CultureInfo newUiCulture)
        {
            OldUICulture = oldUiCulture;
            NewUICulture = newUiCulture;
        }
    }

    public class I18nManager : INotifyPropertyChanged
    {
        public static I18nManager Instance { get; } = new I18nManager();

        private readonly ConcurrentDictionary<string, ResourceManager> _resourceManagerStorage = new ConcurrentDictionary<string, ResourceManager>();
        private CultureInfo _currentUICulture;

        public event EventHandler<CurrentUICultureChangedEventArgs> CurrentUICultureChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        private I18nManager() { }

        public CultureInfo CurrentUICulture
        {
            get => _currentUICulture;
            set
            {
                if (EqualityComparer<CultureInfo>.Default.Equals(value, Thread.CurrentThread.CurrentUICulture)) return;

                var backup = _currentUICulture;
                _currentUICulture = value;
                OnCurrentUICultureChanged(backup, _currentUICulture);
            }
        }

        public void Add(ResourceManager resourceManager)
        {
            if (_resourceManagerStorage.ContainsKey(resourceManager.BaseName))
                throw new ArgumentException("", nameof(resourceManager));

            _resourceManagerStorage[resourceManager.BaseName] = resourceManager;
        }

        private void OnCurrentUICultureChanged(CultureInfo oldCulture, CultureInfo newCulture)
        {
            CurrentUICultureChanged?.Invoke(this, new CurrentUICultureChangedEventArgs(oldCulture, newCulture));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentUICulture)));
        }

        public object Get(ComponentResourceKey key) =>
            GetCurrentResourceManager(key.TypeInTargetAssembly.FullName)?
                .GetObject(key.ResourceId.ToString(), CurrentUICulture) ?? $"<MISSING: {key}>";

        private ResourceManager GetCurrentResourceManager(string key) =>
            _resourceManagerStorage.TryGetValue(key, out var value) ? value : null;
    }
}
