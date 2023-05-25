﻿using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using SVGParser.SvgParser.core;
using SVGParser.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVGParser.SvgParser
{
    class SvgSplineParser : SvgParserBase
    {
        private Spline _line;
        public SvgSplineParser(Entity entity, Dictionary<ObjectId, LayerInfo> layerInfo) : base(entity, layerInfo)
        {
            this._line = this._entity as Spline;
            this._svgFormat = "<path d=\"{0}\" fill=\"none\" stroke=\"{1}\" stroke-width=\"{2}\" name=\"{3}\"/>";
        }

        public override string GetPathXml(double lineWidth)
        {
            string colorStr = this.Color;
            string path = string.Format(this._svgFormat, this.GetSvgPath(), colorStr, lineWidth, "Spline");
            return path;
        }

        private string GetSvgPath()
        {
            string str = "";
            List<EntityPointsData> pointResult = EntityUtils.GetPoints(this._entity,3, this._pointsMinSegments, this._pointsMaxSegments);
            if (pointResult.Count == 0)
            {
                return str;
            }
            bool isClosePath = pointResult[0].isClosePath;
            List<Point3d> points = pointResult[0].points;
            for (int i = 0; i < points.Count; i++)
            {
                Point3d curPoint = points[i];
                if (i == 0)
                {
                    str += "M" + curPoint.X.ToString() + " " + curPoint.Y.ToString();
                }
                else
                {
                    str += "L" + curPoint.X.ToString() + " " + curPoint.Y.ToString();
                }

                if (i == points.Count - 1 && isClosePath)
                {
                    str += "Z";
                }
            }
            return str;
        }
    }
}
