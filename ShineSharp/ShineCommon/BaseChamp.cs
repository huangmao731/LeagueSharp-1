using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using ShineCommon;
using Geometry = ShineCommon.Maths.Geometry;
using SharpDX;


namespace ShineCommon
{
    public class BaseChamp
    {
        public const int Q = 0, W = 1, E = 2, R = 3;

        public Menu Config, combo, harass, laneclear, misc, drawing, evade;
        public Orbwalking.Orbwalker Orbwalker;
        public Spell[] Spells = new Spell[4];
        public Evader m_evader;

        public delegate void dVoidDelegate();
        public dVoidDelegate BeforeOrbWalking;
        public dVoidDelegate[] OrbwalkingFunctions = new dVoidDelegate[4];

        public BaseChamp(string szChampName)
        {
            Config = new Menu(szChampName, szChampName, true);
            TargetSelector.AddToMenu(Config.SubMenu("Target Selector"));
            Orbwalker = new Orbwalking.Orbwalker(Config.SubMenu("Orbwalking"));
            SpellDatabase.InitalizeSpellDatabase();
        }

        public virtual void CreateConfigMenu()
        {
            //
        }

        public virtual void SetSpells()
        {
            //
        }

        public virtual void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.IsDead || ObjectManager.Player.IsRecalling() || args == null)
                return;

            if (BeforeOrbWalking != null) BeforeOrbWalking();

            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.None)
                OrbwalkingFunctions[(int)Orbwalker.ActiveMode]();
        }

        public virtual void Drawing_OnDraw(EventArgs args)
        {
            //
        }

        public virtual void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            //
        }

        public virtual void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            //
        }

        public void CastSkillshot(Obj_AI_Hero t, Spell s)
        {
            if (!s.IsSkillshot)
                return;

            PredictionOutput p = s.GetPrediction(t);
            if (s.Collision)
            {
                for (int i = 0; i < p.CollisionObjects.Count; i++)
                    if (!p.CollisionObjects[i].IsDead && (p.CollisionObjects[i].IsEnemy || p.CollisionObjects[i].IsMinion))
                        return;

            }

            if ((t.HasBuffOfType(BuffType.Slow) && p.Hitchance >= HitChance.Medium) || p.Hitchance == HitChance.Immobile)
                s.Cast(p.CastPosition);
            else
            {
                //TO DO: if iswindingup
                if (s.Type != SkillshotType.SkillshotCone)
                {
                    if (t.IsMoving && !t.IsWindingUp)
                    {
                        float dist_waypoint = ObjectManager.Player.ServerPosition.Distance(t.GetWaypoints().Last().To3D());
                        float dist_target = ObjectManager.Player.ServerPosition.Distance(t.ServerPosition);
                        float mulspeeddelay = t.MoveSpeed * s.Delay;
                        if (dist_waypoint - dist_target > mulspeeddelay) //running away from me
                        {
                            dist_target -= mulspeeddelay;
                            dist_target -= (dist_target - mulspeeddelay) / s.Speed * t.MoveSpeed;
                            if (s.Type == SkillshotType.SkillshotCircle)
                                dist_target += s.Width;
                        }


                        if (t.IsValidTarget(dist_target))
                        {
                            //s.Cast(s.GetPrediction(t).CastPosition);
                            if (t.Path.Length >= 2)
                            {
                                var direction = (t.Path[t.Path.Length - 1] - t.Path[0]).Normalized();
                                s.Cast(p.CastPosition + direction * t.MoveSpeed * (s.Delay + dist_target / s.Speed - Game.Ping / 2));
                                Console.WriteLine("new method");
                            }
                            else s.Cast(s.GetPrediction(t).CastPosition);
                            Console.WriteLine("away cast");
                            return;
                        }

                        if (dist_waypoint < dist_target) //coming closer to me
                        {
                            s.CastIfHitchanceEquals(t, HitChance.High);
                            Console.WriteLine("closer cast");
                            return;
                        }
                    }
                    else if (t.IsWindingUp)
                    {
                        if (t.Path.Length >= 2)
                            s.Cast(t.Path[t.Path.Length / 2]);
                        else if (t.Path.Length == 0)
                            s.Cast(t.ServerPosition);
                        else if (t.Path.Length == 1)
                            s.Cast(t.Path[0]);
                        Console.WriteLine("windingup cast");
                        return;
                    }
                }
                else
                {
                    if (t.IsValidTarget(s.Range))
                    {
                        s.Cast(p.CastPosition);
                        Console.WriteLine("else cast");
                    }
                }
            }
        }

        public bool ComboReady()
        {
            return Spells[Q].IsReady() && Spells[W].IsReady() && Spells[E].IsReady() && Spells[R].IsReady();
        }

        #region Damage Calculation Funcitons
        public double CalculateComboDamage(Obj_AI_Hero target)
        {
            return CalculateSpellDamage(target) + CalculateSummonersDamage(target) + CalculateItemsDamage(target);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateDamageQ(Obj_AI_Hero target)
        {
            var item = Config.Item("CUSEQ");
            if (item != null && item.GetValue<bool>() && Spells[Q].IsReady())
                return ObjectManager.Player.GetSpellDamage(target, SpellSlot.Q);

            return 0.0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateDamageW(Obj_AI_Hero target)
        {
            var item = Config.Item("CUSEW");
            if (item != null && item.GetValue<bool>() && Spells[W].IsReady())
                return ObjectManager.Player.GetSpellDamage(target, SpellSlot.W);

            return 0.0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateDamageE(Obj_AI_Hero target)
        {
            var item = Config.Item("CUSEE");
            if (item != null && item.GetValue<bool>() && Spells[E].IsReady())
                return ObjectManager.Player.GetSpellDamage(target, SpellSlot.E);

            return 0.0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual double CalculateDamageR(Obj_AI_Hero target)
        {
            var item = Config.Item("CUSER");
            if (item != null && item.GetValue<bool>() && Spells[R].IsReady())
                return ObjectManager.Player.GetSpellDamage(target, SpellSlot.R);

            return 0.0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double CalculateSpellDamage(Obj_AI_Hero target)
        {
            return CalculateDamageQ(target) + CalculateDamageW(target) + CalculateDamageE(target) + CalculateDamageR(target);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double CalculateSummonersDamage(Obj_AI_Hero target)
        {
            var ignite = ObjectManager.Player.GetSpellSlot("summonerdot");
            if (ignite != SpellSlot.Unknown && ObjectManager.Player.Spellbook.CanUseSpell(ignite) == SpellState.Ready && ObjectManager.Player.Distance(target, false) < 550)
                return ObjectManager.Player.GetSummonerSpellDamage(target, Damage.SummonerSpell.Ignite); //ignite

            return 0.0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double CalculateItemsDamage(Obj_AI_Hero target)
        {
            double dmg = 0.0;

            if (Items.CanUseItem(3144) && ObjectManager.Player.Distance(target, false) < 550)
                dmg += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Bilgewater); //bilgewater cutlass

            if (Items.CanUseItem(3153) && ObjectManager.Player.Distance(target, false) < 550)
                dmg += ObjectManager.Player.GetItemDamage(target, Damage.DamageItems.Botrk); //botrk

            return dmg;
        }
        #endregion
    }
}
