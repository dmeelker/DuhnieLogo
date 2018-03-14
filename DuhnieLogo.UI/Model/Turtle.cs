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

        public Bitmap BufferBitmap { get; set; }
        public Graphics BufferContext { get; set; }

        public Image Image { get; set; }
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
            var oldLocation = Location.Clone();

            if(PenDown)
                BufferContext.DrawLine(pen, Location.X, Location.Y, x, y);

            Location.X = x;
            Location.Y = y;

            var updateRectangle = new Rectangle()
            {
                X = (int)Math.Ceiling(Math.Min(oldLocation.X, Location.X)) - 1,
                Y = (int)Math.Ceiling(Math.Min(oldLocation.Y, Location.Y)) - 1
            };

            updateRectangle.Width = 2 + (int)Math.Ceiling(Math.Max(oldLocation.X, Location.X)) - updateRectangle.X;
            updateRectangle.Height = 2 + (int)Math.Ceiling(Math.Max(oldLocation.Y, Location.Y)) - updateRectangle.Y;

            // Copy the modified bit of the drawing buffer
            GraphicsContext.DrawImage(BufferBitmap, updateRectangle, updateRectangle, GraphicsUnit.Pixel);

            // Replace the area where the turtle image was
            GraphicsContext.DrawImage(BufferBitmap,
                new Rectangle((int)oldLocation.X, (int)oldLocation.Y, Image.Width, Image.Height),
                new Rectangle((int)oldLocation.X, (int)oldLocation.Y, Image.Width, Image.Height),
                GraphicsUnit.Pixel);

            // Draw the turtle in the new location
            GraphicsContext.DrawImage(Image, (int) Location.X, (int) Location.Y);
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

