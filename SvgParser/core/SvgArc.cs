using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;

namespace CADParser
{
    class SvgArc
    {
        #region members

        private Point2d _start;
        private Point2d _end;
        private Point2d _originCenter;
        private bool _isCW;
        private bool _isLargeArc;
        private double _radius;

        public bool isCW
        {

            get
            {
                return this._isCW;
            }
        }

        public double radius
        {

            get
            {
                return this._radius;
            }
        }

        public bool isLargeArc
        {

            get
            {
                return this._isLargeArc;
            }
        }

        public Point2d start
        {

            get
            {
                return this._start;
            }
        }

        public Point2d end
        {
            get
            {
                return this._end;
            }
        }

        public Point2d originCenter
        {
            get
            {
                return this._originCenter;
            }
        }

        #endregion

        public SvgArc(Point2d start, Point2d end, double bulge)
        {
            this._start = start;
            this._end = end;
            this._originCenter = this.GetArcOriginCenter(this._start, this._end, bulge);
            this._isCW = bulge < 0;
            this._isLargeArc = Math.Abs(bulge) > 1;
            this._radius = this._originCenter.GetDistanceTo(start);
        }

        public SvgArc(Arc arc)
        {
            this._start = new Point2d(arc.StartPoint.X, arc.StartPoint.Y);
            this._end = new Point2d(arc.EndPoint.X, arc.EndPoint.Y);
            this._originCenter = new Point2d(arc.Center.X, arc.Center.Y);
            // arc is ccw
            this._isCW = false;
            this._isLargeArc = arc.TotalAngle > Math.PI;
            this._radius = this._originCenter.GetDistanceTo(start);
        }

        #region private functions

        private bool CalcCW(Point2d curPoint, Point2d nextPoint, Point2d arcCenter)
        {
            Vector2d next2cur = (nextPoint - curPoint);
            Vector2d arcCenter2cur = (arcCenter - curPoint);
            double cross = (next2cur.X * arcCenter2cur.Y) - (next2cur.Y * arcCenter2cur.X);
            return cross > 0;
        }

        private bool CalcLargeArcFlag(Point2d curPoint, Point2d nextPoint, Point2d originCenter, Point2d arcCenter)
        {
            Vector2d s2o = (originCenter - curPoint).GetNormal();
            Vector2d e2s = (nextPoint - curPoint).GetNormal();
            Vector2d c2s = (arcCenter - curPoint).GetNormal();
            double cross1 = (s2o.X * e2s.Y) - (s2o.Y * e2s.X);
            double cross2 = (s2o.X * c2s.Y) - (s2o.Y * c2s.X);
            if (cross1 * cross2 > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        private Point2d GetArcOriginCenter(Point2d start, Point2d end, double bulge)
        {
            double b = 0.5 * (1 / bulge - bulge);
            double x1 = start.X;
            double y1 = start.Y;
            double x2 = end.X;
            double y2 = end.Y;
            double cx = 0.5 * ((x1 + x2) - b * (y2 - y1));
            double cy = 0.5 * ((y1 + y2) + b * (x2 - x1));
            Point2d center = new Point2d(cx, cy);
            return center;
        }

        private Point2d GetArcCenter(Point2d curPoint, Point2d nextPoint, Point2d center)
        {
            double radius = curPoint.GetDistanceTo(center);
            Point2d xCenter = curPoint + (nextPoint - curPoint).GetNormal().MultiplyBy(nextPoint.GetDistanceTo(curPoint) * 0.5);
            Point2d arcCenter = center + (xCenter - center).GetNormal().MultiplyBy(radius);
            return arcCenter;
        }

        #endregion
    }
}
