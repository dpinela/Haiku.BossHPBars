using BepConfig = BepInEx.Configuration;
using MAPI = Modding;

namespace Haiku.BossHPBars
{
    internal class Settings
    {
        public BepConfig.ConfigEntry<bool> ShowBar;

        public Settings(BepConfig.ConfigFile config)
        {
            ShowBar = config.Bind("", "Show Boss HP Bar", false);
        }
    }
}