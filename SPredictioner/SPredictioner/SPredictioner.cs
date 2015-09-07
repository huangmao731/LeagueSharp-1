using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;

namespace SPredictioner
{
    public class SPredictioner
    {
        public static Spell[] Spells = { null, null, null, null };
        public static Menu Config;

        public static void Initialize()
        {
            #region Initialize Menu
            Config = new Menu("SPredictioner", "spredictioner", true);
            TargetSelector.AddToMenu(Config.SubMenu("Target Selector"));
            Config.AddItem(new MenuItem("COMBOKEY", "Combo").SetValue(new KeyBind(32, KeyBindType.Press)));
            Config.AddItem(new MenuItem("HARASSKEY", "Harass").SetValue(new KeyBind('C', KeyBindType.Press)));
            Config.AddItem(new MenuItem("ENABLED", "Enabled").SetValue(true));
            SPrediction.Prediction.Initialize(Config);
            Config.SubMenu("SPRED").AddItem(new MenuItem("SPREDHITC", "Hit Chance").SetValue(new StringList(ShineCommon.Utility.HitchanceNameArray, 2)));
            Config.AddToMainMenu();
            #endregion

            #region Initialize Events
            Spellbook.OnCastSpell += EventHandlers.Spellbook_OnCastSpell;
            Obj_AI_Hero.OnProcessSpellCast += EventHandlers.Obj_AI_Hero_OnProcessSpellCast;
            #endregion

            #region Initialize Spells
            foreach (var spell in SpellDatabase.Spells)
            {
                if (spell.ChampionName == ObjectManager.Player.CharData.BaseSkinName)
                {
                    Spells[(int)spell.Slot] = new Spell(spell.Slot, spell.Range);
                    Spells[(int)spell.Slot].SetSkillshot(spell.Delay / 1000, spell.Radius, spell.MissileSpeed, spell.Collisionable, spell.Type);
                }
            }
            #endregion
        }
    }
}
