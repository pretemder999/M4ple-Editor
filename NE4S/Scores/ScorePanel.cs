﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using NE4S.Component;

namespace NE4S.Scores
{
    /// <summary>
    /// MainFormのTabPageのScoreに貼るやつ
    /// </summary>
    public class ScorePanel
    {
        private Size panelSize;
        private int currentPositionX, currentWidthMax;
        private LaneBook lanes;
        private Model model;
        private HScrollBar hSBar;
        private PictureBox pBox;

        class Margin
        {
            public static int
                Top = ScoreInfo.PanelMargin.Top,
                Bottom = ScoreInfo.PanelMargin.Bottom,
                Left = ScoreInfo.PanelMargin.Left,
                Right = ScoreInfo.PanelMargin.Right;
        }

        public ScorePanel(PictureBox pBox, HScrollBar hSBar)
        {
            this.pBox = pBox;
            panelSize = pBox.Size;
            currentPositionX = 0;
            currentWidthMax = 0;
            lanes = new LaneBook();
            model = new Model();
            this.hSBar = hSBar;
            hSBar.Minimum = 0;
#if DEBUG
            //*
            SetScore(4, 4, 10);
            SetScore(3, 4, 5);
            SetScore(6, 8, 8);
            
            SetScore(2, 64, 32);
            SetScore(13, 4, 2);
            SetScore(2, 4, 8);
            SetScore(26, 8, 2);
            SetScore(1, 32, 32);
            //*/
            SetScore(7, 1, 1);
            SetScore(4, 4, 1);
            SetScore(8, 8, 1);
            SetScore(16, 16, 1);

            //System.Diagnostics.Debug.WriteLine(lanes.Count);
#endif
        }

        /// <summary>
        /// 末尾に指定した拍子数の譜面を指定した個数追加
        /// </summary>
        /// <param name="beatNumer">拍子分子</param>
        /// <param name="beatDenom">拍子分母</param>
        /// <param name="barCount">個数</param>
        private void SetScore(int beatNumer, int beatDenom, int barCount)
        {
            //新たに追加する譜面たちをリストでまとめる
            List<Score> newScores = new List<Score>();
            for (int i = 0; i < barCount; ++i) newScores.Add(new Score(beatNumer, beatDenom));
            //まとめた譜面たちをmodelに入れる
            model.AppendScore(newScores);
            //新規追加前のレーンリストの要素数を記録
            int pastLaneCount = lanes.Count;
            //新譜面たちをレーンに割り当て
            foreach(Score newScore in newScores)
            {
                //newScoreが1つのレーンの最大サイズで収まるか判定
                if(newScore.BarSize <= ScoreInfo.LaneMaxBar)
                {
                    //そもそも現在のレーンリストが空の時は新レーンを1つ補充
                    if (!lanes.Any())
                    {
                        lanes.Add(new ScoreLane());
                    }
                    //現在のリストにあるレーンの末尾にまだnewScoreを入れる余裕があるか判定
                    if (lanes.Last().CurrentBarSize + newScore.BarSize > ScoreInfo.LaneMaxBar)
                    {
                        //余裕がないときは新たな空レーンを追加
                        lanes.Add(new ScoreLane());
                    }
                    //レーン末尾にnewScoreを格納
                    lanes.Last().AddScore(newScore, new Range(1, newScore.BeatNumer));
                }
                //収まらなかった場合
                else
                {
                    //なんやかんやで分割して複数レーンに格納する
                    for(int i = 0; i < newScore.BarSize / ScoreInfo.LaneMaxBar; ++i)
                    {
                        //新たにレーンを追加
                        lanes.Add(new ScoreLane());
                        //末尾のレーンに新たなScoreを範囲を指定して格納
                        lanes.Last().AddScore(
                            newScore, 
                            new Range(
                                (int)(i * newScore.BeatDenom * ScoreInfo.LaneMaxBar + 1),
                                Math.Min(newScore.BeatNumer, (int)((i + 1) * newScore.BeatDenom * ScoreInfo.LaneMaxBar))));
                    }
                }
            }
            //レーンの増分だけパネルの最大幅を更新
            currentWidthMax += (int)(ScoreLane.Width + Margin.Left + Margin.Right) * (lanes.Count - pastLaneCount);
            hSBar.Maximum = currentWidthMax < panelSize.Width ? 0 : currentWidthMax - panelSize.Width;
        }

        /// <summary>
        /// scoreの1つ先に新たにscoreを挿入
        /// </summary>
        /// <param name="score"></param>
        /// <param name="beatNumer"></param>
        /// <param name="beatDenom"></param>
        /// <param name="barCount"></param>
        public void InsertScoreForward(Score score, int beatNumer, int beatDenom, int barCount)
        {
            if(model.ScoreNext(score) == null)
            {
                SetScore(beatNumer, beatDenom, barCount);
            }
            else
            {
                InsertScoreBackward(model.ScoreNext(score), beatNumer, beatDenom, barCount);
            }
        }

        /// <summary>
        /// scoreの1つ前に新たにscoreを挿入
        /// </summary>
        /// <param name="score"></param>
        /// <param name="beatNumer"></param>
        /// <param name="beatDenom"></param>
        /// <param name="barCount"></param>
        public void InsertScoreBackward(Score score, int beatNumer, int beatDenom, int barCount)
        {
            //scoreを初めて含むレーンを取得
            ScoreLane lane = lanes.Find(x => x.Contains(score));
            //新たに追加する譜面たちをリストでまとめる
            List<Score> newScores = new List<Score>();
            for (int i = 0; i < barCount; ++i) newScores.Add(new Score(beatNumer, beatDenom));
            //まとめた譜面たちをmodelに挿入
            model.InsertScore(score.ScoreIndex, newScores);
            //新規追加前のレーンリストの要素数を記録
            int pastLaneCount = lanes.Count;
            //挿入する譜面を格納するためのレーンリストを作成
            List<ScoreLane> newLanes = new List<ScoreLane>();
            //新譜面たちをnewLanesに割り当て
            foreach (Score newScore in newScores)
            {
                //newScoreが1つのレーンの最大サイズで収まるか判定
                if (newScore.BarSize <= ScoreInfo.LaneMaxBar)
                {
                    //そもそも現在のレーンリストが空の時は新レーンを1つ補充
                    if (!newLanes.Any())
                    {
                        newLanes.Add(new ScoreLane());
                    }
                    //現在のリストにあるレーンの末尾にまだnewScoreを入れる余裕があるか判定
                    if (newLanes.Last().CurrentBarSize + newScore.BarSize > ScoreInfo.LaneMaxBar)
                    {
                        //余裕がないときは新たな空レーンを追加
                        newLanes.Add(new ScoreLane());
                    }
                    //レーン末尾にnewScoreを格納
                    newLanes.Last().AddScore(newScore, new Range(1, newScore.BeatNumer));
                }
                //収まらなかった場合
                else
                {
                    //なんやかんやで分割して複数レーンに格納する
                    for (int i = 0; i < newScore.BarSize / ScoreInfo.LaneMaxBar; ++i)
                    {
                        //新たにレーンを追加
                        newLanes.Add(new ScoreLane());
                        //末尾のレーンに新たなScoreを範囲を指定して格納
                        newLanes.Last().AddScore(
                            newScore,
                            new Range(
                                (int)(i * newScore.BeatDenom * ScoreInfo.LaneMaxBar + 1),
                                Math.Min(newScore.BeatNumer, (int)((i + 1) * newScore.BeatDenom * ScoreInfo.LaneMaxBar))));
                    }
                }
            }
            //scoreとその1つ前のScoreでレーンを分割
            DivideLane(score);
            //
            lanes.InsertRange(lanes.IndexOf(lane), newLanes);
            //
            FillLane();
            //レーンの増分だけパネルの最大幅を更新
            currentWidthMax += (int)(ScoreLane.Width + Margin.Left + Margin.Right) * (lanes.Count - pastLaneCount);
            hSBar.Maximum = currentWidthMax < panelSize.Width ? 0 : currentWidthMax - panelSize.Width;
            //PictureBoxを更新
            pBox.Refresh();
        }

        /// <summary>
        /// 指定したscoreとその1つ前のScoreでレーンを2つに分割する
        /// </summary>
        /// <param name="score"></param>
        public void DivideLane(Score score)
        {
            //scoreを初めて含むレーンを取得
            ScoreLane lane = lanes.Find(x => x.Contains(score));
            //scoreがlaneの最初の要素の時は分割の意味がないので何もせずメソッドを抜ける
            if (lane.BeginScore().Equals(score)) return;
            //新規追加前のレーンリストの要素数を記録
            int pastLaneCount = lanes.Count;
            ScoreLane newLane = new ScoreLane();
            lanes.Insert(lanes.IndexOf(lane), newLane);
            while (!lane.BeginScore().Equals(score))
            {
                newLane.AddScore(lane.BeginScore(), lane.BeginRange());
                lane.DeleteScore(lane.BeginScore());
            }
            //lane以降のレーンを詰める
            FillLane(lane);
            //レーンの増分だけパネルの最大幅を更新
            currentWidthMax += (int)(ScoreLane.Width + Margin.Left + Margin.Right) * (lanes.Count - pastLaneCount);
            hSBar.Maximum = currentWidthMax < panelSize.Width ? 0 : currentWidthMax - panelSize.Width;
            //PictureBoxを更新
            pBox.Refresh();
        }

        /// <summary>
        /// 指定されたscore削除
        /// </summary>
        /// <param name="score">削除対象のScore</param>
        public void DeleteScore(Score score)
        {
            DeleteScore(score, 1);
        }

        /// <summary>
        /// 指定されたscoreからcount個のScoreを削除
        /// </summary>
        /// <param name="score">削除開始のScore</param>
        /// <param name="count">削除する個数</param>
        public void DeleteScore(Score score, int count)
        {
            //削除前のレーンリストの要素数を記録
            int pastLaneCount = lanes.Count;
            //イテレータ
            Score itrScore;
            int itrCount;
            //scoreからcount個Scoreを削除
            for(itrScore = score, itrCount = count; itrScore != null && itrCount > 0; itrScore = model.ScoreNext(itrScore), --itrCount)
            {
                //選択されたScoreが初めて含まれるレーンを特定
                ScoreLane laneBegin = lanes.Find(x => x.Contains(itrScore));
                if (laneBegin.IsScoreClose(itrScore))
                {
                    laneBegin.DeleteScore(itrScore);
                    //削除処理によってレーンが空になっていないか判定
                    if (!laneBegin.Any()) lanes.Remove(laneBegin);
                }
                else
                {
                    
                }
            }
            //modelから該当範囲のScoreを削除
            model.DeleteScore(score.ScoreIndex, count);
            //レーンを詰める
            FillLane();
            //レーンの増分だけパネルの最大幅を更新
            currentWidthMax += (int)(ScoreLane.Width + Margin.Left + Margin.Right) * (lanes.Count - pastLaneCount);
            hSBar.Maximum = currentWidthMax < panelSize.Width ? 0 : currentWidthMax - panelSize.Width;
            //PictureBoxを更新
            pBox.Refresh();
        }

        /// <summary>
        /// レーン全体を詰める
        /// </summary>
        public void FillLane()
        {
            if(lanes.Any()) FillLane(lanes.First());
        }

        /// <summary>
        /// begin以降のレーンを詰める
        /// </summary>
        /// <param name="begin"></param>
        public void FillLane(ScoreLane begin)
        {
            //lanesの末尾1つ前のレーンまで処理対象
            for(ScoreLane itrLane = begin; lanes.Next(itrLane) != null; itrLane = lanes.Next(itrLane))
            {
                //nextLaneに何かScoreが入っていて、そのnextLaneの最初のScoreをitrLaneに詰める余裕がある場合のみ処理を行う
                for(ScoreLane nextLane = lanes.Next(itrLane); 
                    nextLane != null && nextLane.Any() && itrLane.CurrentBarSize + nextLane.BeginScore().BarSize <= ScoreInfo.LaneMaxBar;
                    nextLane = lanes.Next(itrLane)
                    )
                {
                    //itrLaneとnextLaneの間でScoreを詰める
                    itrLane.AddScore(nextLane.BeginScore());
                    nextLane.DeleteScore(nextLane.BeginScore());
                    //詰めた結果nextLaneが空になったらnextLaneを削除
                    if (!nextLane.Any()) lanes.Remove(nextLane);
                }
            }
            //pBoxを更新
            pBox.Refresh();
        }

        public void MouseClick(MouseEventArgs e)
        {
#if DEBUG
            //クリックされたレーンを特定
            ScoreLane selectedLane = null;
            for(int i = 0; i < lanes.Count; ++i)
            {
                if(lanes[i].HitRect.Contains(
                    (float)(currentPositionX + e.X - i * (ScoreLane.Width + Margin.Left + Margin.Right)),
                    e.Y
                    ))
                {
                    selectedLane = lanes[i];
                }
            }
            if (selectedLane != null && selectedLane.SelectedScore(e) != null && e.Button == MouseButtons.Right)
            {
                new EditCMenu(this, selectedLane, selectedLane.SelectedScore(e)).Show(pBox, e.Location);
            }
#endif
        }

        public void MouseScroll(int delta)
        {
            currentPositionX -= delta;
            if (currentPositionX < hSBar.Minimum) currentPositionX = hSBar.Minimum;
            else if (hSBar.Maximum < currentPositionX) currentPositionX = hSBar.Maximum;
            hSBar.Value = currentPositionX;
        }

        public void HSBarScroll(ScrollEventArgs e)
        {
            currentPositionX += (e.NewValue - e.OldValue);
        }

        public void PaintPanel(PaintEventArgs e)
        {
            for(int i = 0; i < lanes.Count; ++i)
            {
                //ScoreLaneが表示範囲内にあるか
                if (currentPositionX < (ScoreLane.Width + Margin.Left + Margin.Right) * (i + 1) && 
                    (ScoreLane.Width + Margin.Left + Margin.Right) * i < currentPositionX + panelSize.Width)
                {
                    //ScoreLaneの相対位置のX座標を設定
                    int drawPosX = ((int)ScoreLane.Width + Margin.Left + Margin.Right) * i - currentPositionX + Margin.Left;
                    int drawPosY = Margin.Top;
                    //ScoreLaneを描画
                    lanes[i].PaintLane(e, drawPosX, drawPosY);
                }
            }
        }
    }
}
