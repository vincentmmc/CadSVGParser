﻿using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using SVGParser.SvgParser.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SVGParser.Utils
{
    class ColorUtils
    {
        public static bool FLIP_WHITE_COLOR = false;

        public static string GetColorStr(Color color)
        {
            string colorValue = color.ColorValue.Name;
            if (colorValue.Length == 8)
            {
                // white reserve to black
                string str = colorValue.Substring(2, 6);
                if (str == "ffffff" && FLIP_WHITE_COLOR)
                {
                    str = "000000";
                }
                return "#" + str;
            }
            return "#000000";
        }

        public static string GetSvgColor(Entity entity, Dictionary<ObjectId, LayerInfo> layerDict)
        {
            if (entity.Color.IsByLayer)
            {
                if (layerDict.ContainsKey(entity.LayerId))
                {

                    return layerDict[entity.LayerId].Color;
                }
            }
            return GetColorStr(entity.Color);
        }
    }
}
