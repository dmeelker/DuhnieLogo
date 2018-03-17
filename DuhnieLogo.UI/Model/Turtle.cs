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
        public Turtle(string name)
        {
            Name = name;
            RecreatePen();
        }

        public Graphics GraphicsContext { get; set; }

        public string Name { get; set; }
        private Image image;
        public Image Image { get { return image; } set { image = value; ImageWidth = image.Width; ImageHeight = image.Height; } }
        public int ImageWidth { get; private set; }
        public int ImageHeight { get; private set; }
        public Point Location { get; set; }
        public int Orientation { get; set; }
        public bool PenDown { get; set; } = true;
        public bool Visible { get; set; } = true;
        public bool VariantModus { get; set; } = false;

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
            var oldLocation = Location.Clone();

            if(PenDown)
                GraphicsContext.DrawLine(pen, Location.X, Location.Y, x, y);

            Location.X = x;
            Location.Y = y;

            var updateRectangle = new Rectangle()
            {
                X = (int)Math.Ceiling(Math.Min(oldLocation.X, Location.X)) - 1,
                Y = (int)Math.Ceiling(Math.Min(oldLocation.Y, Location.Y)) - 1
            };
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

        public void Print(string text)
        {
            GraphicsContext.TranslateTransform((int) Location.X, (int) Location.Y);
            GraphicsContext.RotateTransform(Orientation);

            GraphicsContext.DrawString(text, SystemFonts.DefaultFont, Brushes.Black, 0, 0);

            GraphicsContext.ResetTransform();
        }

        public Rectangle BoundingBox => new Rectangle((int) Location.X, (int) Location.Y, ImageWidth, ImageHeight);
    }
}

