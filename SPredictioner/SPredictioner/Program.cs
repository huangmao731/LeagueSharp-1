using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SPrediction;

namespace SPredictioner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Game.Mode == GameMode.Running)
            {
                EventHandlers.Game_OnGameLoad(null);
            }

            CustomEvents.Game.OnGameLoad += EventHandlers.Game_OnGameLoad;
        }
    }
}