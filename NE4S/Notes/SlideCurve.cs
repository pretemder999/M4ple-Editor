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
    public class SlideCurve : Note
    {
        public SlideCurve()
        {

        }

        public SlideCurve(int size, Position pos, PointF location, int laneIndex) : base(size, pos, location)
        {
            LaneIndex = laneIndex;
        }

        public override void Draw(PaintEventArgs e, int originPosX, int originPosY)
        {
            RectangleF drawRect = new RectangleF(
                hitRect.X - originPosX,
                hitRect.Y - originPosY,
                hitRect.Width,
                hitRect.Height);
            using (LinearGradientBrush gradientBrush = new LinearGradientBrush(new PointF(0, drawRect.Y), new PointF(0, drawRect.Y + drawRect.Height), Color.White, Color.Gray))
            {
                e.Graphics.FillRectangle(gradientBrush, drawRect);
            }
            using (Pen pen = new Pen(Color.Blue, 1))
            {
                e.Graphics.DrawRectangles(pen, new RectangleF[]{ drawRect });
            }
        }
    }
}
