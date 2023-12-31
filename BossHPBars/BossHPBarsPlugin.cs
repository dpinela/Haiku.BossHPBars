global using System;
using Bep = BepInEx;
using MMDetour = MonoMod.RuntimeDetour;
using USM = UnityEngine.SceneManagement;
using Reflection = System.Reflection;
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
            hpBar.modSettings = () => modSettings!;

            var rflags = 
                Reflection.BindingFlags.Public | 
                Reflection.BindingFlags.NonPublic | 
                Reflection.BindingFlags.Instance;

            On.SwingingGarbageMagnet.StartFight += ShowMagnetHP;
            On.SwingingGarbageMagnet.Die += HideMagnetHP;
            On.TiredMother.StartFight += ShowTireMomHP;
            On.TiredMother.Die += HideTireMomHP;
            On.DrillBoss.StartFight += ShowDrillHP;
            On.DrillBoss.BossDefeated += HideDrillHP;
            On.DoorBoss.StartFight += ShowDoorHP;
            On.DoorBoss.DeathSequence += HideDoorHP;
            On.FightManager.StartFight += ShowTrioHP;
            On.FightManager.ReduceHealth += HideTrioHP;
            On.ScubaHead.StartFight += ShowScubaHP;
            On.ScubaBossManager.BossCount += HideScubaHP;
            On.MischievousMechanic.StartFight += ShowMischievousHP;
            On.MischievousMechanic.Die += HideMischievousHP;
            On.BuzzSaw.StartFight += ShowBuzzsawHP;
            On.BuzzSaw.DeathSequence += HideBuzzsawHP;
            On.BigBrotherCamera.StartFight += ShowBigBrotherHP;
            On.BigBrotherCamera.StopLoop += HideBigBrotherHP;
            On.FactorySentient.StartFight += ShowProtonHP;
            On.FactorySentient.DeathAnimation += HideProtonHP;
            On.BunkerSentient.StartFight += ShowNeutronHP;
            On.BunkerSentient.DeathAnimation += HideNeutronHP;
            new MMDetour.Hook(typeof(TvBoss).GetMethod("StartFight", rflags), ShowTVHP);
            new MMDetour.Hook(typeof(TvBoss).GetMethod("DeathSequence", rflags), HideTVHP);
            // StartFight is called too early on Elegy, before the health
            // pool adjustment is made.
            new MMDetour.Hook(typeof(ReactorCore).GetMethod("Start", rflags), ShowElegyHP);
            new MMDetour.Hook(typeof(ReactorCore).GetMethod("DeathSequence", rflags), HideElegyHP);
            new MMDetour.Hook(typeof(DoubleDoorBoss).GetMethod("StartFight", rflags), ShowDoubleDoorHP);
            new MMDetour.Hook(typeof(DoubleDoorBoss).GetMethod("DefeatedOneBoss", rflags), HideDoubleDoorHP);
            On.CarBattery.StartFight += ShowCarBatteryHP;
            On.CarBattery.DeathSequence += HideCarBatteryHP;
            On.BeeHive.Update += ShowHideHiveHP;
            On.ElectricSentientRework.StartFight += ShowElectronHP;
            On.ElectricSentientRework.Death += HideElectronHP;
            On.TheVirus.StartFight += ShowVirusHP;
            On.VirusDefeated.Start += HideVirusHP;
            new MMDetour.Hook(typeof(UncorruptVirusBoss).GetMethod("StartFight", rflags), ShowAtomHP);
            new MMDetour.Hook(typeof(UncorruptVirusDefeated).GetMethod("Start", rflags), HideAtomHP);

            USM.SceneManager.activeSceneChanged += HideBarOnExit;
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
            // In the boss rush mode, there are two DrillBoss objects in the
            // scene at once. Both call StartFight at the start of the fight.
            if (hpBar!.bossHP is HP firstDrillHP)
            {
                hpBar!.bossHP = new(
                    () => firstDrillHP.Current() + DrillHealth(self),
                    firstDrillHP.Max + DrillHealth(self)
                );
            }
            else
            {
                hpBar!.bossHP = new(() => DrillHealth(self), DrillHealth(self));
            }
        }

        private void HideDrillHP(On.DrillBoss.orig_BossDefeated orig, DrillBoss self)
        {
            orig(self);
            // The HP bar must stay up as long as at least one drill remains
            // alive.
            if (!self.bossRush || self.drillBossRush.currentBossesDead == self.drillBossRush.numberOfBosses)
            {
                hpBar!.bossHP = null;
            }
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

        private void ShowTrioHP(On.FightManager.orig_StartFight orig, FightManager self)
        {
            orig(self);
            hpBar!.bossHP = new(() => TrioHealth(self), TrioHealth(self));
        }

        private void HideTrioHP(On.FightManager.orig_ReduceHealth orig, FightManager self, string boss, int n)
        {
            orig(self, boss, n);
            if (!self.bAlive && !self.eAlive && !self.fAlive)
            {
                hpBar!.bossHP = null;
            }
        }

        private static int TrioHealth(FightManager fm) =>
            (fm.bAlive ? fm.bHealth : 0) +
            (fm.eAlive ? fm.eHealth : 0) +
            (fm.fAlive ? fm.fHealth : 0);

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

        private void ShowMischievousHP(On.MischievousMechanic.orig_StartFight orig, MischievousMechanic self)
        {
            // StartFight has this extra condition in it, presumably
            // because it may be called multiple times.
            if (!self.fightStarted)
            {
                // Mischievous stores their health as a float for some
                // reason, even though the value is always an integer
                // from Mathf.FloorToInt.
                hpBar!.bossHP = new(() => (int)self.currentHealth, (int)self.currentHealth);
            }
            orig(self);
        }

        private void HideMischievousHP(On.MischievousMechanic.orig_Die orig, MischievousMechanic self)
        {
            orig(self);
            hpBar!.bossHP = null;
        }

        private void ShowBuzzsawHP(On.BuzzSaw.orig_StartFight orig, BuzzSaw self)
        {
            orig(self);
            hpBar!.bossHP = new(() => self.currentHealth, self.currentHealth);
        }

        private SC.IEnumerator HideBuzzsawHP(On.BuzzSaw.orig_DeathSequence orig, BuzzSaw self)
        {
            hpBar!.bossHP = null;
            return orig(self);
        }

        private void ShowBigBrotherHP(On.BigBrotherCamera.orig_StartFight orig, BigBrotherCamera self)
        {
            orig(self);
            hpBar!.bossHP = new(() => self.currentHealth, self.currentHealth);
        }

        private void HideBigBrotherHP(On.BigBrotherCamera.orig_StopLoop orig, BigBrotherCamera self)
        {
            orig(self);
            hpBar!.bossHP = null;
        }

        private void ShowProtonHP(On.FactorySentient.orig_StartFight orig, FactorySentient self)
        {
            orig(self);
            hpBar!.bossHP = new(() => self.currentHealth, self.health);
        }

        private void HideProtonHP(On.FactorySentient.orig_DeathAnimation orig, FactorySentient self)
        {
            orig(self);
            hpBar!.bossHP = null;
        }

        private void ShowNeutronHP(On.BunkerSentient.orig_StartFight orig, BunkerSentient self)
        {
            orig(self);
            hpBar!.bossHP = new(() => self.currentHealth, self.currentHealth);
        }

        private void HideNeutronHP(On.BunkerSentient.orig_DeathAnimation orig, BunkerSentient self)
        {
            orig(self);
            hpBar!.bossHP = null;
        }

        private void ShowTVHP(Action<TvBoss> orig, TvBoss self)
        {
            orig(self);
            hpBar!.bossHP = new(() => self.currentHealth, self.currentHealth);
        }

        private SC.IEnumerator HideTVHP(Func<TvBoss, SC.IEnumerator> orig, TvBoss self)
        {
            hpBar!.bossHP = null;
            return orig(self);
        }

        private void ShowElegyHP(Action<ReactorCore> orig, ReactorCore self)
        {
            orig(self);
            hpBar!.bossHP = new(() => self.currentHealth, self.currentHealth);
        }

        private void HideElegyHP(Action<ReactorCore> orig, ReactorCore self)
        {
            orig(self);
            hpBar!.bossHP = null;
        }

        private void ShowDoubleDoorHP(Action<DoubleDoorBoss> orig, DoubleDoorBoss self)
        {
            orig(self);
            hpBar!.bossHP = new(() => DoubleDoorHealth(self), DoubleDoorHealth(self));
        }

        private void HideDoubleDoorHP(Action<DoubleDoorBoss> orig, DoubleDoorBoss self)
        {
            orig(self);
            // This method is called once for each of the individual doors.
            if (self.defeated == 2)
            {
                hpBar!.bossHP = null;
            }
        }

        private static int DoubleDoorHealth(DoubleDoorBoss doors) =>
            (doors.door1.dead ? 0 : doors.door1.currentHealth) +
            (doors.door2.dead ? 0 : doors.door2.currentHealth);

        private void ShowCarBatteryHP(On.CarBattery.orig_StartFight orig, CarBattery self)
        {
            orig(self);
            hpBar!.bossHP = new(() => self.currentHealth, self.currentHealth);
        }

        private SC.IEnumerator HideCarBatteryHP(On.CarBattery.orig_DeathSequence orig, CarBattery self)
        {
            hpBar!.bossHP = null;
            return orig(self);
        }

        private void ShowHideHiveHP(On.BeeHive.orig_Update orig, BeeHive self)
        {
            orig(self);
            if (hpBar!.bossHP == null)
            {
                if (self.playerInRange && self.state != BeeHive.State.Dead)
                {
                    hpBar!.bossHP = new(() => self.health, self.initialHealth);
                }
            }
            else if (self.state == BeeHive.State.Dead)
            {
                hpBar!.bossHP = null;
            }
        }

        private void ShowElectronHP(On.ElectricSentientRework.orig_StartFight orig, ElectricSentientRework self)
        {
            orig(self);
            hpBar!.bossHP = new(() => self.currentHealth, self.health);
        }

        private void HideElectronHP(On.ElectricSentientRework.orig_Death orig, ElectricSentientRework self)
        {
            orig(self);
            hpBar!.bossHP = null;
        }

        private void ShowVirusHP(On.TheVirus.orig_StartFight orig, TheVirus self)
        {
            orig(self);
            hpBar!.bossHP = new(() => self.currentHealth, self.currentHealth);
        }

        private void HideVirusHP(On.VirusDefeated.orig_Start orig, VirusDefeated self)
        {
            orig(self);
            hpBar!.bossHP = null;
        }

        private void ShowAtomHP(Action<UncorruptVirusBoss> orig, UncorruptVirusBoss self)
        {
            orig(self);
            hpBar!.bossHP = new(() => self.currentHealth, self.currentHealth);
        }

        private void HideAtomHP(Action<UncorruptVirusDefeated> orig, UncorruptVirusDefeated self)
        {
            orig(self);
            hpBar!.bossHP = null;
        }

        private void HideBarOnExit(USM.Scene from, USM.Scene to)
        {
            hpBar!.bossHP = null;
        }

        private Settings? modSettings;
        private HPBar? hpBar;
    }
}
