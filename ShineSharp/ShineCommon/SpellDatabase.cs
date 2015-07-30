using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ShineCommon
{
    [Flags]
    public enum EvadeMethods
    {
        Default = 0,
        EzrealE = 1,
        SivirE = 2,
        MorganaE = 4,
        QSS = 16,
        None = 32,
    }

    [Flags]
    public enum Collisions
    {
        None = 0,
        Minions = 1,
        Champions = 2,
        YasuoWall = 4,
    }

    public class SpellData
    {
        public string ChampionName;
        public string SpellName;
        public SpellSlot Slot;
        public bool IsSkillshot;
        public SkillshotType Type;
        public int Delay;
        public int Range;
        public int Radius;
        public int MissileSpeed;
        public bool IsDangerous;
        public string MissileSpellName;
        public EvadeMethods EvadeMethods;
        public Collisions Collisionable;
    }

    public class EscapeSpellData : SpellData
    {

    }

    public class SpellDatabase
    {
        public static List<SpellData> EvadeableSpells;

        public static void InitalizeSpellDatabase()
        {
            EvadeableSpells = new List<SpellData>();
            #region CC Spell Database
            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Ahri",
                    SpellName = "AhriSeduce",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 60,
                    MissileSpeed = 1550,
                    IsDangerous = true,
                    MissileSpellName = "AhriSeduceMissile",
                    EvadeMethods =  EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                    Collisionable = Collisions.Champions | Collisions.Minions | Collisions.YasuoWall 
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Amumu",
                    SpellName = "BandageToss",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 90,
                    MissileSpeed = 2000,
                    IsDangerous = true,
                    MissileSpellName = "SadMummyBandageToss",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                    Collisionable = Collisions.Champions | Collisions.Minions | Collisions.YasuoWall

                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Amumu",
                    SpellName = "CurseoftheSadMummy",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 0,
                    Radius = 550,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Annie",
                    SpellName = "InfernalGuardian",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 600,
                    Radius = 251,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Ashe",
                    SpellName = "EnchantedCrystalArrow",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 20000,
                    Radius = 130,
                    MissileSpeed = 1600,
                    IsDangerous = true,
                    MissileSpellName = "EnchantedCrystalArrow",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                    Collisionable = Collisions.Champions | Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Bard",
                    SpellName = "BardR",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 500,
                    Range = 3400,
                    Radius = 350,
                    MissileSpeed = 2100,
                    IsDangerous = false,
                    MissileSpellName = "BardR",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Blitzcrank",
                    SpellName = "RocketGrab",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1050,
                    Radius = 70,
                    MissileSpeed = 1800,
                    IsDangerous = true,
                    MissileSpellName = "RocketGrabMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE,
                    Collisionable = Collisions.Champions | Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Braum",
                    SpellName = "BraumRWrapper",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1200,
                    Radius = 115,
                    MissileSpeed = 1400,
                    IsDangerous = true,
                    MissileSpellName = "braumrmissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE,
                    Collisionable = Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Draven",
                    SpellName = "DravenDoubleShot",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 130,
                    MissileSpeed = 1400,
                    IsDangerous = true,
                    MissileSpellName = "DravenDoubleShotMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE,
                    Collisionable = Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Elise",
                    SpellName = "EliseHumanE",
                    IsSkillshot = true,
                    Slot = SpellSlot.E,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 55,
                    MissileSpeed = 1600,
                    IsDangerous = true,
                    MissileSpellName = "EliseHumanE",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE,
                    Collisionable = Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Evelynn",
                    SpellName = "EvelynnR",
                    IsSkillshot = true,
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 650,
                    Radius = 350,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "EvelynnR",
                    EvadeMethods =  EvadeMethods.EzrealE | EvadeMethods.SivirE
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Cassiopeia",
                    SpellName = "CassiopeiaPetrifyingGaze",
                    IsSkillshot = true,
                    Slot = SpellSlot.R,
                    Type = SkillshotType.SkillshotCone,
                    Delay = 600,
                    Range = 825,
                    Radius = 80,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "CassiopeiaPetrifyingGaze",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Fizz",
                    SpellName = "FizzMarinerDoom",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1300,
                    Radius = 120,
                    MissileSpeed = 1350,
                    IsDangerous = true,
                    MissileSpellName = "FizzMarinerDoomMissile",
                    Collisionable = Collisions.Champions | Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Galio",
                    SpellName = "GalioIdolOfDurand",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 0,
                    Radius = 550,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Gnar",
                    SpellName = "GnarR",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 0,
                    Radius = 500,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Gragas",
                    SpellName = "GragasE",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 0,
                    Range = 950,
                    Radius = 200,
                    MissileSpeed = 1200,
                    IsDangerous = false,
                    MissileSpellName = "GragasE",
                    Collisionable = Collisions.Champions | Collisions.Minions ,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE
                });



            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Gragas",
                    SpellName = "GragasR",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 250,
                    Range = 1050,
                    Radius = 375,
                    MissileSpeed = 1800,
                    IsDangerous = true,
                    MissileSpellName = "GragasRBoom",
                    Collisionable = Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Graves",
                    SpellName = "GravesChargeShot",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 100,
                    MissileSpeed = 2100,
                    IsDangerous = true,
                    MissileSpellName = "GravesChargeShotShot",
                    Collisionable = Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE 
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Leona",
                    SpellName = "LeonaSolarFlare",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 1000,
                    Range = 1200,
                    Radius = 300,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "LeonaSolarFlare",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Leona",
                    SpellName = "LeonaZenithBlade",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 905,
                    Radius = 70,
                    MissileSpeed = 2000,
                    IsDangerous = true,
                    MissileSpellName = "LeonaZenithBladeMissile",
                    Collisionable = Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Malphite",
                    SpellName = "UFSlash",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 0,
                    Range = 1000,
                    Radius = 270,
                    MissileSpeed = 1500,
                    IsDangerous = true,
                    MissileSpellName = "UFSlash",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Morgana",
                    SpellName = "DarkBindingMissile",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1300,
                    Radius = 80,
                    MissileSpeed = 1200,
                    IsDangerous = true,
                    MissileSpellName = "DarkBindingMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                    Collisionable = Collisions.Champions | Collisions.Minions | Collisions.YasuoWall
                });


            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Nami",
                    SpellName = "NamiQ",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 950,
                    Range = 1625,
                    Radius = 150,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "namiqmissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS
                });


            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Nautilus",
                    SpellName = "NautilusAnchorDrag",
                    IsSkillshot = true,
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1100,
                    Radius = 90,
                    MissileSpeed = 2000,
                    IsDangerous = true,
                    MissileSpellName = "NautilusAnchorDragMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE,
                    Collisionable = Collisions.Champions | Collisions.Minions | Collisions.YasuoWall
                });


            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Rengar",
                    SpellName = "RengarE",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 70,
                    MissileSpeed = 1500,
                    IsDangerous = true,
                    MissileSpellName = "RengarEFinal",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                    Collisionable = Collisions.Champions | Collisions.Minions | Collisions.YasuoWall
                });


            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Sona",
                    SpellName = "SonaR",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1000,
                    Radius = 140,
                    MissileSpeed = 2400,
                    IsDangerous = true,
                    MissileSpellName = "SonaR",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                    Collisionable = Collisions.YasuoWall
                });



            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Swain",
                    SpellName = "SwainShadowGrasp",
                    Slot = SpellSlot.W,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 1100,
                    Range = 900,
                    Radius = 180,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "SwainShadowGrasp",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Syndra",
                    SpellName = "syndrae5",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 300,
                    Range = 950,
                    Radius = 90,
                    MissileSpeed = 1601,
                    IsDangerous = false,
                    MissileSpellName = "syndrae5",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Syndra",
                    SpellName = "SyndraE",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 300,
                    Range = 950,
                    Radius = 90,
                    MissileSpeed = 1601,
                    IsDangerous = false,
                    MissileSpellName = "SyndraE",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Thresh",
                    SpellName = "ThreshQ",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1100,
                    Radius = 70,
                    MissileSpeed = 1900,
                    IsDangerous = true,
                    MissileSpellName = "ThreshQMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                    Collisionable = Collisions.Champions | Collisions.Minions | Collisions.YasuoWall
                });


            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Varus",
                    SpellName = "VarusQMissilee",
                    Slot = SpellSlot.Q,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1800,
                    Radius = 70,
                    MissileSpeed = 1900,
                    IsDangerous = false,
                    MissileSpellName = "VarusQMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE,
                    Collisionable = Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Varus",
                    SpellName = "VarusR",
                    Slot = SpellSlot.R,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1200,
                    Radius = 120,
                    MissileSpeed = 1950,
                    IsDangerous = true,
                    MissileSpellName = "VarusRMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                    Collisionable = Collisions.Champions | Collisions.YasuoWall
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Velkoz",
                    SpellName = "VelkozE",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 500,
                    Range = 800,
                    Radius = 225,
                    MissileSpeed = 1500,
                    IsDangerous = false,
                    MissileSpellName = "VelkozEMissile",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Yasuo",
                    SpellName = "yasuoq3w",
                    Slot = SpellSlot.Q,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1150,
                    Radius = 90,
                    MissileSpeed = 1500,
                    IsDangerous = true,
                    MissileSpellName = "yasuoq3w",
                    Collisionable = Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Zyra",
                    SpellName = "ZyraGraspingRoots",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 250,
                    Range = 1150,
                    Radius = 70,
                    MissileSpeed = 1150,
                    IsDangerous = true,
                    MissileSpellName = "ZyraGraspingRoots",
                    Collisionable = Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Zyra",
                    SpellName = "zyrapassivedeathmanager",
                    Slot = SpellSlot.E,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotLine,
                    Delay = 500,
                    Range = 1474,
                    Radius = 70,
                    MissileSpeed = 2000,
                    IsDangerous = true,
                    MissileSpellName = "zyrapassivedeathmanager",
                    Collisionable = Collisions.YasuoWall,
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Swain",
                    SpellName = "SwainShadowGrasp",
                    Slot = SpellSlot.W,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 1100,
                    Range = 900,
                    Radius = 180,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "SwainShadowGrasp",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });

            EvadeableSpells.Add(
                new SpellData
                {
                    ChampionName = "Swain",
                    SpellName = "SwainShadowGrasp",
                    Slot = SpellSlot.W,
                    IsSkillshot = true,
                    Type = SkillshotType.SkillshotCircle,
                    Delay = 1100,
                    Range = 900,
                    Radius = 180,
                    MissileSpeed = int.MaxValue,
                    IsDangerous = true,
                    MissileSpellName = "SwainShadowGrasp",
                    EvadeMethods = EvadeMethods.EzrealE | EvadeMethods.SivirE | EvadeMethods.MorganaE | EvadeMethods.QSS,
                });
            #endregion
            #region Escape Spell Data

            #endregion
        }
    }

    public class DetectedSpellData
    {
        public SpellData Spell;
        public Vector2 StartPosition;
        public Vector2 EndPosition;
        public Obj_AI_Base Sender;
        public GameObjectProcessSpellCastEventArgs Args;

        public void Set(SpellData s, Vector2 sp, Vector2 ep, Obj_AI_Base snd, GameObjectProcessSpellCastEventArgs ar)
        {
            Spell = s;
            StartPosition = sp;
            EndPosition = ep;
            Sender = snd;
            Args = ar;
        }
    }
}
