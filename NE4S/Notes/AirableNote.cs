﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NE4S.Scores;

namespace NE4S.Notes
{
    [Serializable()]
    public class AirableNote : Note
    {
        public Air Air { get; private set; } = null;
        public AirHold AirHold { get; private set; } = null;
        private AirHoldBegin airHoldBegin = null;

        public AirableNote() { }

        public AirableNote(int size, Position pos, PointF location, int laneIndex) : base(size, pos, location, laneIndex) { }

        /// <summary>
        /// Airが取り付けられているか判定します
        /// 取り付けられている場合trueを返します
        /// </summary>
        public bool IsAirAttached
        {
            get
            {
                return Air != null;
            }
        }

        /// <summary>
        /// AirHoldが取り付けられているか判定します
        /// 取り付けられている場合trueを返します
        /// </summary>
        public bool IsAirHoldAttached
        {
            get
            {
                return AirHold != null;
            }
        }

        public void AttachAir(Air air)
        {
            Air = air;
            air.DetachAir += DetachAir;
            air.GetAirable += () => this;
        }

        public void AttachAirHold(AirHold airHold)
        {
            AirHold = airHold;
            airHold.DetachAirHold += DetachAirHold;
            airHoldBegin = airHold.AirHoldBegin;
            System.Diagnostics.Debug.Assert(airHoldBegin != null, "だめです");
            return;
        }

        public void DetachAir()
        {
            Air = null;
            return;
        }

        public void DetachAirHold()
        {
            AirHold = null;
            airHoldBegin = null;
            return;
        }

        public override void ReSize(int size)
        {
            base.ReSize(size);
            Air?.ReSize(size);
            airHoldBegin?.ReSize(size);
            return;
        }

        /// <summary>
        /// このノーツの位置変更とそれに付随するAir、AirHoldノーツの位置のみを変更します
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="location"></param>
        /// <param name="laneIndex"></param>
        public new void RelocateOnly(Position pos, PointF location, int laneIndex)
        {
            base.RelocateOnly(pos, location, laneIndex);
            Air?.RelocateOnly(pos, location, laneIndex);
            //AirHold全体に変更を反映させるためRelocateを使う
            airHoldBegin?.Relocate(pos, location, laneIndex);
        }

        public override void Relocate(Position pos, PointF location, int laneIndex)
        {
            Relocate(location, laneIndex);
            Relocate(pos);
        }

        public override void Relocate(PointF location, int laneIndex)
        {
            base.Relocate(location, laneIndex);
            Air?.Relocate(location, laneIndex);
            airHoldBegin?.Relocate(location, laneIndex);
            return;
        }

        public override void Relocate(Position pos)
        {
            base.Relocate(pos);
            Air?.Relocate(pos);
            airHoldBegin?.Relocate(pos);
            return;
        }

        public override void RelocateOnlyAndUpdate(Position position, LaneBook laneBook)
        {
            base.RelocateOnlyAndUpdate(position, laneBook);
            Air?.RelocateOnlyAndUpdate(position, laneBook);
            airHoldBegin?.Relocate(position);
            airHoldBegin?.UpdateLocation(laneBook);
        }

        public override void Draw(PaintEventArgs e, Point drawLocation)
        {
            RectangleF drawRect = new RectangleF(
                noteRect.X - drawLocation.X + adjustNoteRect.X,
                noteRect.Y - drawLocation.Y + adjustNoteRect.Y,
                noteRect.Width,
                noteRect.Height);
            using (LinearGradientBrush gradientBrush = new LinearGradientBrush(new PointF(0, drawRect.Y), new PointF(0, drawRect.Y + drawRect.Height), Color.LightGreen, Color.GreenYellow))
            {
                e.Graphics.FillRectangle(gradientBrush, drawRect);
            }
            using (Pen pen = new Pen(Color.LightGray, 1))
            {
                e.Graphics.DrawPath(pen, drawRect.RoundedPath());
            }
        }
    }
}