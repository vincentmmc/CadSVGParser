using Autodesk.AutoCAD.DatabaseServices;
using CADParser.SvgParser.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CADParser
{
    class SvgCircleParser : SvgParserBase
    {
        private Circle _circle;
        public SvgCircleParser(Entity entity, Dictionary<ObjectId, LayerInfo> layerInfo) : base(entity, layerInfo)
        {
            this._circle = this._entity as Circle;
            this._svgFormat = "<circle cx=\"{0}\" cy=\"{1}\" r=\"{2}\" fill=\"none\" stroke=\"{3}\" stroke-width=\"{4}\" name=\"{5}\"/>";
        }

        public override string GetPathXml(double lineWidth)
        {
            string path = string.Format(this._svgFormat, this._circle.Center.X.ToString(), this._circle.Center.Y.ToString(), this._circle.Radius.ToString(), this.Color, lineWidth,"Circle");
            return path;
        }
    }
}
