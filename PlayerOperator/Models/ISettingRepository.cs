using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerOperator.Models
{
    public interface ISettingRepository
    {
        void SetSetting(Setting setting);
        Setting GetSetting();
    }
}
