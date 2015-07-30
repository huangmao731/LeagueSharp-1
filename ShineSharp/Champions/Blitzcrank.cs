using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using ShineCommon;

namespace ShineSharp.Champions
{
    public class Blitzcrank : BaseChamp
    {
        public Blitzcrank()
            : base("Shine# Blitzcrank!")
        {
        }

        public override void CreateConfigMenu()
        {
            combo = new Menu("Combo", "Combo");
            combo.AddItem(new MenuItem("CUSEQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("CUSEW", "Use W").SetValue(true));
            combo.AddItem(new MenuItem("CUSEE", "Use E").SetValue(false));
            combo.AddItem(new MenuItem("CUSERHIT", "Use R If Enemies >=").SetValue(new Slider(1, 1, 5)));

            harass = new Menu("Harass", "Harass");
            harass.AddItem(new MenuItem("HUSEQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("HMANA", "Min. Mana Percent").SetValue(new Slider(50, 100, 0)));

            laneclear = new Menu("LaneClear", "LaneClear");
            //laneclear.AddItem(new MenuItem("LUSEQ", "Use Q").SetValue(true));
            //laneclear.AddItem(new MenuItem("LMANA", "Min. Mana Percent").SetValue(new Slider(50, 100, 0)));

            misc = new Menu("Misc", "Misc");
            misc.AddItem(new MenuItem("MAUTOQ", "Auto Grab (Q)").SetValue(true));
            m_evader = new Evader(out evade, EvadeMethods.EzrealE);
    
            Config.AddSubMenu(combo);
            Config.AddSubMenu(harass);
            Config.AddSubMenu(laneclear);
            Config.AddSubMenu(misc);
            Config.AddToMainMenu();

            OrbwalkingFunctions[(int) Orbwalking.OrbwalkingMode.Combo] += Combo;
            OrbwalkingFunctions[(int) Orbwalking.OrbwalkingMode.Mixed] += Harass;
            OrbwalkingFunctions[(int) Orbwalking.OrbwalkingMode.LaneClear] += LaneClear;
            OrbwalkingFunctions[(int) Orbwalking.OrbwalkingMode.LastHit] += LastHit;
            BeforeOrbWalking += BeforeOrbwalk;
            Obj_AI_Base.OnBuffAdd += Obj_AI_Base_OnBuffAdd;
        }

        public override void SetSpells()
        {
            Spells[Q] = new Spell(SpellSlot.Q, 1000f);
            Spells[Q].SetSkillshot(0.25f, 70f, 1800f, true, SkillshotType.SkillshotLine);
            Spells[W] = new Spell(SpellSlot.W, 200);
            Spells[E] = new Spell(SpellSlot.E, 475);
            Spells[R] = new Spell(SpellSlot.R, 550f);
        }


        public void BeforeOrbwalk()
        {
            #region Auto Harass

            if (Spells[Q].IsReady() && Config.Item("MAUTOQ").GetValue<bool>() && !ObjectManager.Player.UnderTurret())
            {
                var t = (from enemy in HeroManager.Enemies
                    where enemy.IsValidTarget(Spells[Q].Range)
                    orderby TargetSelector.GetPriority(enemy) descending
                    select enemy).FirstOrDefault();
                if (t != null)
                    CastSkillshot(t, Spells[Q]);
            }

            #endregion
        }

        public void Combo()
        {
            if (Spells[Q].IsReady() && Config.Item("CUSEQ").GetValue<bool>())
            {
                var t = TargetSelector.GetTarget(Spells[Q].Range - 30, TargetSelector.DamageType.Magical);
                if (t != null)
                    CastSkillshot(t, Spells[Q]);
            }
            if (Spells[W].IsReady() && Config.Item("CUSEW").GetValue<bool>())
            {
                Spells[W].Cast();
            }
            if (Spells[E].IsReady() && Config.Item("CUSEE").GetValue<bool>())
            {
                Spells[E].Cast();
            }

            if (Spells[R].IsReady())
            {
                var t =
                    HeroManager.Enemies.Where(p => p.IsValidTarget(Spells[R].Range))
                        .OrderBy(q => q.ServerPosition.Distance(ObjectManager.Player.ServerPosition))
                        .FirstOrDefault();
                if (t != null)
                {
                    Spells[R].CastIfWillHit(t, Config.Item("CUSERHIT").GetValue<Slider>().Value);
                }
            }
        }

        public void Harass()
        {
            if (ObjectManager.Player.ManaPercent < Config.Item("HMANA").GetValue<Slider>().Value)
                return;

            if (Spells[Q].IsReady() && Config.Item("HUSEQ").GetValue<bool>())
            {
                var target = TargetSelector.GetTarget(Spells[Q].Range, TargetSelector.DamageType.Magical);
                if (target != null)
                    CastSkillshot(target, Spells[Q]);
            }

            if (Spells[E].IsReady())
            {
                var target = TargetSelector.GetTarget(Spells[E].Range, TargetSelector.DamageType.Physical);
                Spells[E].Cast();
            }
        }

        public void LaneClear()
        {
        }

        public void LastHit()
        {
            //if (Spells[E].IsReady() && Config.Item("MLASTE").GetValue<bool>())
            {
                //var t =
                //    (from minion in
                //        MinionManager.GetMinions(Spells[E].Range, MinionTypes.All, MinionTeam.Enemy,
                //            MinionOrderTypes.MaxHealth)
                //        where
                //            minion.IsValidTarget(Spells[E].Range) && Spells[E].GetDamage(minion) >= minion.Health &&
                //            (!minion.UnderTurret() &&
                //             minion.Distance(ObjectManager.Player.Position) > ObjectManager.Player.AttackRange)
                //        orderby minion.Health ascending
                //        select minion).FirstOrDefault();
            }
        }

        private void Obj_AI_Base_OnBuffAdd(Obj_AI_Base sender, Obj_AI_BaseBuffAddEventArgs args)
        {
            if (args.Buff.Caster.IsAlly && (args.Buff.Type == BuffType.Snare || args.Buff.Type == BuffType.Stun))
            {
                if (Spells[Q].IsReady())
                    Spells[Q].Cast(sender.ServerPosition);
            }
        }
    }
}



