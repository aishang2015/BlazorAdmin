using BlazorAdmin.Data.Entities.Setting;
using Microsoft.EntityFrameworkCore;

namespace BlazorAdmin.Data.Extensions
{
    public static class SettingSetExtensions
    {
        public static string? GetSettingValue(this DbSet<Setting> settings, string key)
        {
            return settings.FirstOrDefault(s => s.Key == key)?.Value;
        }

        public static void SetSettingValue(this DbSet<Setting> settings, string key, string? value)
        {
            var setting = settings.FirstOrDefault(s => s.Key == key);
            if (setting == null)
            {
                settings.Add(new Setting { Key = key, Value = value ?? string.Empty });
            }
            else
            {
                setting.Value = value ?? string.Empty;
                settings.Update(setting);
            }
        }
    }
}
