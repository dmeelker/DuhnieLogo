﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.UI.Model
{
    class Point
    {
        public Point() { }
        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; set; }
        public float Y { get; set; }

        public Point Clone()
        {
            return new Point(X, Y);
        }
    }
}
