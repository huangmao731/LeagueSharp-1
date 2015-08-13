using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace ShineCommon.Maths
{
    public static class Prediction
    {
        public struct EnemyData
        {
            public bool IsStopped;
            public List<Vector2> LastWaypoints;
            public int LastWaypointTick;
            public int StopTick;
            public float AvgTick;
            public int Count;
            public object m_lock;

            public EnemyData(List<Vector2> wp)
            {
                IsStopped = false;
                LastWaypoints = wp;
                LastWaypointTick = 0;
                StopTick = 0;
                AvgTick = 0;
                Count = 0;
                m_lock = new object();
            }
        }

        public static Dictionary<int, EnemyData> EnemyInfo = new Dictionary<int, EnemyData>();
        private static bool blInitialized;

        public static void Initialize()
        {
            foreach (Obj_AI_Hero enemy in HeroManager.Enemies)
                EnemyInfo.Add(enemy.NetworkId, new EnemyData(new List<Vector2>()));

            Obj_AI_Hero.OnNewPath += Obj_AI_Hero_OnNewPath;
            blInitialized = true;
        }

        public static Vector2 GetPrediction(Obj_AI_Hero target, Spell s, List<Vector2> path, float avgt, float movt, out HitChance hc)
        {
            if (!blInitialized)
                throw new Exception("Prediction is not initalized");

            float dist_target = ObjectManager.Player.ServerPosition.Distance(target.ServerPosition);
            Vector2 Vt = (path[path.Count - 1] - path[0]).Normalized() * target.MoveSpeed;
            Vector2 Vs = (target.ServerPosition.To2D() - ObjectManager.Player.ServerPosition.To2D()).Normalized() * s.Speed;
            Vector2 Vr = Vs - Vt;

            float flytime = 0f;
             
            if (s.Speed != 0)
            {
                flytime = dist_target / Vr.Length();

                if (path.Count > 5) //complicated movement
                    flytime = dist_target / s.Speed;
            }

            float t = flytime + s.Delay + Game.Ping / 1000f;
            float t_ms = t * 1000f;

            float distance = t * target.MoveSpeed;

            //can be improved by checking area of circle
            if (s.Type == SkillshotType.SkillshotCircle) //haven't tested yet.
                distance -= s.Width / 2;

            if (avgt - movt >= t_ms)
                hc = HitChance.VeryHigh;
            else if (avgt - movt >= t_ms * 0.5f)
                hc = HitChance.High;
            else if (avgt - movt >= t_ms && avgt / movt >= 1.5f)
                hc = HitChance.Medium;
            else
                hc = HitChance.Low;

            for (int i = 0; i < path.Count - 1; i++)
            {
                float d = path[i + 1].Distance(path[i]);
                if (distance == d)
                {
                    return path[i + 1];
                }
                else if (distance < d)
                {
                    return path[i] + distance * (path[i + 1] - path[i]).Normalized();
                }
                else distance -= d;
            }

            hc = HitChance.Impossible;
            return path[path.Count - 1];
        }

        public static bool CastWithMovementCheck(this Spell s, Obj_AI_Hero t, HitChance hc = HitChance.Medium)
        {
            if (!blInitialized)
                throw new Exception("Prediction is not initalized");

            if (Monitor.TryEnter(EnemyInfo[t.NetworkId].m_lock))
            {
                try
                {
                    HitChance predictedhc;
                    if (t.MovImmobileTime() > 200 || t.AvgMovChangeTime() == 0)
                    {
                        s.Cast(t.ServerPosition.To2D() + t.Direction.To2D().Perpendicular() * s.Width / 2);

                        Monitor.Pulse(EnemyInfo[t.NetworkId].m_lock);
                        return true;
                    }
                    
                    float avgt = t.AvgMovChangeTime();
                    float movt = t.LastMovChangeTime();
                    Vector2 pos = GetPrediction(t, s, t.GetWaypoints(), avgt, movt, out predictedhc);

                    if (pos.Distance(t.ServerPosition.To2D()) > s.Range)
                    {
                        Monitor.Pulse(EnemyInfo[t.NetworkId].m_lock);
                        return false;
                    }

                    //pos = pos + pos.Perpendicular() * s.Width / 2; //need moar test (for lineaar skillshots)
                    if (s.Collision && s.GetCollision(ObjectManager.Player.ServerPosition.To2D(), new List<Vector2> { pos }).Exists(q => q.IsEnemy)) //needs update
                    {
                        Monitor.Pulse(EnemyInfo[t.NetworkId].m_lock);
                        return false;
                    }


                    if (predictedhc >= hc)
                    {
                        s.Cast(pos);

                        return true;
                    }

                    Monitor.Pulse(EnemyInfo[t.NetworkId].m_lock);
                    return false;
                }
                finally
                {
                    Monitor.Exit(EnemyInfo[t.NetworkId].m_lock);
                }
            }

            return false;
        }

        static void Obj_AI_Hero_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (!sender.IsEnemy || !sender.IsChampion())
                return;

            EnemyData enemy = EnemyInfo[sender.NetworkId];

            lock (enemy.m_lock)
            {
                if (args.Path.Length < 2)
                {
                    if (!enemy.IsStopped)
                    {
                        enemy.StopTick = Environment.TickCount;
                        enemy.LastWaypointTick = Environment.TickCount;
                        enemy.IsStopped = true;
                        enemy.Count = 0;
                        enemy.AvgTick = 0;
                    }
                }
                else
                {
                    List<Vector2> wp = args.Path.Select(p => p.To2D()).ToList();
                    if (!enemy.LastWaypoints.SequenceEqual(wp))
                    {
                        if (!enemy.IsStopped)
                            enemy.AvgTick = (enemy.Count * enemy.AvgTick + (Environment.TickCount - enemy.LastWaypointTick)) / ++enemy.Count;
                        enemy.LastWaypointTick = Environment.TickCount;
                        enemy.IsStopped = false;
                        enemy.LastWaypoints = wp;
                    }
                }

                EnemyInfo[sender.NetworkId] = enemy;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MovImmobileTime(this Obj_AI_Hero e)
        {
            return EnemyInfo[e.NetworkId].IsStopped ? Environment.TickCount - EnemyInfo[e.NetworkId].StopTick : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastMovChangeTime(this Obj_AI_Hero e)
        {
            return Environment.TickCount - EnemyInfo[e.NetworkId].LastWaypointTick;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AvgMovChangeTime(this Obj_AI_Hero e)
        {
            return EnemyInfo[e.NetworkId].AvgTick;
        }
    }
}
