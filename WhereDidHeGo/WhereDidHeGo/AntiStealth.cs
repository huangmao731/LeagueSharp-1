using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace WhereDidHeGo
{
    public class AntiStealth
    {
        public static bool TryDeStealth(Vector3 pos = new Vector3(), int level = 1)
        {
            if (!TryConsumables())
                return TrySpells(pos, level);

            return true;
        }

        private static bool TryConsumables()
        {
            SpellSlot slot = SpellSlot.Unknown;
            if (Data.Config.Item("USEVISIONWARD").GetValue<bool>())
            {
                if (Items.CanUseItem(2043) && Items.HasItem(2043))
                    slot = ObjectManager.Player.GetSpellSlot("VisionWard");
                else if ((Items.CanUseItem(3362) && Items.HasItem(3362)))
                    slot = SpellSlot.Trinket;
            }

            if (Data.Config.Item("USEORACLESLENS").GetValue<bool>())
            {
                if (Items.CanUseItem(3364) && Items.HasItem(3364))
                    slot = SpellSlot.Trinket;
            }

            if (slot != SpellSlot.Unknown)
                ObjectManager.Player.Spellbook.CastSpell(slot, ObjectManager.Player.ServerPosition);

            return slot != SpellSlot.Unknown;
        }

        private static bool TrySpells(Vector3 pos, int level)
        {
            var spells = Data.AntiStealthSpells.Where(p => p.ChampionName == ObjectManager.Player.ChampionName.ToLower());
            foreach (var spell in spells)
            {
                if (Data.Config.Item(String.Format("USE{0}", spell.Spell.Slot.ToString())).GetValue<bool>() && Data.Config.Item(String.Format("DETECT{0}", spell.Spell.Slot.ToString())).GetValue<Slider>().Value >= level)
                {
                    if (spell.Spell.IsReady())
                    {
                        if (spell.SelfCast)
                            spell.Spell.Cast();
                        else
                        {
                            if (!ObjectManager.Player.HasBuff("rengarralertsound") && pos.Distance(ObjectManager.Player.ServerPosition) <= spell.Spell.Range)
                                spell.Spell.Cast(pos);
                        }

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
