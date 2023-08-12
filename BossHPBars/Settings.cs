using BepConfig = BepInEx.Configuration;
using MAPI = Modding;

namespace Haiku.BossHPBars
{
    internal class Settings
    {
        public BepConfig.ConfigEntry<bool> ShowBar;
        public BepConfig.ConfigEntry<bool> ShowNumbers;

        public Settings(BepConfig.ConfigFile config)
        {
            ShowBar = config.Bind("", "Show Boss HP Bar", false);
            ShowNumbers = config.Bind("", "Show Boss HP Numbers", false);
        }
    }
}