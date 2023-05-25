using Autodesk.AutoCAD.Colors;
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
    public class SvgParserBase : IComparable<SvgParserBase>
    {
        protected string _svgFormat;
        protected int _order;
        protected Entity _entity;
        protected Dictionary<ObjectId, LayerInfo> _layerInfo;
        protected int _pointsMinSegments = 5;
        protected int _pointsMaxSegments = 1000;


        public Entity entity {
            get
            {
                return this._entity;
            }
        }

        public int order {
            get
            {
                return this._order;
            }
        }

        public int pointsMinSegments
        {
            get
            {
                return this._pointsMinSegments;
            }
        }

        public int pointsMaxSegments
        {
            get
            {
                return this._pointsMaxSegments;
            }
        }

        public SvgParserBase(Entity entity, Dictionary<ObjectId, LayerInfo> layerInfo)
        {
            this._entity = entity;
            this._layerInfo = layerInfo;
            this._svgFormat = "";
            this._order = layerInfo.ContainsKey(entity.LayerId) ? layerInfo[entity.LayerId].Order : 0;
        }

        protected string Color
        {
            get
            {
                return ColorUtils.GetSvgColor(this._entity, this._layerInfo);
            }
        }

        public virtual string GetPathXml(double lineWidth)
        {
            return "SvgParseBase";
        }

        public int CompareTo(SvgParserBase other)
        {
            return this.order - other.order;
        }  
    }
}
