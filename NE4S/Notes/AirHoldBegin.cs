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
    public class AirHoldBegin : Note
    {
        public event NoteEventHandler CheckNoteSize;
        public event NoteEventHandlerEx CheckNotePosition;

        public AirHoldBegin()
        {

        }

        public AirHoldBegin(int size, Position pos, PointF location, int laneIndex) : base(size, pos, location, laneIndex)
        {
            
        }

        public override void ReSize(int size)
        {
            base.ReSize(size);
            CheckNoteSize?.Invoke(this);
            return;
        }

        public override void Relocate(Position pos, PointF location, int laneIndex)
        {
            int deltaTick = pos.Tick - Position.Tick;
            base.Relocate(pos);
            base.Relocate(location, laneIndex);
            CheckNotePosition?.Invoke(this, deltaTick);
            return;
        }

        public override void Relocate(Position pos)
        {
            int deltaTick = pos.Tick - Position.Tick;
            base.Relocate(pos);
            CheckNotePosition?.Invoke(this, deltaTick);
            return;
        }

        public override void Relocate(PointF location, int laneIndex)
        {
            base.Relocate(location, laneIndex);
            CheckNotePosition?.Invoke(this, 0);
            return;
        }

        public override void Draw(PaintEventArgs e, int originPosX, int originPosY)
        {
#if DEBUG
            RectangleF drawRect = new RectangleF(
                noteRect.X - originPosX + adjustNoteRect.X,
                noteRect.Y - originPosY + adjustNoteRect.Y,
                noteRect.Width,
                noteRect.Height);

            using (Pen pen = new Pen(Color.LightGray, 1))
            {
                e.Graphics.DrawPath(pen, drawRect.RoundedPath());
            }
#endif
            return;
        }
    }
}
