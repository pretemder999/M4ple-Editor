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
    public class SlideBegin : Note
    {
        public SlideBegin()
        {

        }

        public SlideBegin(int size, Position pos, PointF location, int laneIndex) : base(size, pos, location)
        {
            LaneIndex = laneIndex;
        }

        public override void Draw(PaintEventArgs e, int originPosX, int originPosY)
        {
            RectangleF drawRect = new RectangleF(
                noteRect.X - originPosX + adjustNoteRect.X,
                noteRect.Y - originPosY + adjustNoteRect.Y,
                noteRect.Width,
                noteRect.Height);
            using (LinearGradientBrush gradientBrush = new LinearGradientBrush(new PointF(0, drawRect.Y), new PointF(0, drawRect.Y + drawRect.Height), Color.Blue, Color.DarkBlue))
            {
                e.Graphics.FillRectangle(gradientBrush, drawRect);
            }
            using (Pen pen = new Pen(Color.White, 1))
            {
                e.Graphics.DrawLine(pen, new PointF(drawRect.X + 4, drawRect.Y + 2), new PointF(drawRect.X + drawRect.Width - 4, drawRect.Y + 2));
                //e.Graphics.DrawRectangles(pen, new RectangleF[]{ drawRect });
            }
        }
    }
}
