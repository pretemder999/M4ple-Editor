﻿using NE4S.Notes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NE4S.Scores
{
    public delegate void LaneEventHandler(LaneBook laneBook);

    [Serializable()]
    public class LaneBook : List<ScoreLane>
    {
        public event LaneEventHandler UpdateNoteLocation;

        public LaneBook()
        {
			
        }

        public void SetScore(ScoreBook scoreBook, int beatNumer, int beatDenom, int barCount)
        {
			//新たに追加する譜面たちをリストでまとめる
			//これはLaneBookではないのでRefreshIndex()が行われない
			List<Score> newScores = new List<Score>();
            for (int i = 0; i < barCount; ++i) newScores.Add(new Score(beatNumer, beatDenom));
			//まとめた譜面たちをscoreBookに入れる
			scoreBook.Append(newScores);
            //新譜面たちをレーンに割り当て
            if (!this.Any())
            {
                Add(new ScoreLane());
            }
            foreach (Score newScore in newScores)
            {
                //newScoreが1つのレーンの最大サイズで収まるか判定
                if (newScore.BarSize <= ScoreInfo.LaneMaxBar)
                {
                    //現在のリストにあるレーンの末尾にまだnewScoreを入れる余裕があるか判定
                    if (this.Last().CurrentBarSize + newScore.BarSize > ScoreInfo.LaneMaxBar)
                    {
                        //余裕がないときは新たな空レーンを追加
                        Add(new ScoreLane());
                    }
                    //レーン末尾にnewScoreを格納
                    this.Last().AddScore(newScore);
                }
                //収まらなかった場合
                else
                {
                    //なんやかんやで分割して複数レーンに格納する
                    for (int i = 0; i < newScore.BarSize / ScoreInfo.LaneMaxBar; ++i)
                    {
                        //新たにレーンを追加
                        if(this.Last().CurrentBarSize > 0) Add(new ScoreLane());
                        //末尾のレーンに新たなScoreを範囲を指定して格納
                        this.Last().AddScore(
                            newScore,
                            new Range(
                                (int)(i * newScore.BeatDenom * ScoreInfo.LaneMaxBar + 1),
                                Math.Min(newScore.BeatNumer, (int)((i + 1) * newScore.BeatDenom * ScoreInfo.LaneMaxBar))));
                    }
                }
            }
            UpdateNoteLocation?.Invoke(this);
        }

        public void InsetScoreForward(NoteBook noteBook, ScoreBook scoreBook, Score score, int beatNumer, int beatDenom, int barCount)
        {
            if (scoreBook.Next(score) == null)
            {
                SetScore(scoreBook, beatNumer, beatDenom, barCount);
            }
            else
            {
                InsertScoreBackward(noteBook, scoreBook, scoreBook.Next(score), beatNumer, beatDenom, barCount);
            }
        }

        public void InsertScoreBackward(NoteBook noteBook, ScoreBook scoreBook, Score score, int beatNumer, int beatDenom, int barCount)
        {
            int initialScoreTick = score.StartTick;
            //scoreを初めて含むレーンを取得
            ScoreLane lane = Find(x => x.Contains(score));
            //新たに追加する譜面たちをリストでまとめる
			//これはLaneBookではないのでRefreshIndex()が行われない
            List<Score> newScores = new List<Score>();
            for (int i = 0; i < barCount; ++i) newScores.Add(new Score(beatNumer, beatDenom));
            //まとめた譜面たちをmodelに挿入
            scoreBook.InsertRange(score.Index, newScores);
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
            InsertRange(lane.Index, newLanes);
            //score以降に含まれるすべてのノーツに対して位置をずらす
            int deltaTick = barCount * ScoreInfo.MaxBeatDiv * beatNumer / beatDenom;
            noteBook.RelocateNoteTickAfterScoreTick(initialScoreTick, deltaTick);
            //
            FillLane();
        }

        /// <summary>
        /// 指定されたlaneの次の要素を返します
        /// </summary>
        /// <param name="lane"></param>
        /// <returns></returns>
        public ScoreLane Next(ScoreLane lane)
        {
            if(!Contains(lane) || lane.Index == Count - 1 || lane.Index == -1)
            {
                return null;
            }
            else
            {
                return this.ElementAt(lane.Index + 1);
            }
        }

        /// <summary>
        /// 最初からレーンを詰める
        /// </summary>
        public void FillLane()
        {
            if(this.Any()) FillLane(this.First());
        }

        /// <summary>
        /// begin以降のレーンを詰める
        /// </summary>
        /// <param name="begin"></param>
        public void FillLane(ScoreLane begin)
        {
            //lanesの末尾1つ前のレーンまで処理対象
            for (ScoreLane itrLane = begin; Next(itrLane) != null; itrLane = Next(itrLane))
            {
                //nextLaneに何かScoreが入っていて、そのnextLaneの最初のScoreをitrLaneに詰める余裕がある場合のみ処理を行う
                for (ScoreLane nextLane = Next(itrLane);
                    nextLane != null && nextLane.Any() && itrLane.CurrentBarSize + nextLane.FirstScore.BarSize <= ScoreInfo.LaneMaxBar;
                    nextLane = Next(itrLane)
                    )
                {
                    //itrLaneとnextLaneの間でScoreを詰める
                    itrLane.AddScore(nextLane.FirstScore);
                    nextLane.DeleteScore(nextLane.FirstScore);
                    //詰めた結果nextLaneが空になったらnextLaneを削除
                    if (!nextLane.Any()) Remove(nextLane);
                }
            }
            UpdateNoteLocation?.Invoke(this);
        }

        /// <summary>
        /// 指定したscoreからレーンを改行する
        /// </summary>
        /// <param name="score"></param>
        public void DivideLane(Score score)
        {
            if (score.BarSize > ScoreInfo.LaneMaxBar) return;
            //scoreを初めて含むレーンを取得
            ScoreLane lane = Find(x => x.Contains(score));
            //scoreがlaneの最初の要素の時は分割の意味がないので何もせずメソッドを抜ける
            if (lane.FirstScore.Equals(score)) return;
            ScoreLane newLane = new ScoreLane();
            Insert(lane.Index, newLane);
            while (!lane.FirstScore.Equals(score))
            {
                newLane.AddScore(lane.FirstScore, lane.FirstRange);
                lane.DeleteScore(lane.FirstScore);
            }
            //lane以降のレーンを詰める
            FillLane(lane);
        }

        /// <summary>
        /// scoreを始めとしてcount個のScoreを削除
        /// </summary>
        /// <param name="model"></param>
        /// <param name="score"></param>
        /// <param name="count"></param>
        public void DeleteScore(ScoreBook scoreBook, Score score, int count)
        {
            //イテレータ
            Score itrScore;
            int itrCount;
            //scoreからcount個Scoreを削除
            for (itrScore = score, itrCount = count; itrScore != null && itrCount > 0; itrScore = scoreBook.Next(itrScore), --itrCount)
            {
                //選択されたScoreが初めて含まれるレーンを特定
                ScoreLane laneBegin = Find(x => x.Contains(itrScore));
                int linkCount = itrScore.LinkCount;
                for (int i = 0; i < linkCount; ++i)
                {
                    laneBegin.DeleteScore(itrScore);
                    if (!laneBegin.Any())
                    {
                        ScoreLane blankLane = laneBegin;
                        laneBegin = Next(laneBegin);
                        Remove(blankLane);
                    }
                    else
                    {
                        laneBegin = Next(laneBegin);
                    }
                }
#if DEBUG
                System.Diagnostics.Debug.WriteLine("LinkCount : " + linkCount.ToString());
#endif
            }
            //scoreBookから該当範囲のScoreを削除
            scoreBook.Delete(score.Index, count);
            //レーンを詰める
            FillLane();
        }

        /// <summary>
        /// レーンのインデックスを更新
        /// </summary>
        private void RefreshIndex()
        {
            for (int i = 0; i < Count; ++i) this[i].Index = i;
        }

        public new void Add(ScoreLane item)
        {
            base.Add(item);
            RefreshIndex();
        }

        public new void Remove(ScoreLane item)
        {
            base.Remove(item);
            RefreshIndex();
        }

        public new void Insert(int index, ScoreLane item)
        {
            base.Insert(index, item);
            RefreshIndex();
        }

        public new void InsertRange(int index, IEnumerable<ScoreLane> collection)
        {
            base.InsertRange(index, collection);
            RefreshIndex();
        }
    }
}
