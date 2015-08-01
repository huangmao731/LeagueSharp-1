using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ShineCommon.Maths
{
    public class Prediction
    {
        public static float PredictSpeed(Obj_AI_Hero target, int msafter = 0)
        {
            float percent_remove = 0;
            float flat_remove = 0;
            int elapsed = msafter;
            Parallel.ForEach(target.Buffs, p => 
            {
                var spl = SpellDatabase.MovementBuffers.FirstOrDefault(q => q.SpellName == p.Name); ;
                if (spl != null)
                {
                    float leftTime = p.EndTime - p.StartTime;
                    float percent = 0;
                    float extra = 0;
                    if (spl.Percent != null)
                    {
                        if (spl.Percent.Length == 1)
                            percent = spl.Percent[0];
                        else
                        {
                            Obj_AI_Hero caster = (Obj_AI_Hero)p.Caster;
                            percent = spl.Percent[caster.Spellbook.Spells[(int)spl.Slot].Level];
                        }
                    }
                    else
                    {
                        
                        if (spl.Extra.Length == 1)
                            extra = spl.Percent[0];
                        else
                        {
                            Obj_AI_Hero caster = (Obj_AI_Hero)p.Caster;
                            extra = spl.Extra[caster.Spellbook.Spells[(int)spl.Slot].Level];
                        }
                    }
                    if (spl.IsDecaying)
                    {
                        if (extra != 0 && spl.DecaysTo > extra)
                        {
                            extra = (spl.DecaysTo - extra) / spl.DecayTime * (Game.Time - p.StartTime);
                        }
                        else if (percent != 0 && spl.DecaysTo > percent)
                        {
                            percent = (spl.DecaysTo - percent) / spl.DecayTime * (Game.Time - p.StartTime);
                        }
                    }
                    //if(elapsed - leftTime <= 0)
                }
            });
            return 0;
        }
    }
}
