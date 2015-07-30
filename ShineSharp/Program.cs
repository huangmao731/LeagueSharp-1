using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using ShineCommon;
using ShineSharp.Champions;

namespace ShineSharp
{
    class Program
    {
        public static BaseChamp Champion;
        static void Main(string[] args)
        {
            if (Game.Mode == GameMode.Running)
            {
                Game_OnGameLoad(null);
            }

            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            switch (ObjectManager.Player.ChampionName.ToLowerInvariant())
            {
                case "ezreal":
                    Champion = new Ezreal();
                    break;
                case "morgana":
                    Champion = new Morgana();
                    break;
                case "blitzcrank":
                    Champion = new Blitzcrank();
                    break;
            }

            Champion.CreateConfigMenu();
            Champion.SetSpells();

            Game.OnUpdate += Champion.Game_OnUpdate;
            Drawing.OnDraw += Champion.Drawing_OnDraw;
            Orbwalking.BeforeAttack += Champion.Orbwalking_BeforeAttack;
            Orbwalking.AfterAttack += Champion.Orbwalking_AfterAttack;
            AntiGapcloser.OnEnemyGapcloser += Champion.AntiGapcloser_OnEnemyGapcloser;

            Notifications.AddNotification(String.Format("Shine# - {0} Loaded", ObjectManager.Player.ChampionName), 3000);
        }
    }
}
