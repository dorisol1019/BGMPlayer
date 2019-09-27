using System;
using System.Collections.Generic;
using System.Text;

namespace PlayerOperator.Models
{
    public class SettingRepository : ISettingRepository
    {
        private Setting setting;

        public SettingRepository()
        {
            setting = new Setting();
        }

        public void SetSetting(Setting setting)
        {
            this.setting = setting;
        }

        public Setting GetSetting()
        {
            return setting;
        }
    }
}
