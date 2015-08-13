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
    public static class Geometry
    {
        //from Esk0r's evade's geometry class, orginal code: https://github.com/Esk0r/LeagueSharp/blob/master/Evade/Geometry.cs
        public class Polygon
        {
            public List<Vector2> Points = new List<Vector2>();

            public void Add(Vector2 point)
            {
                Points.Add(point);
            }

            public void Draw(int width = 1)
            {
                for (var i = 0; i <= Points.Count - 1; i++)
                {
                    var nextIndex = (Points.Count - 1 == i) ? 0 : (i + 1);
                    var start = Points[i].To3D();
                    var end = Points[nextIndex].To3D();
                    var from = Drawing.WorldToScreen(start);
                    var to = Drawing.WorldToScreen(end);
                    Drawing.DrawLine(from[0], from[1], to[0], to[1], width, System.Drawing.Color.White);
                }
            }
        }

        public class Circle
        {
            private const int CircleLineSegmentN = 22;
            public Vector2 Center;
            public float Radius;

            public Circle(float x, float y, float r)
            {
                Center = new Vector2(x, y);
                Radius = r;
            }

            public Circle(Vector2 c, float r)
            {
                Center = c;
                Radius = r;
            }

            public Polygon Polygons
            {
                get
                {
                    var result = new Polygon();
                    var outRadius = (Radius) / (float)Math.Cos(2 * Math.PI / CircleLineSegmentN);

                    for (var i = 1; i <= CircleLineSegmentN; i++)
                    {
                        var angle = i * 2 * Math.PI / CircleLineSegmentN;
                        Vector2 point = new Vector2(
                            Center.X + outRadius * (float)Math.Cos(angle), Center.Y + outRadius * (float)Math.Sin(angle));
                        result.Add(point);
                    }

                    return result;
                }
            }
        }

        public class Rectangle
        {
            public Vector2 Direction;
            public Vector2 Perpendicular;
            public Vector2 REnd;
            public Vector2 RStart;
            public float Width;

            public Rectangle(Vector2 start, Vector2 end, float width)
            {
                RStart = start;
                REnd = end;
                Width = width;
                Direction = (end - start).Normalized();
                Perpendicular = Direction.Perpendicular();
            }

            public Polygon Polygons
            {
                get
                {
                    var result = new Polygon();

                    result.Add(RStart + Width * Perpendicular);
                    result.Add(RStart - Width * Perpendicular);
                    result.Add(REnd - Width * Perpendicular);
                    result.Add(REnd + Width * Perpendicular);

                    return result;
                }
            }
        }

        public class Sector
        {
            private const int CircleLineSegmentN = 22;
            public float Angle;
            public Vector2 Center;
            public Vector2 Direction;
            public float Radius;

            public Sector(Vector2 center, Vector2 direction, float angle, float radius)
            {
                Center = center;
                Direction = direction;
                Angle = angle;
                Radius = radius;
            }

            public Polygon Polygons
            {
                get
                {
                    var result = new Polygon();
                    var outRadius = Radius / (float)Math.Cos(2 * Math.PI / CircleLineSegmentN);

                    result.Add(Center);
                    var Side1 = Direction.Rotated(-Angle * 0.5f);

                    for (var i = 0; i <= CircleLineSegmentN; i++)
                    {
                        var cDirection = Side1.Rotated(i * Angle / CircleLineSegmentN).Normalized();
                        result.Add(new Vector2(Center.X + outRadius * cDirection.X, Center.Y + outRadius * cDirection.Y));
                    }

                    return result;
                }
            }
        }

        public static Vector2 PositionAfter(List<Vector2> path, float t, float speed, float delay = 0)
        {
            if (delay >= t)
                throw new ArgumentException("Invalid Argument [delay]");
            var distance = (t - delay) * speed / 1000;
            for (var i = 0; i <= path.Count - 2; i++)
            {
                var d = path[i + 1].Distance(path[i]);
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
            return path[path.Count - 1];
        }

    }
}
