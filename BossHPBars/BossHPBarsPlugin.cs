global using System;
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
            hpBar = gameObject.AddComponent<HPBar>();
            hpBar.modEnabled = () => modSettings!.ShowBar.Value;

            On.SwingingGarbageMagnet.StartFight += ShowMagnetHP;
            On.SwingingGarbageMagnet.Die += HideMagnetHP;
            On.TiredMother.StartFight += ShowTireMomHP;
            On.TiredMother.Die += HideTireMomHP;
        }

        private void ShowMagnetHP(On.SwingingGarbageMagnet.orig_StartFight orig, SwingingGarbageMagnet self)
        {
            orig(self);
            hpBar!.bossHP = new(() => self.currentHealth, self.health);
        }

        private void HideMagnetHP(On.SwingingGarbageMagnet.orig_Die orig, SwingingGarbageMagnet self)
        {
            orig(self);
            hpBar!.bossHP = null;
        }

        private void ShowTireMomHP(On.TiredMother.orig_StartFight orig, TiredMother self)
        {
            orig(self);
            hpBar!.bossHP = new(() => self.currentHealth, self.health);
        }

        private void HideTireMomHP(On.TiredMother.orig_Die orig, TiredMother self)
        {
            orig(self);
            hpBar!.bossHP = null;
        }

        private Settings? modSettings;
        private HPBar? hpBar;
    }
}
