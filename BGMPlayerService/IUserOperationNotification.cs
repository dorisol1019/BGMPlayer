using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BGMPlayerService
{
    public interface IUserOperationNotification<T>
    {
        ReactivePropertySlim<T> Notification { get; }
    }
}
