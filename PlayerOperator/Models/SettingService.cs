using Reactive.Bindings;
using Reactive.Bindings.Notifiers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerOperator.Models
{
    public class SettingService : ISettingService
    {
        public SettingService(ISettingRepository settingRepository)
        {
            var setting = settingRepository.LoadSetting();
            IsTopMostWindow = setting.IsTopMostWindow;
        }

        public BooleanNotifier IsTopMostWindow { get; set; }
    }
}
