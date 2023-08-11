global using System;
using Bep = BepInEx;
using static System.Linq.Enumerable;

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
            On.DrillBoss.StartFight += ShowDrillHP;
            On.DrillBoss.BossDefeated += HideDrillHP;
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

        private void ShowDrillHP(On.DrillBoss.orig_StartFight orig, DrillBoss self)
        {
            orig(self);
            hpBar!.bossHP = new(() => DrillHealth(self), DrillHealth(self));
        }

        private void HideDrillHP(On.DrillBoss.orig_BossDefeated orig, DrillBoss self)
        {
            orig(self);
            hpBar!.bossHP = null;
        }

        private static int DrillHealth(DrillBoss drill) =>
            drill.bodyPartScripts
                .Select(p => p.health)
                // Individual parts can go below 0 health, which would
                // make a naive sum yield wrong results.
                .Where(h => h > 0)
                .Sum();

        private Settings? modSettings;
        private HPBar? hpBar;
    }
}
