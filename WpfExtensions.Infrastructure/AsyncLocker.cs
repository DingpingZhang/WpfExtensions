using System;
using System.Threading.Tasks;

namespace WpfExtensions.Infrastructure;

public class AsyncLocker
{
    private event Action Unlocked;

    private bool _isLocked;

    public bool IsLocked
    {
        get => _isLocked;
        set
        {
            if (_isLocked == value) return;
            _isLocked = value;
            if (!value) Unlocked?.Invoke();
        }
    }

    public void Await(Action action, bool discardIfLocked = false)
    {
        if (!IsLocked)
        {
            try
            {
                IsLocked = true;
                action?.Invoke();
            }
            finally
            {
                IsLocked = false;
            }
        }
        else if (!discardIfLocked)
        {
            Unlocked += OnUnlocked;
        }

        void OnUnlocked()
        {
            Unlocked -= OnUnlocked;
            Await(action, discardIfLocked);
        }
    }

    public async void Await(Func<Task> task, bool discardIfLocked = false)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        if (!IsLocked)
        {
            try
            {
                IsLocked = true;
                await task();
            }
            catch (OperationCanceledException) { /* ignore */ }
            finally
            {
                IsLocked = false;
            }
        }
        else if (!discardIfLocked)
        {
            Unlocked += OnUnlocked;
        }

        void OnUnlocked()
        {
            Unlocked -= OnUnlocked;
            Await(task, discardIfLocked);
        }
    }

    public async Task AwaitAsync(Func<Task> task, bool discardIfLocked = false)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));

        if (!IsLocked)
        {
            try
            {
                IsLocked = true;
                await task();
            }
            catch (OperationCanceledException) { /* ignore */ }
            finally
            {
                IsLocked = false;
            }
        }
        else if (!discardIfLocked)
        {
            Unlocked += OnUnlocked;
        }

        async void OnUnlocked()
        {
            Unlocked -= OnUnlocked;
            await AwaitAsync(task, discardIfLocked);
        }
    }
}
