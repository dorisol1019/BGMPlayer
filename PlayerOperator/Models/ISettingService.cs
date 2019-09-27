using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerOperator.Models
{
    public interface ISettingService
    {
        BooleanNotifier IsTopMostWindow { get; set; }
    }
}
