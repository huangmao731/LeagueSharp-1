using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using ShineCommon;
using SharpDX;

namespace ShineSharp.Champions
{
    public class Ezreal : BaseChamp
    {
        public Ezreal()
            : base("Ezreal")
        {
            
        }

        public override void CreateConfigMenu()
        {
            combo = new Menu("Combo", "Combo");
            combo.AddItem(new MenuItem("CUSEQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("CUSEW", "Use W").SetValue(true));
            //combo.AddItem(new MenuItem("CUSEE", "Use E").SetValue(false));
            combo.AddItem(new MenuItem("CUSERHIT", "Use R If Enemies >=").SetValue(new Slider(2, 2, 5)));

            harass = new Menu("Harass", "Harass");
            harass.AddItem(new MenuItem("HUSEQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("HUSEW", "Use W").SetValue(true));
            harass.AddItem(new MenuItem("HMANA", "Min. Mana Percent").SetValue(new Slider(50, 100, 0)));

            laneclear = new Menu("LaneClear", "LaneClear");
            laneclear.AddItem(new MenuItem("LUSEQ", "Use Q").SetValue(true));
            laneclear.AddItem(new MenuItem("LMANA", "Min. Mana Percent").SetValue(new Slider(50, 100, 0)));

            misc = new Menu("Misc", "Misc");
            misc.AddItem(new MenuItem("MLASTQ", "Last Hit Q").SetValue(true));
            misc.AddItem(new MenuItem("MAUTOQ", "Auto Harass Q").SetValue(true));
            misc.AddItem(new MenuItem("MUSER", "Use R If Killable").SetValue(true));

            m_evader = new Evader(out evade, EvadeMethods.EzrealE);

            Config.AddSubMenu(combo);
            Config.AddSubMenu(harass);
            Config.AddSubMenu(laneclear);
            Config.AddSubMenu(evade);
            Config.AddSubMenu(misc);
            Config.AddToMainMenu();

            base.OrbwalkingFunctions[(int)Orbwalking.OrbwalkingMode.Combo] += Combo;
            base.OrbwalkingFunctions[(int)Orbwalking.OrbwalkingMode.Mixed] += Harass;
            base.OrbwalkingFunctions[(int)Orbwalking.OrbwalkingMode.LaneClear] += LaneClear;
            base.OrbwalkingFunctions[(int)Orbwalking.OrbwalkingMode.LastHit] += LastHit;
            base.BeforeOrbWalking += BeforeOrbwalk;
        }

        public override void SetSpells()
        {
            Spells[Q] = new Spell(SpellSlot.Q, 1190);
            Spells[Q].SetSkillshot(0.25f, 60f, 2000f, true, SkillshotType.SkillshotLine);

            Spells[W] = new Spell(SpellSlot.W, 800);
            Spells[W].SetSkillshot(0.25f, 80f, 1600f, false, SkillshotType.SkillshotLine);

            Spells[E] = new Spell(SpellSlot.E, 475);

            Spells[R] = new Spell(SpellSlot.R, 2500);
            Spells[R].SetSkillshot(1f, 160f, 2000f, false, SkillshotType.SkillshotLine);

            m_evader.SetEvadeSpell(Spells[E]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override double CalculateDamageR(Obj_AI_Hero target)
        {
            double dmg = 0.0;
            var item = Config.Item("CUSER");
            if (item != null && item.GetValue<bool>() && Spells[R].IsReady())
            {
                dmg = ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);
                int collCount = Spells[R].GetCollision(ObjectManager.Player.ServerPosition.To2D(), new List<Vector2>() { target.ServerPosition.To2D() }).Count();
                int percent = 10 - collCount > 7 ? 7 : collCount;
                dmg = dmg / 10 * percent;
            }
            return dmg;
        }

        public override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            var awayPosition = gapcloser.End.Extend(ObjectManager.Player.ServerPosition, ObjectManager.Player.Distance(gapcloser.End) + Spells[E].Range);
            Spells[E].Cast(awayPosition);
        }

        public void BeforeOrbwalk()
        {
            #region Auto Harass
            if (Spells[Q].IsReady() && Config.Item("MAUTOQ").GetValue<bool>() && !ObjectManager.Player.UnderTurret())
            {
                var t = (from enemy in HeroManager.Enemies where enemy.IsValidTarget(Spells[Q].Range) orderby TargetSelector.GetPriority(enemy) descending select enemy).FirstOrDefault();
                if (t != null)
                    CastSkillshot(t, Spells[Q]);
            }
            #endregion

            #region Auto Ult
            if (Config.Item("MUSER").GetValue<bool>() && Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo && ObjectManager.Player.CountEnemiesInRange(600) == 0)
            {
                var t = (from enemy in HeroManager.Enemies where enemy.IsValidTarget(Spells[R].Range) && CalculateDamageR(enemy) > enemy.Health orderby enemy.ServerPosition.Distance(ObjectManager.Player.ServerPosition) descending select enemy).FirstOrDefault();
                if (t != null)
                    Spells[R].Cast(Spells[R].GetPrediction(t).CastPosition);
            }
            #endregion
        }
        
        public void Combo()
        {
            /*if (Config.Item("CUSER").GetValue<bool>() && ObjectManager.Player.CountEnemiesInRange(600) == 0) //combo kill
            {
                var t = (from enemy in HeroManager.Enemies where enemy.IsValidTarget(1500) && CalculateComboDamage(enemy) >= enemy.Health orderby TargetSelector.GetPriority(enemy) descending select enemy).FirstOrDefault();
                if (t != null && !t.UnderTurret())
                {
                    if (Spells[R].CastIfHitchanceEquals(t, HitChance.VeryHigh))
                    {
                        if (Config.Item("CUSEE").GetValue<bool>()) Spells[E].Cast(t);
                        if (Config.Item("CUSEQ").GetValue<bool>())  Spells[Q].Cast(t);
                        if (Config.Item("CUSEW").GetValue<bool>())  Spells[W].Cast(t);
                        return;
                    }
                }
            }

            if (Spells[E].IsReady() && Config.Item("CUSEE").GetValue<bool>()) //find best position
            {
                
            }*/

            if (Spells[Q].IsReady() && Config.Item("CUSEQ").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Spells[Q].Range, TargetSelector.DamageType.Physical);
                if (t != null)
                    CastSkillshot(t, Spells[Q]);
            }

            if (Spells[W].IsReady() && Config.Item("CUSEW").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Spells[W].Range, TargetSelector.DamageType.Magical);
                if (t != null)
                    CastSkillshot(t, Spells[W]);
            }

            if (Spells[R].IsReady())
            {
                var t = HeroManager.Enemies.Where(p => p.IsValidTarget(Spells[R].Range)).OrderBy(q => q.ServerPosition.Distance(ObjectManager.Player.ServerPosition)).FirstOrDefault();
                if(t != null)
                    Spells[R].CastIfWillHit(t, Config.Item("CUSERHIT").GetValue<Slider>().Value);
            }
        }

        public void Harass()
        {
            if (ObjectManager.Player.ManaPercent < Config.Item("HMANA").GetValue<Slider>().Value)
                return;

            if (Spells[Q].IsReady() && Config.Item("HUSEQ").GetValue<bool>())
            {
                var target = TargetSelector.GetTarget(Spells[Q].Range, TargetSelector.DamageType.Physical);
                if (target != null)
                    CastSkillshot(target, Spells[Q]);
            }

            if (Spells[W].IsReady() && Config.Item("HUSEW").GetValue<bool>())
            {
                var target = TargetSelector.GetTarget(Spells[Q].Range, TargetSelector.DamageType.Magical);
                if (target != null)
                    CastSkillshot(target, Spells[W]);
            }

        }

        public void LaneClear()
        {
            if (Spells[Q].IsReady() && ObjectManager.Player.ManaPercent < Config.Item("LMANA").GetValue<Slider>().Value || !Config.Item("LUSEQ").GetValue<bool>())
                return;

            var t = (from minion in MinionManager.GetMinions(Spells[Q].Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth) where minion.IsValidTarget(Spells[Q].Range) && Spells[Q].GetDamage(minion) >= minion.Health orderby minion.Distance(ObjectManager.Player.Position) descending select minion).FirstOrDefault();
            if (t != null)
                Spells[Q].CastIfHitchanceEquals(t, HitChance.High);
        }

        public void LastHit()
        {
            if (Spells[Q].IsReady() && Config.Item("MLASTQ").GetValue<bool>())
            {
                var t = (from minion in MinionManager.GetMinions(Spells[Q].Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth) where minion.IsValidTarget(Spells[Q].Range) && Spells[Q].GetDamage(minion) >= minion.Health && (!minion.UnderTurret() && minion.Distance(ObjectManager.Player.Position) > ObjectManager.Player.AttackRange) orderby minion.Health ascending select minion).FirstOrDefault();
                if (t != null)
                    Spells[Q].CastIfHitchanceEquals(t, HitChance.High);
            }
        }
    }
}
