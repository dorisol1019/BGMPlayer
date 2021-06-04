using Reactive.Bindings.Notifiers;

namespace PlayerOperator.Models
{
    public interface ISettingService
    {
        BooleanNotifier IsTopMostWindow { get; set; }
    }
}
