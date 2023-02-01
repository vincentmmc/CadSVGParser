using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using SVGParser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public struct ImageDrawData
{
    public List<Point2d> point2ds;
    public Color color;
    public float lineWidth;
    public bool isClosePath;
}

namespace SVGParser.Utils
{
    class ImageUtils
    {
        public static Bitmap Draw(float lineWidth, float minWidth, double width, double height, Vector2d offset, List<ImageDrawData> drawDatas)
        {
            if (width <= 1e-3 || height <= 1e-3)
            {
                return null;
            }

            double scale = 1.0;
            if (width < minWidth)
            {
                scale = minWidth / width;
            }
            double dHeight = height * scale;
            int imgWidth = (int)Math.Ceiling(width * scale);
            int imgHeight = (int)Math.Ceiling(height * scale);
            Bitmap bitmap = new Bitmap(imgWidth, imgHeight);
            Graphics g = Graphics.FromImage(bitmap);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //g.FillRectangle(new SolidBrush(Color.White), 0, 0, imgWidth, imgHeight);

            Pen pen = new Pen(new SolidBrush(Color.Black), lineWidth);
            pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Round;
            foreach (ImageDrawData drawData in drawDatas)
            {
                List<Point2d> points = drawData.point2ds;
                if (drawData.isClosePath)
                {
                    PointF[] dPoints = new PointF[points.Count];
                    for (int i = 0; i < dPoints.Length; i++)
                    {
                        Point2d p1 = points[i] + (offset);
                        dPoints[i] = new PointF((float)(p1.X * scale), (float)(dHeight - p1.Y * scale));
                    }
                    g.FillPolygon(new SolidBrush(drawData.color), dPoints);
                }
                else
                {
                    (pen.Brush as SolidBrush).Color = drawData.color;
                    for (int i = 0; i < points.Count - 1; i++)
                    {
                        Point2d p1 = points[i] + (offset);
                        Point2d p2 = points[i + 1] + (offset);
                        g.DrawLine(pen, (float)(p1.X * scale), (float)(dHeight - p1.Y * scale), (float)(p2.X * scale), (float)(dHeight - p2.Y * scale));
                    }
                }
            }
            g.Dispose();
            return bitmap;
        }
    }
}
