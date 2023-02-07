using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using SVGParser.SvgParser.core;
using SVGParser.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVGParser
{
    class SvgHatchParser : SvgParserBase
    {
        private Hatch _hatch;
        public SvgHatchParser(Entity entity, Dictionary<ObjectId, LayerInfo> layerInfo) : base(entity, layerInfo)
        {
            this._hatch = this._entity as Hatch;
            this._svgFormat = "<path d=\"{0}\" fill=\"{1}\" stroke=\"none\" stroke-width=\"{2}\" name=\"{3}\"/>";
        }

        public override string GetPathXml(double lineWidth)
        {
            if (!this._hatch.IsSolidFill)
            {
                return "";
            }
            string colorStr = this.Color;
            StringBuilder sb = new StringBuilder();
            List<string> paths = GetSvgPath();
            for (int i = 0; i < paths.Count; i++)
            {
                if (i != 0)
                {
                    sb.Append("\n");
                }
                string path = string.Format(this._svgFormat, paths[i], colorStr, lineWidth, "Hatch" + i.ToString());
                sb.Append(path);
            }
            return sb.ToString();
        }

        private List<string> GetSvgPath()
        {
            List<string> paths = new List<string>();
            int loopNums = this._hatch.NumberOfLoops;
            for (int loopIndex = 0; loopIndex < loopNums; loopIndex++)
            {
                HatchLoop loop = this._hatch.GetLoopAt(loopIndex);
                if (loop.LoopType == (HatchLoopTypes.External | HatchLoopTypes.Polyline | HatchLoopTypes.Derived) && loop.IsPolyline)
                {
                    string str = "";
                    BulgeVertexCollection bulgeVertexs = loop.Polyline;
                    int verticeNum = bulgeVertexs.Count;
                    for (int i = 0; i < verticeNum; i++)
                    {
                        BulgeVertex bulgeVertex = bulgeVertexs[i];
                        BulgeVertex nextBulgeVertex = bulgeVertexs[(i + 1) % verticeNum];
                        Point2d curPoint = bulgeVertex.Vertex;
                        Point2d nextPoint = nextBulgeVertex.Vertex;

                        if (EntityUtils.isPoint2dEqual(curPoint, nextPoint))
                        {
                            continue;
                        }

                        if (i == 0)
                        {
                            str += "M" + curPoint.X.ToString() + " " + curPoint.Y.ToString();
                        }
                        double bulge = bulgeVertex.Bulge;
                        if (bulge == 0)
                        {
                            if (i != verticeNum - 1)
                            {
                                str += "L" + nextPoint.X.ToString() + " " + nextPoint.Y.ToString();
                            }
                        }
                        else
                        {
                            SvgArc svgArc = new SvgArc(curPoint, nextPoint, bulge);
                            str += string.Format("A {0} {1} {2} {3} {4} {5} {6}", svgArc.radius, svgArc.radius, 0, (svgArc.isLargeArc) ? 1 : 0, svgArc.isCW ? 0 : 1, svgArc.end.X, svgArc.end.Y);
                        }
                    }
                    str += "Z";
                    paths.Add(str);
                }
            }
            return paths;
        }
    }
}
