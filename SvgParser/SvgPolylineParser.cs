using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using CADParser.SvgParser.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADParser
{
    class SvgPolylineParser : SvgParserBase
    {
        private Polyline _line;
        public SvgPolylineParser(Entity entity, Dictionary<ObjectId, LayerInfo> layerInfo) : base(entity, layerInfo)
        {
            this._line = this._entity as Polyline;
            this._svgFormat = "<path d=\"{0}\" fill=\"none\" stroke=\"{1}\" stroke-width=\"{2}\" name=\"{3}\"/>";
        }

        public override string GetPathXml(double lineWidth)
        {
            string colorStr = this.Color;
            string path = string.Format(this._svgFormat, this.GetSvgPath(), colorStr, lineWidth,"Polyline");
            return path;
        }

        private string GetSvgPath()
        {
            string str = "";
            int verticeNum = this._line.NumberOfVertices;
            for (int i = 0; i < verticeNum; i++)
            {
                Point2d curPoint = this._line.GetPoint2dAt(i);
                Point2d nextPoint = this._line.GetPoint2dAt((i + 1) % verticeNum);
                if (i == 0)
                {
                    str += "M" + curPoint.X.ToString() + " " + curPoint.Y.ToString();
                }
                if (i == verticeNum - 1 && !this._line.Closed)
                {
                    continue;
                }
                double bulge = this._line.HasBulges ? this._line.GetBulgeAt(i) : 0;
                if (bulge == 0)
                {
                    str += "L" + nextPoint.X.ToString() + " " + nextPoint.Y.ToString();
                }
                else
                {
                    SvgArc svgArc = new SvgArc(curPoint, nextPoint, bulge);
                    str += string.Format("A {0} {1} {2} {3} {4} {5} {6}", svgArc.radius, svgArc.radius, 0, (svgArc.isLargeArc) ? 1 : 0, svgArc.isCW ? 0 : 1, svgArc.end.X, svgArc.end.Y);  // 逆时针圆弧，顺时针画}
                }
                if (i == verticeNum - 1 && this._line.Closed)
                {
                    str += "Z";
                }
            }
            return str;
        }
    }
}
