using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVGParser.Utils
{
    class EntityUtils
    {
        public static List<Entity> ConvertByNonUniformMatrix(Entity entity, Matrix3d matrix)
        {
            List<Entity> outs = new List<Entity>();
            Tuple<bool, List<Point3d>> pointResult = GetPoints(entity);

            bool isClosePath = pointResult.Item1;
            List<Point3d> points = pointResult.Item2;

            if (entity is Hatch)
            {
                if (points.Count >= 3)
                {
                    Hatch hatch = entity.Clone() as Hatch;
                    Point2d[] ps = new Point2d[points.Count];
                    double[] ds = new double[points.Count];
                    for (int i = 0; i < points.Count; i++)
                    {
                        ps[i] = to2d(clone3d(points[i]).TransformBy(matrix));
                        ds[i] = 0;
                    }
                    int loopNumber = hatch.NumberOfLoops;
                    for (int i = 0; i < loopNumber; i++)
                    {
                        hatch.RemoveLoopAt(0);
                    }
                    hatch.AppendLoop((HatchLoopTypes.External | HatchLoopTypes.Polyline | HatchLoopTypes.Derived), new Point2dCollection(ps), new DoubleCollection(ds));
                    outs.Add(hatch);
                }
            }
            else
            {
                if (isClosePath || entity is Hatch)
                {
                    if (points.Count >= 3)
                    {
                        Polyline pl = new Polyline(points.Count);
                        for (int i = 0; i < points.Count; i++)
                        {
                            pl.AddVertexAt(i, to2d(clone3d(points[i]).TransformBy(matrix)), 0, 0, 0);
                        }
                        pl.Closed = true;
                        pl.Color = entity.Color;
                        pl.LayerId = entity.LayerId;
                        outs.Add(pl);
                    }
                }
                else
                {
                    if (points.Count == 2)
                    {
                        Point3d start = clone3d(points[0]).TransformBy(matrix);
                        Point3d end = clone3d(points[1]).TransformBy(matrix);
                        Line line = new Line(start, end);
                        outs.Add(line);
                        line.Color = entity.Color;
                        line.LayerId = entity.LayerId;
                    }
                    else if (points.Count >= 3)
                    {
                        Polyline pl = new Polyline(points.Count);
                        for (int i = 0; i < points.Count; i++)
                        {
                            pl.AddVertexAt(i, to2d(clone3d(points[i]).TransformBy(matrix)), 0, 0, 0);
                        }
                        pl.Closed = false;
                        pl.Color = entity.Color;
                        pl.LayerId = entity.LayerId;
                        outs.Add(pl);
                    }
                }
            }
            return outs;
        }

        public static Tuple<bool, List<Point3d>> GetPoints(Entity entity, double segmentDist = 3, int minSegmentCount = 5, int maxSegmentCount = 100)
        {
            List<Point3d> points = new List<Point3d>();
            bool isClosePath = false;
            if (entity is Curve)
            {
                Curve curve = entity as Curve;
                if (curve is Arc)
                {
                    Arc arc = curve as Arc;
                    double length = arc.Length;
                    double segmentCount = Math.Min(Math.Max(Math.Ceiling(length / segmentDist), minSegmentCount), maxSegmentCount);
                    double distance = length / segmentCount;
                    for (int i = 0; i <= segmentCount; i++)
                    {
                        Point3d p = arc.GetPointAtDist(i * distance);
                        points.Add(p);
                    }
                }
                else if (curve is Line)
                {
                    points.Add(curve.StartPoint);
                    points.Add(curve.EndPoint);
                }
                else if (curve is Circle)
                {
                    Circle arc = curve as Circle;
                    double length = arc.Radius * 2 * Math.PI;
                    double segmentCount = Math.Min(Math.Max(Math.Ceiling(length / segmentDist), minSegmentCount), maxSegmentCount);
                    double distance = length / segmentCount;
                    for (int i = 0; i < segmentCount; i++)
                    {

                        Point3d p = arc.GetPointAtDist(Math.Min(i * distance, length));
                        points.Add(p);
                    }
                    isClosePath = true;
                }
                else if (curve is Polyline)
                {
                    Polyline polyline = curve as Polyline;
                    int verticeNum = polyline.NumberOfVertices;
                    for (int i = 0; i < verticeNum; i++)
                    {
                        Point3d curPoint = polyline.GetPoint3dAt(i);
                        Point3d nextPoint = polyline.GetPoint3dAt((i + 1) % verticeNum);

                        List<Point3d> centerPoints = new List<Point3d>();
                        double bulge = polyline.HasBulges ? polyline.GetBulgeAt(i) : 0;
                        if (bulge != 0)
                        {
                            if (i == verticeNum - 1 && !polyline.Closed)
                            {

                            }
                            else
                            {
                                SvgArc svgArc = new SvgArc(to2d(curPoint), to2d(nextPoint), bulge);
                                double startAngle = (svgArc.start - svgArc.originCenter).Angle;
                                double endAngle = (svgArc.end - svgArc.originCenter).Angle;
                                if (svgArc.isCW)
                                {
                                    double tempAngle = startAngle;
                                    startAngle = endAngle;
                                    endAngle = tempAngle;
                                }
                                Arc arc = new Arc(to3d(svgArc.originCenter), svgArc.radius, startAngle, endAngle);
                                double length = arc.Length;
                                double segmentCount = Math.Min(Math.Max(Math.Ceiling(length / segmentDist), minSegmentCount), maxSegmentCount);
                                double distance = length / segmentCount;
                                List<Point3d> midPoints = new List<Point3d>();
                                for (int sIndex = 1; sIndex < segmentCount; sIndex++)
                                {
                                    Point3d p = arc.GetPointAtDist(Math.Min(sIndex * distance, length));
                                    midPoints.Add(p);
                                }
                                if(midPoints[0].DistanceTo(curPoint)> midPoints[0].DistanceTo(nextPoint))
                                {
                                    midPoints.Reverse();
                                }
                                centerPoints.AddRange(midPoints);
                            }
                        }

                        points.Add(curPoint);
                        centerPoints.ForEach((Point3d p) =>
                        {
                            points.Add(p);
                        });
                    }
                    isClosePath = polyline.Closed;
                }
                else if (curve is Spline)
                {
                    Spline arc = curve as Spline;
                    double length = arc.EndParam - arc.StartParam;
                    double segmentCount = Math.Min(Math.Max(Math.Ceiling(length / segmentDist), minSegmentCount), maxSegmentCount);
                    double distance = length / segmentCount;
                    for (int i = 0; i <= segmentCount; i++)
                    {
                        Point3d p = arc.GetPointAtParameter(Math.Min(arc.StartParam + i * distance, arc.EndParam - 1e-3));
                        if (arc.Closed && i == segmentCount)
                        {

                        }
                        else
                        {
                            points.Add(p);
                        }
                    }
                    isClosePath = arc.Closed;
                }
                else if (curve is Ellipse)
                {
                    Ellipse arc = curve as Ellipse;
                    double length = arc.EndParam - arc.StartParam;
                    double segmentCount = Math.Min(Math.Max(Math.Ceiling(length / segmentDist), minSegmentCount), maxSegmentCount);
                    double distance = length / segmentCount;
                    for (int i = 0; i <= segmentCount; i++)
                    {
                        Point3d p = arc.GetPointAtParameter(Math.Min(arc.StartParam + i * distance, arc.EndParam - 1e-3));
                        if (arc.Closed && i == segmentCount)
                        {

                        }
                        else
                        {
                            points.Add(p);
                        }
                    }
                    isClosePath = arc.Closed;
                }
                else if (curve is Polyline2d)
                {
                    Polyline2d arc = curve as Polyline2d;
                    double length = arc.Length;
                    double segmentCount = Math.Min(Math.Max(Math.Ceiling(length / segmentDist), minSegmentCount), maxSegmentCount);
                    double distance = length / segmentCount;
                    for (int i = 0; i <= segmentCount; i++)
                    {
                        Point3d p = arc.GetPointAtDist(Math.Min(i * distance, length - 1e-3));
                        if (arc.Closed && i == segmentCount)
                        {

                        }
                        else
                        {
                            points.Add(p);
                        }
                    }
                    isClosePath = arc.Closed;
                }
            }
            else if (entity is Hatch)
            {
                Hatch hatch = entity as Hatch;
                int loopNums = hatch.NumberOfLoops;
                for (int loopIndex = 0; loopIndex < loopNums; loopIndex++)
                {
                    HatchLoop loop = hatch.GetLoopAt(loopIndex);
                    if (loop.LoopType == (HatchLoopTypes.External | HatchLoopTypes.Polyline | HatchLoopTypes.Derived) && loop.IsPolyline)
                    {
                        BulgeVertexCollection bulgeVertexs = loop.Polyline;
                        int verticeNum = bulgeVertexs.Count;
                        for (int i = 0; i < verticeNum; i++)
                        {
                            BulgeVertex bulgeVertex = bulgeVertexs[i];
                            BulgeVertex nextBulgeVertex = bulgeVertexs[(i + 1) % verticeNum];
                            if (isPoint2dEqual(bulgeVertex.Vertex, nextBulgeVertex.Vertex))
                            {
                                continue;
                            }

                            List<Point3d> centerPoints = new List<Point3d>();
                            double bulge = bulgeVertex.Bulge;
                            if (bulge != 0)
                            {
                                SvgArc svgArc = new SvgArc(bulgeVertex.Vertex, nextBulgeVertex.Vertex, bulge);
                                double startAngle = (svgArc.start - svgArc.originCenter).Angle;
                                double endAngle = (svgArc.end - svgArc.originCenter).Angle;
                                if (svgArc.isCW)
                                {
                                    double tempAngle = startAngle;
                                    startAngle = endAngle;
                                    endAngle = tempAngle;
                                }
                                Arc arc = new Arc(to3d(svgArc.originCenter), svgArc.radius, startAngle, endAngle);
                                double length = arc.Length;
                                double segmentCount = Math.Min(Math.Max(Math.Ceiling(length / segmentDist), minSegmentCount), maxSegmentCount);
                                double distance = length / segmentCount;
                                List<Point3d> midPoints = new List<Point3d>();
                                for (int sIndex = 1; sIndex < segmentCount; sIndex++)
                                {
                                    Point3d p = arc.GetPointAtDist(Math.Min(sIndex * distance, length));
                                    midPoints.Add(p);
                                }
                                if (midPoints[0].DistanceTo(to3d(bulgeVertex.Vertex)) > midPoints[0].DistanceTo(to3d(nextBulgeVertex.Vertex)))
                                {
                                    midPoints.Reverse();
                                }
                                centerPoints.AddRange(midPoints);
                            }

                            points.Add(to3d(bulgeVertex.Vertex));
                            centerPoints.ForEach((Point3d p) =>
                            {
                                points.Add(p);
                            });
                        }
                        break;
                    }
                }


                isClosePath = true;
            }
            return new Tuple<bool, List<Point3d>>(isClosePath, points);
        }

        public static Point2d to2d(Point3d p)
        {
            return new Point2d(p.X, p.Y);
        }

        public static Point3d to3d(Point2d p)
        {
            return new Point3d(p.X, p.Y, 0);
        }

        public static Point3d clone3d(Point3d p)
        {
            return new Point3d(p.X, p.Y, p.Z);
        }

        public static Point2d clone2d(Point2d p)
        {
            return new Point2d(p.X, p.Y);
        }

        public static bool isPoint2dEqual(Point2d p1, Point2d p2, double e = 1e-4)
        {
            if (Math.Abs(p1.X - p2.X) <= e && Math.Abs(p1.Y - p2.Y) <= e)
            {
                return true;
            }
            return false;
        }

        public static bool isPoint3dEqual(Point3d p1, Point3d p2, double e = 1e-4)
        {
            if (Math.Abs(p1.X - p2.X) <= e && Math.Abs(p1.Y - p2.Y) <= e && Math.Abs(p1.Z - p2.Z) <= e)
            {
                return true;
            }
            return false;
        }

        public static Extents3d GetExtents(List<Entity> entities, double expandValue = 0)
        {
            Extents3d extents = new Extents3d();
            foreach (Entity entity in entities)
            {
                Extents3d? ext = entity.Bounds;
                if (ext.HasValue)
                {
                    extents.AddExtents(ext.Value);
                }
            }
            if (expandValue > 0)
            {
                extents.AddPoint(new Point3d(extents.MinPoint.X - expandValue, extents.MinPoint.Y - expandValue, 0));
                extents.AddPoint(new Point3d(extents.MaxPoint.X + expandValue, extents.MaxPoint.Y + expandValue, 0));
            }
            return extents;
        }
    }
}
