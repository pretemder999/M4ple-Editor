﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using NE4S.Notes;
using NE4S.Scores;

namespace NE4S
{
	public static class MyUtil
	{
		public static int Gcd(int m, int n)
		{
			if (m < n) return Gcd(n, m);
			if (m == 0 && n == 0) return 1;
			if (n == 0) return m;
			return Gcd(n, m % n);
		}

        public static int Lcm(int m, int n)
        {
            return m * n / Gcd(m, n);
        }

        /// <summary>
        /// ノーツが選択されたとき、どのへんの領域をクリックしたのか判定します
        /// Editモードでノーツをクリックした位置に応じてサイズ変更か位置変更かを決めるために使う
        /// </summary>
        /// <param name="note"></param>
        /// <param name="location"></param>
        /// <param name="noteArea"></param>
        public static void SetNoteArea(Note note, PointF location, ref int noteArea)
        {
            //それぞれの領域の割合を設定
            float leftCenter = .33f, centerRight = .66f;
            float locationRatio = (location.X - note.Location.X) / note.Width;
            if (locationRatio <= leftCenter) noteArea = Define.NoteArea.LEFT;
            else if (locationRatio <= centerRight) noteArea = Define.NoteArea.CENTER;
            else noteArea = Define.NoteArea.RIGHT;
        }

        /// <summary>
        /// list内のnoteの1つ次の要素を返します。noteがlistに含まれていないか、末尾の場合はnullを返します。
        /// </summary>
        /// <param name="list"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        public static Note Next(this List<Note> list, Note note)
        {
            if (!list.Contains(note)) return null;
            if (list.IndexOf(note) >= list.Count - 1) return null;
            return list.ElementAt(list.IndexOf(note) + 1);
        }

        /// <summary>
        /// 小数第3位以下を四捨五入する
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static decimal MyRound(this decimal value)
        {
            return Math.Round(value * 100m) / 100m;
        }

        public static float MyRound(this float value)
        {
            return (float)Math.Round(value * 100f) / 100f;
        }

        public static PointF AddX(this PointF pointF, float x)
        {
            return new PointF(pointF.X + x, pointF.Y);
        }

        public static PointF AddY(this PointF pointF, float y)
        {
            return new PointF(pointF.X, pointF.Y + y);
        }

        public static PointF Add(this PointF pointF, float x, float y)
        {
            return new PointF(pointF.X + x, pointF.Y + y);
        }

        public static PointF Add(this PointF pointF, PointF other)
        {
            return new PointF(pointF.X + other.X, pointF.Y + other.Y);
        }

        public static PointF Add(this PointF pointF, Point other)
        {
            return new PointF(pointF.X + other.X, pointF.Y + other.Y);
        }

        public static PointF Sub(this PointF pointF, PointF other)
        {
            return new PointF(pointF.X - other.X, pointF.Y - other.Y);
        }

        public static PointF Sub(this PointF pointF, Point other)
        {
            return new PointF(pointF.X - other.X, pointF.Y - other.Y);
        }

        public static PointF Mult(this PointF pointF, float k)
        {
            return new PointF(pointF.X * k, pointF.Y * k);
        }

        public static Point AddX(this Point point, int x)
        {
            return new Point(point.X + x, point.Y);
        }

        public static Point AddY(this Point point, int y)
        {
            return new Point(point.X, point.Y + y);
        }

        public static Point Add(this Point point, Point other)
        {
            return new Point(point.X + other.X, point.Y + other.Y);
        }

        public static Point Sub(this Point point, Point other)
        {
            return new Point(point.X - other.X, point.Y - other.Y);
        }

        /// <summary>
        /// いまのところSlideカーブでの当たり判定用パスを作るためのみに使う
        /// 汎用性を高めればDrawSlideCurveメソッドとかをスッキリできそう
        /// </summary>
        /// <param name="past"></param>
        /// <param name="curve"></param>
        /// <param name="future"></param>
        /// <param name="drawOffset"></param>
        /// <returns></returns>
        public static GraphicsPath CreateSlideCurvePath(Note past, Note curve, Note future, PointF drawOffset)
        {
            GraphicsPath graphicsPath = new GraphicsPath();
            PointF pastRerativeLocation = new PointF(past.Location.X, past.Location.Y);
            float positionDistanceFuture = (future.Position.Tick - past.Position.Tick) * ScoreInfo.MaxBeatHeight;
            float positionDistanceCurve = (curve.Position.Tick - past.Position.Tick) * ScoreInfo.MaxBeatHeight;
            float diffXFuture = (future.Position.Lane - past.Position.Lane) * ScoreInfo.MinLaneWidth;
            float diffXCurve = (curve.Position.Lane - past.Position.Lane) * ScoreInfo.MinLaneWidth;
            //ノーツfutureの位置はノーツpastの位置に2ノーツの距離を引いて表す。またTopRightの水平位置はfutureのWidthを使うことに注意
            PointF topLeft = pastRerativeLocation.Add(diffXFuture, -positionDistanceFuture).Add(drawOffset);
            PointF topRight = pastRerativeLocation.Add(diffXFuture, -positionDistanceFuture).Add(-drawOffset.X, drawOffset.Y).AddX(future.Width);
            //以下の2つはレーンをまたがないときと同じ
            PointF bottomLeft = pastRerativeLocation.Add(drawOffset);
            PointF bottomRight = pastRerativeLocation.Add(-drawOffset.X, drawOffset.Y).AddX(past.Width);
            //3つのそれぞれのノーツの中心の座標
            PointF topCenter = topLeft.AddX(future.Width / 2f - drawOffset.X);
            PointF bottomCenter = bottomLeft.AddX(past.Width / 2f - drawOffset.X);
            PointF curveCenter = pastRerativeLocation.Add(diffXCurve, -positionDistanceCurve).AddX(curve.Width / 2f);
            //
            //下からアンカーまでの比率
            float ratio = (curveCenter.Y - bottomCenter.Y) / (topCenter.Y - bottomCenter.Y);
            //カーブノーツのY座標で水平にスライドを切ったときのスライド幅
            float widthAnchor = (topRight.X - topLeft.X) * ratio + (bottomRight.X - bottomLeft.X) * (1 - ratio);
            //
            graphicsPath.AddBezier(bottomLeft, curveCenter.AddX(-widthAnchor / 2f), topLeft);
            graphicsPath.AddLine(topLeft, topRight);
            graphicsPath.AddBezier(topRight, curveCenter.AddX(widthAnchor / 2f), bottomRight);
            graphicsPath.AddLine(bottomLeft, bottomRight);
            return graphicsPath;
        }

        /// <summary>
        /// 現在の図形に２次ベジエ曲線を追加します
        /// </summary>
        /// <param name="graphicsPath"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="anchor"></param>
        public static void AddBezier(this GraphicsPath graphicsPath, PointF begin, PointF anchor, PointF end)
        {
            float ratio = 2 / 3f;
            PointF beginAnchor = begin.Add(anchor.Sub(begin).Mult(ratio));
            PointF endAnchor = end.Add(anchor.Sub(end).Mult(ratio));
            graphicsPath?.AddBezier(begin, beginAnchor, endAnchor, end);
        }

        /// <summary>
        /// ２次ベジエ曲線を描画します
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="pen"></param>
        /// <param name="begin"></param>
        /// <param name="end"></param>
        /// <param name="anchor"></param>
        public static void DrawBezier(this Graphics graphics, Pen pen, PointF begin, PointF anchor, PointF end)
        {
            float ratio = 2 / 3f;
            PointF beginAnchor = begin.Add(anchor.Sub(begin).Mult(ratio));
            PointF endAnchor = end.Add(anchor.Sub(end).Mult(ratio));
            graphics?.DrawBezier(pen, begin, beginAnchor, endAnchor, end);
        }

        public static GraphicsPath RoundedPath(this RectangleF rectangle)
        {
            float roundRatio = .25f;
            SizeF rectSize = new SizeF(rectangle.Width * roundRatio, rectangle.Height * roundRatio);
            RectangleF rectTopLeft = new RectangleF(rectangle.X, rectangle.Y, rectSize.Width, rectSize.Height);
            RectangleF rectTopRight = new RectangleF(rectangle.X + rectangle.Width * (1 - roundRatio), rectangle.Y, rectSize.Width, rectSize.Height);
            RectangleF rectBottomLeft = new RectangleF(rectangle.X, rectangle.Y + rectangle.Height * (1 - roundRatio), rectSize.Width, rectSize.Height);
            RectangleF rectBottomRight = new RectangleF(rectangle.X + rectangle.Width * (1 - roundRatio), rectangle.Y + rectangle.Height * (1 - roundRatio), rectSize.Width, rectSize.Height);
            GraphicsPath graphicsPath = new GraphicsPath();
            graphicsPath.AddArc(rectTopLeft, 180, 90);
            graphicsPath.AddArc(rectTopRight, 270, 90);
            graphicsPath.AddArc(rectBottomRight, 0, 90);
            graphicsPath.AddArc(rectBottomLeft, 90, 90);
            graphicsPath.CloseFigure();
            return graphicsPath;
        }
    }
}