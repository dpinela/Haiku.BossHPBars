global using System;
using CompilerServices = System.Runtime.CompilerServices;
using Bep = BepInEx;

namespace Haiku.BossHPBars
{
    [Bep.BepInPlugin("haiku.bosshpbars", "Haiku Boss HP Bars", "1.1.0.0")]
    [Bep.BepInDependency("haiku.mapi", "1.0")]
    public class BossHPBarsPlugin : Bep.BaseUnityPlugin
    {
        public void Start()
        {
            modSettings = new(Config);
            gameObject.AddComponent<HPBar>();
        }

        private Settings? modSettings;
    }
}
