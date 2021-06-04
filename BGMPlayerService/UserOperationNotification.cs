using Reactive.Bindings;

namespace BGMPlayerService
{
    public class UserOperationNotification<T> : IUserOperationNotification<T>
    {
        public ReactivePropertySlim<T> Notification { get; } = new ReactivePropertySlim<T>(mode: ReactivePropertyMode.None);
    }
}
