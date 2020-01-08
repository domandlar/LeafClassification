using System;
using System.Collections.Generic;
using System.Text;

namespace ImageProcessing
{
    public class LeafToken
    {
        private int _x1;         // start X-coordinate of the line
        private int _y1;         // start Y-coordinate of the line
        private int _x2;       // end   X-coordinate of the line
        private int _y2;       // end   Y-coordinate of the line
        private double _cos;   // the cosinus angle of this line
        private double _sin;   // the sinus angle of this line


        public int X1 
        {
            get
            {
                return _x1;
            }
            set
            {
                _x1 = value;
            } 
        }
        public int Y1 
        {
            get
            {
                return _y1;
            }
            set
            {
                _y1 = value;
            }
        }
        public int X2 
        {
            get
            {
                return _x2;
            }
            set
            {
                _x2 = value;
            }
        }
        public int Y2 
        {
            get { return _y2; }
            set { _y2 = value; } 
        }
        public double Cos 
        {
            get { return _cos; }
        }
        public double Sin 
        {
            get { return _sin; } 
        }
        public LeafToken(int x1, int y1, int x2, int y2)
        {
            this._x1 = x1;
            this._y1 = y1;
            this._x2 = x2;
            this._y2 = y2;

            CalcCosinus();
            CalcSinus();
        }

        private void CalcCosinus()
        {
            int ax, ay;
            double hyp;

            ax = _x2 - _x1;
            ay = _y2 - _y1;

            hyp = Math.Sqrt(ax * ax + ay * ay);

            if (hyp == 0.0) _cos = 0.0;
            else _cos = ay / hyp;
        }

        private void CalcSinus()
        {
            int ax, ay;
            double hyp;

            ax = _x2 - _x1;
            ay = _y2 - _y1;

            hyp = Math.Sqrt(ax * ax + ay * ay);

            if (hyp == 0.0) hyp = Math.Abs(ax);

            _sin = ax / hyp;
        }
    }
}
