/*
 Copyright 2015 - 2015 ShineCommon
 ClipperWrapper.cs is part of ShineCommon
 
 ShineCommon is free software: you can redistribute it and/or modify
 it under the terms of the GNU General Public License as published by
 the Free Software Foundation, either version 3 of the License, or
 (at your option) any later version.
 
 ShineCommon is distributed in the hope that it will be useful,
 but WITHOUT ANY WARRANTY; without even the implied warranty of
 MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 GNU General Public License for more details.
 
 You should have received a copy of the GNU General Public License
 along with ShineCommon. If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using LeagueSharp;
using LeagueSharp.Common;
using ClipperLib;
//typedefs
using Path = System.Collections.Generic.List<ClipperLib.IntPoint>;
using Paths = System.Collections.Generic.List<System.Collections.Generic.List<ClipperLib.IntPoint>>;

namespace ShineCommon.Maths
{
    public static class ClipperWrapper
    {
        public static bool IsIntersects(Paths p1, Paths p2)
        {
            Clipper c = new Clipper();
            Paths solution = new Paths();
            c.AddPaths(p1, PolyType.ptSubject, true);
            c.AddPaths(p2, PolyType.ptClip, true);
            c.Execute(ClipType.ctIntersection, solution, PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);

            return solution.Count != 0;
        }

        public static Geometry.Polygon DefineRectangle(Vector2 start, Vector2 end, float scale)
        {
            return new Geometry.Rectangle(start, end, scale).Polygons;
        }

        public static Geometry.Polygon DefineCircle(Vector2 c, float r)
        {
            return new Geometry.Circle(c, r).Polygons;
        }

        public static Geometry.Polygon DefineSector(Vector2 center, Vector2 direction, float angle, float radius)
        {
            return new Geometry.Sector(center, direction, angle, radius).Polygons;
        }

        public static Geometry.Polygon DefineArc(Vector2 c, Vector2 direction, float angle, float w, float h)
        {
            return new Geometry.Arc(c, direction, angle, w, h).Polygons;
        }

        public static Paths MakePaths(params Geometry.Polygon[] plist)
        {
            Paths ps = new Paths(plist.Length);
            for (int i = 0; i < plist.Length; i++)
                ps.Add(plist[i].ToClipperPath());
            return ps;
        }

        public static Path ToClipperPath(this Geometry.Polygon val)
        {
            var result = new Path(val.Points.Count);

            foreach (var point in val.Points)
            {
                result.Add(new IntPoint(point.X, point.Y));
            }

            return result;
        }

        public static bool IsOutside(this Geometry.Polygon val, Vector2 point)
        {
            var p = new IntPoint(point.X, point.Y);
            return Clipper.PointInPolygon(p, val.ToClipperPath()) != 1;
        }
    }
}
