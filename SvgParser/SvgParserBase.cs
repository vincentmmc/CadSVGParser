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


        public Entity entity {
            get
            {
                return this._entity;
            }
        }

        public SvgParserBase(Entity entity, Dictionary<ObjectId, LayerInfo> layerInfo)
        {
            double[] data =
            {
                1,0,0,0,
                0,-1,0,0,
                0,0,1,0,
                0,0,0,1
            };
            Matrix3d matrix3d = new Matrix3d(data);
            this._entity = entity;
            this._entity.TransformBy(matrix3d);
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

        public virtual Extents3d? GetBounds()
        {
            return this._entity.Bounds;
        }

        public int CompareTo(SvgParserBase other)
        {
            return this._order - other._order;
        }

        
    }
}
