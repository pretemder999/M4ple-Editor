﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NE4S.Notes
{
    [Serializable()]
    public class AirUpL : AirUpC
    {
        public override int NoteID => 3;

        public AirUpL() { }

        public AirUpL(Note note) : base(note) { }

        public AirUpL(int size, Position pos, PointF location, int laneIndex) : base(size, pos, location, laneIndex) { }

        public override bool Contains(PointF location)
        {
            return GetAirPath(new Point()).IsVisible(location);
        }

        protected override GraphicsPath GetAirPath(Point drawLocation)
        {
            GraphicsPath graphicsPath = base.GetAirPath(drawLocation);
            PointF pathLocation = graphicsPath.GetBounds().Location;
            var mat = new Matrix();
            mat.Translate(-pathLocation.X, -pathLocation.Y, MatrixOrder.Append);
            mat.Shear(shearX, 0, MatrixOrder.Append);
            mat.Translate(pathLocation.X - shearX * (airHeight - borderWidth * 2), pathLocation.Y, MatrixOrder.Append);
            graphicsPath.Transform(mat);
            return graphicsPath;
        }

        public override void Draw(Graphics g, Point drawLocation)
        {
            using (GraphicsPath graphicsPath = GetAirPath(drawLocation))
            using (SolidBrush brush = new SolidBrush(airUpColor))
            using (Pen pen = new Pen(Color.White, borderWidth))
            {
                g.DrawPath(pen, graphicsPath);
                g.FillPath(brush, graphicsPath);
            }
        }
    }
}
