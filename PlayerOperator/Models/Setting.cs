using Reactive.Bindings.Notifiers;

namespace PlayerOperator.Models;

public class Setting
{
    public BooleanNotifier IsTopMostWindow { get; set; } = new BooleanNotifier();
}
