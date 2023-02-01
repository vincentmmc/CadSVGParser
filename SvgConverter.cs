using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;
using SVGParser.SvgParser.core;
using SVGParser.SvgParser;
using SVGParser.Utils;
using System.Drawing;

namespace SVGParser
{
    public class SvgConverter
    {
        private static double DEFAULT_LINE_WIDTH = 1;
        private List<Entity> _entities;
        private Dictionary<ObjectId, LayerInfo> _layerInfoDict = new Dictionary<ObjectId, LayerInfo>();

        public SvgConverter(List<Entity> curves, Dictionary<ObjectId, LayerInfo> _layerInfoDict)
        {
            this._entities = curves;
            this._layerInfoDict = _layerInfoDict;
        }

        public string Parse()
        {
            List<SvgParserBase> parsers = new List<SvgParserBase>();
            foreach (Entity entity in this._entities)
            {
                SvgParserBase parser = this.GetParser(entity, this._layerInfoDict);
                if (parser == null)
                {
                    continue;
                }
                parsers.Add(parser);
            }
            parsers.Sort();

            Extents3d extents = this.GetExtents(parsers, DEFAULT_LINE_WIDTH * 0.5);
            double sizeX = extents.MaxPoint.X - extents.MinPoint.X;
            double sizeY = extents.MaxPoint.Y - extents.MinPoint.Y;
            string viewBoxStr = extents.MinPoint.X.ToString() + " " + extents.MinPoint.Y.ToString() + " " + sizeX + " " + sizeY;
            string paths = this.GetPaths(parsers, DEFAULT_LINE_WIDTH);
            string svgStr = string.Format("<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{0}\" height=\"{1}\" viewBox=\"{2}\">{3}</svg>", sizeX.ToString(), sizeY.ToString(), viewBoxStr, paths);
            return svgStr;
        }

        public Bitmap ParseToBitmap()
        {
            List<SvgParserBase> parsers = new List<SvgParserBase>();
            foreach (Entity entity in this._entities)
            {
                SvgParserBase parser = this.GetParser(entity, this._layerInfoDict);
                if (parser == null)
                {
                    continue;
                }
                parsers.Add(parser);
            }
            parsers.Sort();

            // image draw datas
            List<ImageDrawData> drawDatas = new List<ImageDrawData>();
            foreach (SvgParserBase p in parsers)
            {
                List<Point2d> points = new List<Point2d>();
                bool isClosePath = false;
                System.Drawing.Color color = System.Drawing.Color.White;
                Tuple<bool, List<Point3d>> pointResult = EntityUtils.GetPoints(p.entity, 1, 5, 1000);
                List<Point3d> point3ds = pointResult.Item2;

                if(point3ds.Count==0)
                {
                    continue;
                }

                if (p is SvgHatchParser)
                {
                    point3ds.ForEach(pp =>
                    {
                        points.Add(EntityUtils.to2d(pp));
                    });
                    isClosePath = true;
                    color = System.Drawing.Color.White;
                }
                else
                {
                    point3ds.ForEach(pp =>
                    {
                        points.Add(EntityUtils.to2d(pp));
                    });
                    if (pointResult.Item1)
                    {
                        points.Add(EntityUtils.to2d(point3ds[0]));
                    }
                    color = System.Drawing.Color.Black;
                }


                ImageDrawData drawData = new ImageDrawData();
                drawData.color = color;
                drawData.point2ds = points;
                drawData.isClosePath = isClosePath;
                drawDatas.Add(drawData);
            }

            // size and offset
            Extents3d extents = this.GetExtents(parsers, DEFAULT_LINE_WIDTH * 0.5);
            double sizeX = extents.MaxPoint.X - extents.MinPoint.X;
            double sizeY = extents.MaxPoint.Y - extents.MinPoint.Y;
            Vector2d offset = new Vector2d(-extents.MinPoint.X, -extents.MinPoint.Y);

            Bitmap bitmap = ImageUtils.Draw(2f,512f, sizeX, sizeY, offset, drawDatas);
            return bitmap;
        }


        private SvgParserBase GetParser(Entity entity, Dictionary<ObjectId, LayerInfo> layerInfoDict)
        {
            if (entity is Arc)
            {
                return new SvgArcParser(entity as Arc, layerInfoDict);
            }
            else if (entity is Line)
            {
                return new SvgLineParser(entity as Line, layerInfoDict);
            }
            else if (entity is Circle)
            {
                return new SvgCircleParser(entity as Circle, layerInfoDict);
            }
            else if (entity is Ellipse)
            {
                return new SvgEllipseParser(entity as Ellipse, layerInfoDict);
            }
            else if (entity is Polyline)
            {
                return new SvgPolylineParser(entity as Polyline, layerInfoDict);
            }
            else if (entity is Polyline2d)
            {
                return new SvgPolyLine2dParser(entity as Polyline2d, layerInfoDict);
            }
            else if (entity is Spline)
            {
                return new SvgSplineParser(entity as Spline, layerInfoDict);
            }
            else if (entity is Hatch)
            {
                return new SvgHatchParser(entity as Hatch, layerInfoDict);
            }
            return null;
        }

        private string GetPaths(List<SvgParserBase> parsers, double lineWidth)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\n");
            foreach (SvgParserBase svgParser in parsers)
            {
                sb.Append(svgParser.GetPathXml(lineWidth) + "\n");
            }
            return sb.ToString();
        }

        private Extents3d GetExtents(List<SvgParserBase> parsers, double expandValue = 0)
        {
            Extents3d extents = new Extents3d();
            foreach (SvgParserBase svgParser in parsers)
            {
                Extents3d? ext = svgParser.GetBounds();
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
