using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuhnieLogo.UI.Model
{
    class Turtle
    {
        public Turtle()
        {
            RecreatePen();
        }

        public Graphics GraphicsContext { get; set; }
        public Point Location { get; set; }
        public int Orientation { get; set; }
        public bool PenDown { get; set; } = true;

        private int penWidth = 1;
        public int PenWidth
        {
            get
            {
                return penWidth;
            }

            set
            {
                penWidth = value;
                RecreatePen();
            }
        }

        private Color penColor = Color.Black;
        public Color PenColor
        {
            get { return penColor; }
            set {
                penColor = value;
                RecreatePen();
            }
        }

        private Pen pen;

        private void RecreatePen()
        {
            if (pen != null)
                pen.Dispose();

            pen = new Pen(PenColor, PenWidth);
        }


        public void Forward(int steps)
        {
            Move(
                Location.X + (float) (Math.Sin(ToRadians(Orientation)) * steps),
                Location.Y - (float) (Math.Cos(ToRadians(Orientation)) * steps));
        }

        public void Move(float x, float y)
        {
            if(PenDown)
                GraphicsContext.DrawLine(pen, Location.X, Location.Y, x, y);

            Location.X = x;
            Location.Y = y;
        }

        public void Left(int steps)
        {
            Orientation -= steps;
        }

        public void Right(int steps)
        {
            Orientation += steps;
        }

        private double ToRadians(double angle)
        {
            return (Math.PI / 180) * angle;
        }
    }
}
