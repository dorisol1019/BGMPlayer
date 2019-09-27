using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PlayerOperator.Models
{
    public class Setting
    {
        public BooleanNotifier IsTopMostWindow { get; set; } = new BooleanNotifier();
    }
}
