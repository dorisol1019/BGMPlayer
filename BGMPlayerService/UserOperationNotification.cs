using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;

namespace BGMPlayerService
{
    public class UserOperationNotification<T> : IUserOperationNotification<T>
    {
        public ReactivePropertySlim<T> Notification { get; } = new ReactivePropertySlim<T>(mode: ReactivePropertyMode.None);
    }
}
