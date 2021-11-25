namespace PlayerOperator.Models;

public class SettingRepository : ISettingRepository
{
    private Setting setting;

    public SettingRepository()
    {
        setting = new Setting();
    }

    public void SaveSetting(Setting setting)
    {
        this.setting = setting;
    }

    public Setting LoadSetting()
    {
        return setting;
    }
}
