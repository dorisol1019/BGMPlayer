using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerOperator.Models
{
    public interface ISettingRepository
    {
        void SaveSetting(Setting setting);
        Setting LoadSetting();
    }
}
