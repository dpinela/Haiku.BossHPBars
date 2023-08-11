global using System;
using Bep = BepInEx;
using SC = System.Collections;
using static System.Linq.Enumerable;

namespace Haiku.BossHPBars
{
    [Bep.BepInPlugin("haiku.bosshpbars", "Haiku Boss HP Bars", "1.0.0.0")]
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
            On.DoorBoss.StartFight += ShowDoorHP;
            On.DoorBoss.DeathSequence += HideDoorHP;
            On.ScubaHead.StartFight += ShowScubaHP;
            On.ScubaBossManager.BossCount += HideScubaHP;
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

        private void ShowDoorHP(On.DoorBoss.orig_StartFight orig, DoorBoss self)
        {
            orig(self);
            hpBar!.bossHP = new(() => self.currentHealth, self.currentHealth);
        }

        private SC.IEnumerator HideDoorHP(On.DoorBoss.orig_DeathSequence orig, DoorBoss self)
        {
            hpBar!.bossHP = null;
            return orig(self);
        }

        private void ShowScubaHP(On.ScubaHead.orig_StartFight orig, ScubaHead self)
        {
            orig(self);
            // Each ScubaHead is its own object, but only the first of
            // them has a nextScubaHead.
            if (self.nextScubaHead != null)
            {
                var secondHead = self.nextScubaHead.GetComponent<ScubaHead>();
                var thirdHead = self.manager.thirdScubaHead.GetComponent<ScubaHead>();

                int ScubaHP()
                {
                    return self.currentHealth + secondHead.currentHealth + thirdHead.currentHealth;
                }
                hpBar!.bossHP = new(ScubaHP, ScubaHP());
            }
        }

        private void HideScubaHP(On.ScubaBossManager.orig_BossCount orig, ScubaBossManager self, float x)
        {
            orig(self, x);
            if (self.bossCount == 0)
            {
                hpBar!.bossHP = null;
            }
        }

        private Settings? modSettings;
        private HPBar? hpBar;
    }
}
