using Autodesk.AutoCAD.DatabaseServices;
using SVGParser.SvgParser.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVGParser
{
    class SvgArcParser : SvgParserBase
    {
        private Arc _arc;
        public SvgArcParser(Entity entity, Dictionary<ObjectId, LayerInfo> layerInfo) :base(entity, layerInfo)
        {
            this._arc = this._entity as Arc;
            this._svgFormat = "<path d=\"{0}\" fill=\"none\" stroke=\"{1}\" stroke-width=\"{2}\" name=\"{3}\"/>";
        }

        public override string GetPathXml(double lineWidth)
        {
            string path = string.Format(this._svgFormat, this.GetSvgPath(), this.Color, lineWidth,"Arc");
            return path;
        }

        private string GetSvgPath()
        {
            // A rx ry x-axis-rotation large-arc-flag sweep-flag x y
            SvgArc svgArc = new SvgArc(this._arc);
            return string.Format("M{0} {1}A {2} {3} {4} {5} {6} {7} {8}", svgArc.start.X, svgArc.start.Y, svgArc.radius, svgArc.radius, 0, (svgArc.isLargeArc) ? 1 : 0, svgArc.isCW ? 0 : 1, svgArc.end.X, svgArc.end.Y);
        }
    }
}
