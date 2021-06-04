using Reactive.Bindings.Notifiers;

namespace PlayerOperator.Models
{
    public class SettingService : ISettingService
    {
        public SettingService(ISettingRepository settingRepository)
        {
            Setting? setting = settingRepository.LoadSetting();
            IsTopMostWindow = setting.IsTopMostWindow;
        }

        public BooleanNotifier IsTopMostWindow { get; set; }
    }
}
