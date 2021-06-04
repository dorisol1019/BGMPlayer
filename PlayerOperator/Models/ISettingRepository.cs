namespace PlayerOperator.Models
{
    public interface ISettingRepository
    {
        void SaveSetting(Setting setting);
        Setting LoadSetting();
    }
}
