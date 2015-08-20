/*
 Copyright 2015 - 2015 SPrediction
 Prediction.cs is part of SPrediction
 
 SPrediction is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 SPrediction is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with SPrediction. If not, see <http://www.gnu.org/licenses/>.
*/

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
    /// <summary>
    /// Shine Prediction class
    /// </summary>
    public static class Prediction
    {
        public static Dictionary<int, EnemyData> EnemyInfo = new Dictionary<int, EnemyData>();
        private static bool blInitialized;
        private static Menu predMenu;

        /// <summary>
        /// Initializes Prediction Services
        /// </summary>
        public static void Initialize(Menu mainMenu = null)
        {
            foreach (Obj_AI_Hero enemy in HeroManager.Enemies)
                EnemyInfo.Add(enemy.NetworkId, new EnemyData(new List<Vector2>()));

            Obj_AI_Hero.OnNewPath += Obj_AI_Hero_OnNewPath;

            if (mainMenu != null)
            {
                predMenu = new Menu("Shine Prediction", "SHINEPRED");
                predMenu.AddItem(new MenuItem("SHINEPREDREACTIONDELAY", "Ignore Rection Delay").SetValue<Slider>(new Slider(0, 0, 200)));
                mainMenu.AddSubMenu(predMenu);
            }

            blInitialized = true;
        }
       
        /// <summary>
        /// Gets Predicted position
        /// </summary>
        /// <param name="target">Target for spell</param>
        /// <param name="s">Spell to cast</param>
        /// <param name="path">Waypoint of target</param>
        /// <param name="avgt">Average reaction time (in ms)</param>
        /// <param name="movt">Passed time from last movement change (in ms)</param>
        /// <param name="hc">Predicted HitChance</param>
        /// <param name="rangeCheckFrom">Position where spell will be casted from</param>
        /// <returns>Predicted position and HitChance out value</returns>
        public static Vector2 GetPrediction(Obj_AI_Hero target, Spell s, List<Vector2> path, float avgt, float movt, out HitChance hc, Vector3 rangeCheckFrom)
        {
            if (!blInitialized)
                throw new InvalidOperationException("Prediction is not initalized");

            if (target.MovImmobileTime() > 200 || target.AvgMovChangeTime() == 0 || Utility.IsImmobileTarget(target)) //if target is not moving, easy to hit
            {
                hc = HitChance.Immobile;
                return target.ServerPosition.To2D() + target.Direction.To2D().Perpendicular() * s.Width / 2;
            }

            if (target.IsDashing())
                return GetDashingPrediction(target, s, out hc, rangeCheckFrom);

            float targetDistance = rangeCheckFrom.Distance(target.ServerPosition);
            float flyTime = 0f;
             
            if (s.Speed != 0) //skillshot with a missile
            {
                Vector2 Vt = (path[path.Count - 1] - path[0]).Normalized() * target.MoveSpeed;
                Vector2 Vs = (target.ServerPosition.To2D() - rangeCheckFrom.To2D()).Normalized() * s.Speed;
                Vector2 Vr = Vs - Vt;

                flyTime = targetDistance / Vr.Length();

                if (path.Count > 5) //complicated movement
                    flyTime = targetDistance / s.Speed;
            }

            float t = flyTime + s.Delay + Game.Ping / 1000f;
            float distance = t * target.MoveSpeed;

            //can be improved by checking area of circle
            if (s.Type == SkillshotType.SkillshotCircle) //haven't tested yet.
                distance -= s.Width / 2;

            hc = GetHitChance(t * 1000f, avgt, movt);

            for (int i = 0; i < path.Count - 1; i++)
            {
                float d = path[i + 1].Distance(path[i]);
                if (distance == d)
                    return path[i + 1];
                else if (distance < d)
                    return path[i] + distance * (path[i + 1] - path[i]).Normalized();
                else distance -= d;
            }

            hc = HitChance.Impossible;
            return path[path.Count - 1];
        }

        /// <summary>
        /// Gets Predicted position for arc
        /// </summary>
        /// <param name="target">Target for spell</param>
        /// <param name="s">Spell to cast</param>
        /// <param name="avgt">Average reaction time (in ms)</param>
        /// <param name="movt">Passed time from last movement change (in ms)</param>
        /// <param name="hc">Predicted HitChance</param>
        /// <param name="rangeCheckFrom">Position where spell will be casted from</param>
        /// <returns>Predicted position and HitChance out value</returns>
        public static Vector2 GetArcPrediction(Obj_AI_Hero target, Spell s, List<Vector2> path, float avgt, float movt, out HitChance hc, Vector3 rangeCheckFrom)
        {
            if (!blInitialized)
                throw new InvalidOperationException("Prediction is not initalized");

            if (target.MovImmobileTime() > 200 || target.AvgMovChangeTime() == 0 || Utility.IsImmobileTarget(target)) //if target is not moving, easy to hit
            {
                hc = HitChance.Immobile;
                return target.ServerPosition.To2D();
            }

            if (target.IsDashing())
                return GetDashingPrediction(target, s, out hc, rangeCheckFrom);

            float targetDistance = rangeCheckFrom.Distance(target.ServerPosition);
            float flyTime = 0f;

            if (s.Speed != 0)
            {
                Vector2 Vt = (path[path.Count - 1] - path[0]).Normalized() * target.MoveSpeed;
                Vector2 Vs = (target.ServerPosition.To2D() - rangeCheckFrom.To2D()).Normalized() * s.Speed;
                Vector2 Vr = Vs - Vt;

                flyTime = targetDistance / Vr.Length();

                if (path.Count > 5)
                    flyTime = targetDistance / s.Speed;
            }

            float t = flyTime + s.Delay + Game.Ping / 1000f;
            float distance = t * target.MoveSpeed;

            hc = GetHitChance(t * 1000f, avgt, movt);

            #region arc collision test
            for (int i = 1; i < path.Count; i++)
            {
                Vector2 senderPos = ObjectManager.Player.ServerPosition.To2D();
                Vector2 testPos = path[i];

                float multp = (testPos.Distance(senderPos) / 875.0f);

                var dianaArc = new ShineCommon.Maths.Geometry.Polygon(
                                ClipperWrapper.DefineArc(senderPos - new Vector2(875 / 2f, 20), testPos, (float)Math.PI * multp, 410, 200 * multp),
                                ClipperWrapper.DefineArc(senderPos - new Vector2(875 / 2f, 20), testPos, (float)Math.PI * multp, 410, 320 * multp));

                if (!ClipperWrapper.IsOutside(dianaArc, target.ServerPosition.To2D()))
                {
                    hc = HitChance.VeryHigh;
                    return testPos;
                }
            }
            #endregion

            for (int i = 0; i < path.Count - 1; i++)
            {
                float d = path[i + 1].Distance(path[i]);
                if (distance == d)
                    return path[i + 1];
                else if (distance < d)
                    return path[i] + distance * (path[i + 1] - path[i]).Normalized();
                else distance -= d;
            }

            hc = HitChance.Impossible;
            return path[path.Count - 1];
        }

        /// <summary>
        /// Gets Predicted position while target is dashing
        /// </summary>
        private static Vector2 GetDashingPrediction(Obj_AI_Hero target, Spell s, out HitChance hc, Vector3 rangeCheckFrom)
        {
            if (target.IsDashing())
            {
                var dashInfo = target.GetDashInfo();
                
                float dashPassedDistance = (Utils.TickCount - dashInfo.StartTick) / 1000f * dashInfo.Speed;
                Vector2 currentDashPos = dashInfo.StartPos + (dashInfo.EndPos - dashInfo.StartPos).Normalized() * dashPassedDistance;

                float targetDistance = rangeCheckFrom.To2D().Distance(currentDashPos);
                float flyTime = 0f;

                if (s.Speed != 0) //skillshot with a missile
                {
                    Vector2 Vt = (dashInfo.Path[dashInfo.Path.Count - 1] - dashInfo.Path[0]).Normalized() * dashInfo.Speed;
                    Vector2 Vs = (target.ServerPosition.To2D() - rangeCheckFrom.To2D()).Normalized() * s.Speed;
                    Vector2 Vr = Vs - Vt;

                    flyTime = targetDistance / Vr.Length();
                }
                int dashLeftTime = dashInfo.EndTick - Utils.TickCount;
                float t = flyTime + s.Delay + Game.Ping / 1000f;

                if (dashLeftTime >= t * 1000f)
                {
                    float distance = t * dashInfo.Speed;
                    hc = HitChance.Dashing;

                    for (int i = 0; i < dashInfo.Path.Count - 1; i++)
                    {
                        float d = dashInfo.Path[i + 1].Distance(dashInfo.Path[i]);
                        if (distance == d)
                            return dashInfo.Path[i + 1];
                        else if (distance < d)
                            return dashInfo.Path[i] + distance * (dashInfo.Path[i + 1] - dashInfo.Path[i]).Normalized();
                        else distance -= d;
                    }
                }
            }

            hc = HitChance.Impossible;
            return rangeCheckFrom.To2D();
        }

        /// <summary>
        /// Casts spell
        /// </summary>
        /// <param name="s">Spell to cast</param>
        /// <param name="t">Target for spell</param>
        /// <param name="hc">Minimum HitChance to cast</param>
        /// <param name="reactionIgnoreDelay">Delay to ignore target's reaction time</param>
        /// <param name="minHit">Minimum Hit Count to cast</param>
        /// <param name="rangeCheckFrom">Position where spell will be casted from</param>
        /// <param name="filterHPPercent">Minimum HP Percent to cast (for target)</param>
        /// <returns>true if spell has casted</returns>
        public static bool Cast(this Spell s, Obj_AI_Hero t, HitChance hc, int reactionIgnoreDelay = 0, byte minHit = 1, Vector3? rangeCheckFrom = null, float filterHPPercent = 100)
        {
            if (rangeCheckFrom == null)
                rangeCheckFrom = ObjectManager.Player.ServerPosition;

            if (minHit > 1)
                return Aoe.Cast(s, t, hc, reactionIgnoreDelay, minHit, rangeCheckFrom, filterHPPercent);

            if (t.HealthPercent > filterHPPercent)
                return false;

            if (Monitor.TryEnter(EnemyInfo[t.NetworkId].m_lock))
            {
                try
                {
                    HitChance predictedhc;
                    float avgt = t.AvgMovChangeTime() + reactionIgnoreDelay;
                    float movt = t.LastMovChangeTime();
                    Vector2 pos = GetPrediction(t, s, t.GetWaypoints(), avgt, movt, out predictedhc, rangeCheckFrom.Value);

                    if (rangeCheckFrom.Value.To2D().Distance(pos) > s.Range + (s.Type == SkillshotType.SkillshotCircle ? s.Width / 2 : 0)) //out of range
                    {
                        Monitor.Pulse(EnemyInfo[t.NetworkId].m_lock);
                        return false;
                    }
                    
                    if (s.Collision && s.GetCollision(rangeCheckFrom.Value.To2D(), new List<Vector2> { pos }).Exists(q => q.IsEnemy)) //needs update
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

        /// <summary>
        /// Casts spell
        /// </summary>
        /// <param name="s">Spell to cast</param>
        /// <param name="t">Target for spell</param>
        /// <param name="hc">Minimum HitChance to cast</param>
        /// <param name="reactionIgnoreDelay">Delay to ignore target's reaction time</param>
        /// <param name="minHit">Minimum Hit Count to cast</param>
        /// <param name="rangeCheckFrom">Position where spell will be casted from</param>
        /// <param name="filterHPPercent">Minimum HP Percent to cast (for target)</param>
        /// <returns>true if spell has casted</returns>
        public static bool CastArc(this Spell s, Obj_AI_Hero t, HitChance hc, int reactionIgnoreDelay = 0, byte minHit = 1, Vector3? rangeCheckFrom = null, float filterHPPercent = 100)
        {
            if (minHit > 1)
                throw new NotSupportedException("Arc aoe prediction has not supported yet");

            if (t.HealthPercent > filterHPPercent)
                return false;

            if (rangeCheckFrom == null)
                rangeCheckFrom = ObjectManager.Player.ServerPosition;
            
            if (Monitor.TryEnter(EnemyInfo[t.NetworkId].m_lock))
            {
                try
                {
                    HitChance predictedhc;
                    float avgt = t.AvgMovChangeTime() + reactionIgnoreDelay;
                    float movt = t.LastMovChangeTime();
                    Vector2 pos = GetArcPrediction(t, s, t.GetWaypoints(), avgt, movt, out predictedhc, rangeCheckFrom.Value);

                    if (rangeCheckFrom.Value.To2D().Distance(pos) > s.Range + s.Width / 2) //out of range
                    {
                        Monitor.Pulse(EnemyInfo[t.NetworkId].m_lock);
                        return false;
                    }

                    if (s.Collision && s.GetCollision(rangeCheckFrom.Value.To2D(), new List<Vector2> { pos }).Exists(q => q.IsEnemy)) //needs update
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

        /// <summary>
        /// Class for aoe spell castings
        /// </summary>
        public static class Aoe
        {
            /// <summary>
            /// Casts aoe spell
            /// </summary>
            /// <param name="s">Spell to cast</param>
            /// <param name="t">Target for spell</param>
            /// <param name="hc">Minimum HitChance to cast</param>
            /// <param name="minHit">Minimum Hit Count to cast</param>
            /// <param name="rangeCheckFrom">Position where spell will be casted from</param>
            /// <param name="filterHPPercent">Minimum HP Percent to cast (for target)</param>
            /// <returns>true if spell has casted</returns>
            public static bool Cast(Spell s, Obj_AI_Hero t, HitChance hc, int reactionIgnoreDelay = 0, byte minHit = 2, Vector3? rangeCheckFrom = null, float filterHPPercent = 0)
            {
                if (!blInitialized)
                    throw new Exception("Prediction is not initalized");

                if (rangeCheckFrom == null)
                    rangeCheckFrom = ObjectManager.Player.ServerPosition;

                if (Monitor.TryEnter(EnemyInfo[t.NetworkId].m_lock))
                {
                    try
                    {
                        HitChance predictedhc = HitChance.Impossible;
                        float avgt = t.AvgMovChangeTime() + reactionIgnoreDelay;
                        float movt = t.LastMovChangeTime();
                        Vector2 pos = ObjectManager.Player.ServerPosition.To2D();
                        switch (s.Type)
                        {
                            case SkillshotType.SkillshotLine:   pos = Line.GetPrediction(t, s, t.GetWaypoints(), avgt, movt, filterHPPercent, minHit, out predictedhc, rangeCheckFrom.Value);
                                break;
                            case SkillshotType.SkillshotCircle: pos = Circle.GetPrediction(t, s, t.GetWaypoints(), avgt, movt, filterHPPercent, minHit, out predictedhc, rangeCheckFrom.Value);
                                break;
                            case SkillshotType.SkillshotCone:   pos = Cone.GetPrediction(t, s, t.GetWaypoints(), avgt, movt, filterHPPercent, minHit, out predictedhc, rangeCheckFrom.Value);
                                break;
                        }
                        
                        //pos = pos + pos.Perpendicular() * s.Width / 2; //need moar test (for lineaar skillshots)
                        if (s.Collision && s.GetCollision(rangeCheckFrom.Value.To2D(), new List<Vector2> { pos }).Exists(q => q.IsEnemy)) //needs update
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

            //modified common aoe classes, i think logic is nice

            /// <summary>
            /// aoe line prediction class
            /// </summary>
            public static class Line
            {
                private static IEnumerable<Vector2> GetHits(Vector2 start, Vector2 end, double radius, List<Vector2> points)
                {
                    return points.Where(p => p.Distance(start, end, true, true) <= radius * radius);
                }

                private static bool GetCandidates(Vector2 from, Vector2 to, float radius, float range, out Vector2[] vec)
                {
                    var middlePoint = (from + to) / 2;
                    var intersections = LeagueSharp.Common.Geometry.CircleCircleIntersection(
                        from, middlePoint, radius, from.Distance(middlePoint));

                    if (intersections.Length > 1)
                    {
                        var c1 = intersections[0];
                        var c2 = intersections[1];

                        c1 = from + range * (to - c1).Normalized();
                        c2 = from + range * (to - c2).Normalized();

                        vec = new[] { c1, c2 };
                        return true;
                    }

                    vec = new Vector2[] { };
                    return false;
                }
                
                public static Vector2 GetPrediction(Obj_AI_Hero t, Spell s, List<Vector2> path, float avgt, float movt, float filterHPPercent, byte minHit, out HitChance hc, Vector3 rangeCheckFrom)
                {
                    Vector2 castPos = Prediction.GetPrediction(t, s, path, avgt, movt, out hc, rangeCheckFrom);

                    var posibleTargets = new List<PossibleTarget>
                    {
                        new PossibleTarget { Position = t.ServerPosition.To2D(), Unit = t }
                    };

                    if (hc >= HitChance.Low)
                    {
                        //Add the posible targets  in range:
                        posibleTargets.AddRange(GetPossibleTargets(t, s, rangeCheckFrom, filterHPPercent));
                        if (posibleTargets.Count < minHit)
                        {
                            hc = HitChance.Impossible;
                            return castPos;
                        }
                    }

                    if (posibleTargets.Count > 1)
                    {
                        var candidates = new List<Vector2>();
                        foreach (var target in posibleTargets)
                        {
                            Vector2[] v;
                            if (GetCandidates(rangeCheckFrom.To2D(), target.Position, s.Width, s.Range, out v))
                                candidates.AddRange(v);
                        }

                        var bestCandidateHits = -1;
                        var bestCandidate = new Vector2();
                        var bestCandidateHitPoints = new List<Vector2>();
                        var positionsList = posibleTargets.Select(p => p.Position).ToList();

                        foreach (var candidate in candidates)
                        {
                            if (GetHits(rangeCheckFrom.To2D(), candidate, s.Width, new List<Vector2> { posibleTargets[0].Position }).Count() == 1)
                            {
                                var hits = GetHits(rangeCheckFrom.To2D(), candidate, s.Width, positionsList).ToList();
                                var hitsCount = hits.Count;
                                if (hitsCount >= bestCandidateHits)
                                {
                                    bestCandidateHits = hitsCount;
                                    bestCandidate = candidate;
                                    bestCandidateHitPoints = hits.ToList();
                                }
                            }
                        }

                        if (bestCandidateHits > 1)
                        {
                            float maxDistance = -1;
                            Vector2 p1 = new Vector2(), p2 = new Vector2();

                            //Center the position
                            for (var i = 0; i < bestCandidateHitPoints.Count; i++)
                            {
                                for (var j = 0; j < bestCandidateHitPoints.Count; j++)
                                {
                                    var startP = rangeCheckFrom.To2D();
                                    var endP = bestCandidate;
                                    var proj1 = positionsList[i].ProjectOn(startP, endP);
                                    var proj2 = positionsList[j].ProjectOn(startP, endP);
                                    var dist = Vector2.DistanceSquared(bestCandidateHitPoints[i], proj1.LinePoint) +
                                               Vector2.DistanceSquared(bestCandidateHitPoints[j], proj2.LinePoint);
                                    if (dist >= maxDistance &&
                                        (proj1.LinePoint - positionsList[i]).AngleBetween(
                                            proj2.LinePoint - positionsList[j]) > 90)
                                    {
                                        maxDistance = dist;
                                        p1 = positionsList[i];
                                        p2 = positionsList[j];
                                    }
                                }
                            }

                            if (bestCandidateHits < minHit)
                                hc = HitChance.Impossible;

                            return (p1 + p2) * 0.5f;
                        }
                    }

                    hc = HitChance.Impossible;
                    return castPos;
                }
            }

            /// <summary>
            /// aoe circle prediction class
            /// </summary>
            public static class Circle
            {
                public static Vector2 GetPrediction(Obj_AI_Hero t, Spell s, List<Vector2> path, float avgt, float movt, float filterHPPercent, byte minHit, out HitChance hc, Vector3 rangeCheckFrom)
                {
                    Vector2 castPos = Prediction.GetPrediction(t, s, path, avgt, movt, out hc, rangeCheckFrom);
                    var posibleTargets = new List<PossibleTarget>
                    {
                        new PossibleTarget { Position = t.ServerPosition.To2D(), Unit = t }
                    };

                    if (hc >= HitChance.Low)
                    {
                        //Add the posible targets  in range:
                        posibleTargets.AddRange(GetPossibleTargets(t, s, rangeCheckFrom, filterHPPercent));
                        if (posibleTargets.Count < minHit)
                        {
                            hc = HitChance.Impossible;
                            return castPos;
                        }
                    }


                    while (posibleTargets.Count > 1)
                    {
                        var mecCircle = MEC.GetMec(posibleTargets.Select(h => h.Position).ToList());

                        if (mecCircle.Radius <= s.Width - 10 && Vector2.DistanceSquared(mecCircle.Center, rangeCheckFrom.To2D()) < s.Range * s.Range)
                        {
                            if (posibleTargets.Count < minHit)
                                hc = HitChance.Impossible;

                            return mecCircle.Center;
                        }

                        float maxdist = -1;
                        var maxdistindex = 1;
                        for (var i = 1; i < posibleTargets.Count; i++)
                        {
                            var distance = Vector2.DistanceSquared(posibleTargets[i].Position, posibleTargets[0].Position);
                            if (distance > maxdist || maxdist.CompareTo(-1) == 0)
                            {
                                maxdistindex = i;
                                maxdist = distance;
                            }
                        }
                        posibleTargets.RemoveAt(maxdistindex);
                    }

                    hc = HitChance.Impossible;
                    return castPos;
                }
            }

            /// <summary>
            /// aoe cone prediction class
            /// </summary>
            public static class Cone
            {
                private static int GetHits(Vector2 end, double range, float angle, List<Vector2> points)
                {
                    return (from point in points
                            let edge1 = end.Rotated(-angle / 2)
                            let edge2 = edge1.Rotated(angle)
                            where
                                point.Distance(new Vector2(), true) < range * range && edge1.CrossProduct(point) > 0 &&
                                point.CrossProduct(edge2) > 0
                            select point).Count();
                }

                public static Vector2 GetPrediction(Obj_AI_Hero t, Spell s, List<Vector2> path, float avgt, float movt, float filterHPPercent, byte minHit, out HitChance hc, Vector3 rangeCheckFrom)
                {
                    Vector2 castPos = Prediction.GetPrediction(t, s, path, avgt, movt, out hc, rangeCheckFrom);
                    var posibleTargets = new List<PossibleTarget>
                    {
                        new PossibleTarget { Position = t.ServerPosition.To2D(), Unit = t }
                    };

                    if (hc >= HitChance.Low)
                    {
                        //Add the posible targets  in range:
                        posibleTargets.AddRange(GetPossibleTargets(t, s, rangeCheckFrom, filterHPPercent));
                        if (posibleTargets.Count < minHit)
                        {
                            hc = HitChance.Impossible;
                            return castPos;
                        }
                    }

                    if (posibleTargets.Count > 1)
                    {
                        var candidates = new List<Vector2>();

                        foreach (var target in posibleTargets)
                        {
                            target.Position = target.Position - rangeCheckFrom.To2D();
                        }

                        for (var i = 0; i < posibleTargets.Count; i++)
                        {
                            for (var j = 0; j < posibleTargets.Count; j++)
                            {
                                if (i != j)
                                {
                                    var p = (posibleTargets[i].Position + posibleTargets[j].Position) * 0.5f;
                                    if (!candidates.Contains(p))
                                    {
                                        candidates.Add(p);
                                    }
                                }
                            }
                        }

                        var bestCandidateHits = -1;
                        var bestCandidate = new Vector2();
                        var positionsList = posibleTargets.Select(p => p.Position).ToList();

                        foreach (var candidate in candidates)
                        {
                            var hits = GetHits(candidate, s.Range, s.Width, positionsList);
                            if (hits > bestCandidateHits)
                            {
                                bestCandidate = candidate;
                                bestCandidateHits = hits;
                            }
                        }

                        if (bestCandidateHits < minHit)
                            hc = HitChance.Impossible;

                        if (bestCandidateHits > 1 && rangeCheckFrom.To2D().Distance(bestCandidate, true) > 50 * 50)
                        {
                            return bestCandidate;
                        }
                    }

                    hc = HitChance.Impossible;
                    return castPos;
                }
            }

            private class PossibleTarget
            {
                public Vector2 Position;
                public Obj_AI_Base Unit;
            }

            private static List<PossibleTarget> GetPossibleTargets(Obj_AI_Hero target, Spell s, Vector3 rangeCheckFrom, float filterHPPercent)
            {
                var result = new List<PossibleTarget>();
                var originalUnit = target;
                var enemies = HeroManager.Enemies.FindAll(h => h.NetworkId != originalUnit.NetworkId && h.IsValidTarget(s.Range, true, rangeCheckFrom) && h.Health / h.MaxHealth * 100 <= filterHPPercent);
                foreach (var enemy in enemies)
                {
                    HitChance hc;
                    var prediction = Prediction.GetPrediction(enemy, s, enemy.GetWaypoints(), enemy.AvgMovChangeTime(), enemy.LastMovChangeTime(), out hc, rangeCheckFrom);
                    if (hc >= HitChance.High)
                    {
                        result.Add(new PossibleTarget { Position = prediction, Unit = enemy });
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// Get HitChance
        /// </summary>
        /// <param name="t">Arrive time to target (in ms)</param>
        /// <param name="avgt">Average reaction time (in ms)</param>
        /// <param name="movt">Passed time from last movement change (in ms)</param>
        /// <returns>HitChance</returns>
        private static HitChance GetHitChance(float t, float avgt, float movt)
        {
            if (avgt - movt >= t)
                return HitChance.VeryHigh;
            else if (avgt - movt >= t * 0.5f)
                return HitChance.High;
            else if (avgt - movt >= t && avgt / movt >= 1.5f)
                return HitChance.Medium;
            else
                return HitChance.Low;
        }

        private static int IgnoreRectionDelay
        {
            get
            {
                if (predMenu == null)
                    return 0;
                return predMenu.Item("SHINEPREDREACTIONDELAY").GetValue<Slider>().Value;
            }
        }

        private static void Obj_AI_Hero_OnNewPath(Obj_AI_Base sender, GameObjectNewPathEventArgs args)
        {
            if (!sender.IsEnemy || !sender.IsChampion() || args.IsDash)
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

        #region Obj_AI_Hero extensions
        /// <summary>
        /// Gets passed time without moving
        /// </summary>
        /// <param name="t">target</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int MovImmobileTime(this Obj_AI_Hero t)
        {
            if(!blInitialized)
                throw new InvalidOperationException("Prediction is not initalized");

            return EnemyInfo[t.NetworkId].IsStopped ? Environment.TickCount - EnemyInfo[t.NetworkId].StopTick : 0;
        }
        /// <summary>
        /// Gets passed time from last movement change
        /// </summary>
        /// <param name="t">target</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int LastMovChangeTime(this Obj_AI_Hero t)
        {
            if (!blInitialized)
                throw new InvalidOperationException("Prediction is not initalized");

            return Environment.TickCount - EnemyInfo[t.NetworkId].LastWaypointTick;
        }

        /// <summary>
        /// Gets average movement reaction time
        /// </summary>
        /// <param name="t"><target/param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float AvgMovChangeTime(this Obj_AI_Hero t)
        {
            if (!blInitialized)
                throw new InvalidOperationException("Prediction is not initalized");

            return EnemyInfo[t.NetworkId].AvgTick + IgnoreRectionDelay;
        }
        #endregion

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
    }
}
