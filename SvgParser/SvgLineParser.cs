using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using CADParser.SvgParser.core;

namespace CADParser
{
    public class SvgLineParser : SvgParserBase
    {
        private Line _line;
        public SvgLineParser(Entity entity, Dictionary<ObjectId, LayerInfo> layerInfo) : base(entity, layerInfo)
        {
            this._line = this._entity as Line;
            this._svgFormat = "<path d=\"{0}\" fill=\"none\" stroke=\"{1}\" stroke-width=\"{2}\" name=\"{3}\"/>";
        }

        public override string GetPathXml(double lineWidth)
        {
            string colorStr = this.Color;
            string path = string.Format(this._svgFormat, this.GetSvgPath(), colorStr, lineWidth,"Line");
            return path;
        }

        private string GetSvgPath()
        {
            return string.Format("M{0} {1}L{2} {3}", this._line.StartPoint.X, this._line.StartPoint.Y, this._line.EndPoint.X, this._line.EndPoint.Y);
        }
    }
}
