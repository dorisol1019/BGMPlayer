using Reactive.Bindings;

namespace BGMPlayerService
{
    public interface IUserOperationNotification<T>
    {
        ReactivePropertySlim<T> Notification { get; }
    }
}
