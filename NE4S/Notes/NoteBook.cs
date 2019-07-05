﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using NE4S.Scores;
using NE4S.Define;
using System.Diagnostics;

namespace NE4S.Notes
{
    /// <summary>
    /// 置かれている全ノーツをまとめる
    /// </summary>
    [Serializable()]
    public class NoteBook
    {
        public List<Note> ShortNotes { get; private set; } = new List<Note>();
        public List<Hold> HoldNotes { get; private set; } = new List<Hold>();
        public List<Slide> SlideNotes { get; private set; } = new List<Slide>();
        public List<AirHold> AirHoldNotes { get; private set; } = new List<AirHold>();
        public List<Air> AirNotes { get; private set; } = new List<Air>();
        public List<AttributeNote> AttributeNotes { get; private set; } = new List<AttributeNote>();

        public NoteBook()
        {
            //HACK: 開始時BPMを無理やり設定
            Status.CurrentValue = 120;
            AttributeNotes.Add(new BPM(
                new Position(0, 0),
                new PointF(
                    ScorePanel.Margin.Left + ScoreLane.Margin.Left,
                    ScorePanel.Margin.Top + ScoreLane.Height - ScoreLane.Margin.Bottom),
                Status.CurrentValue,
                0));
        }

		public bool Add(Note note)
		{
            if (note == null)
            {
                Logger.Error("ノーツを追加できません。引数newNoteがnullです。", true);
                return false;
            }
            switch (note)
            {
                case Tap _:
                case ExTap _:
                case AwesomeExTap _:
                case Flick _:
                case HellTap _:
                    {
                        ShortNotes.Add(note);
                    }
                    break;
                case AttributeNote att:
                    {
                        AttributeNotes.Add(att);
                    }
                    break;
                default:
                    {
                        Logger.Warn("不適切なノーツを追加できません。", true);
                        return false;
                    }
            }
            return true;
        }
        
        /// <summary>
        /// すでに配置されているショートノーツに対して新規Airノーツを配置し取り付けます。
        /// </summary>
        public bool AttachAirToShortNote(AirableNote airable, Air air)
        {
            if (airable == null || air == null)
            {
                Logger.Error("Airを取り付けできません。引数にnullが含まれます。", true);
                return false;
            }
            if (!ShortNotes.Contains(airable))
            {
                Logger.Error("Airノーツ取り付け先のAirableノーツはすでに配置されている必要があります。");
                return false;
            }
            if (airable.IsAirAttached)
            {
                Logger.Error("Air取り付け先のAirableノーツにはすでにAirが取り付けられています。");
                return false;
            }
            airable.AttachAir(air);
            AirNotes.Add(air);
            return true;
        }

        /// <summary>
        /// すでに配置されているショートノーツに対して新規AirHoldと新規AirUpCを配置し取り付けます。
        /// </summary>
        public bool AttachAirHoldToShortNote(AirableNote airable, AirHold airHold, AirUpC air)
        {
            if (airable == null || airHold == null || air == null)
            {
                Logger.Error("AirHoldを取り付けできません。引数にnullが含まれます。", true);
                return false;
            }
            if (!ShortNotes.Contains(airable))
            {
                Logger.Error("AirHold取り付け先のAirableノーツはすでに配置されている必要があります。", true);
                return false;
            }
            if (!airable.IsAirHoldAttachable)
            {
                Logger.Error("AirHold取り付け先のAirableノーツにはすでにAirやAirHoldが取り付けられています。", true);
                return false;
            }
            airable.AttachAirHold(airHold);
            AirHoldNotes.Add(airHold);
            if (!airable.IsAirAttached)
            {
                airable.AttachAir(air);
                AirNotes.Add(air);
            }
            return true;
        }

        /// <summary>
        /// すでに配置されているHoldに対して新規Airノーツを配置し取り付けます。
        /// </summary>
        public bool AttachAirToHold(Hold hold, Air air)
        {
            if (hold == null || air == null)
            {
                Logger.Error("Airの取り付けを行えません。引数にnullが含まれます。", true);
                return false;
            }
            if (!HoldNotes.Contains(hold))
            {
                Logger.Error("Air取り付け先のHoldノーツはすでに配置されている必要があります。", true);
                return false;
            }
            var end = hold.EndNote as AirableNote;
            if (end == null)
            {
                Logger.Critical("HoldにAirを取り付けられません。HoldEndがありません。", true);
                return false;
            }
            if (end.IsAirAttached)
            {
                Logger.Error("HoldにAirを取り付けられません。すでにAirが取り付けられています。", true);
                return false;
            }
            end.AttachAir(air);
            AirNotes.Add(air);
            return true;
        }

        public bool AttachAirHoldToHold(Hold hold, AirHold airHold, AirUpC air)
        {
            return true;
        }

		public bool Add(LongNote longNote)
		{
            if (longNote == null)
            {
                Logger.Error("ロングノーツを追加できません。引数がnullです。", true);
                return false;
            }
            switch (longNote)
            {
                case Hold hold:
                    {
                        HoldNotes.Add(hold);
                    }
                    break;
                case Slide slide:
                    {
                        SlideNotes.Add(slide);
                    }
                    break;
                case AirHold airHold:
                    {
                        AirHoldNotes.Add(airHold);
                    }
                    break;
                default:
                    {
                        Logger.Warn("不適切なロングノーツを追加できません。", true);
                        return false;
                    }
            }
            return true;
        }

		public void Delete(Note note)
		{
            if (note == null) return;
            if (note is Air)
            {
                Air air = note as Air;
                AirNotes.Remove(air);
                // air.DetachNote();
            }
            else if (note is HoldBegin || note is HoldEnd)
            {
                Hold hold = HoldNotes.Find(x => x.Contains(note));
                if (hold != null)
                {
                    HoldNotes.Remove(hold);
                    //終点にAirやAirHoldがくっついていたときの処理
                    HoldEnd holdEnd = hold.Find(x => x is HoldEnd) as HoldEnd;
                    AirNotes.Remove(holdEnd.Air);
                    AirHoldNotes.Remove(holdEnd.AirHold);
                }
            }
            else if (note is SlideBegin || note is SlideEnd)
            {
                Slide slide = SlideNotes.Find(x => x.Contains(note));
                if (slide != null)
                {
                    SlideNotes.Remove(slide);
                    //終点にAirやAirHoldがくっついていたときの処理
                    SlideEnd slideEnd = slide.Find(x => x is SlideEnd) as SlideEnd;
                    AirNotes.Remove(slideEnd.Air);
                    AirHoldNotes.Remove(slideEnd.AirHold);
                }
            }
            else if (note is SlideTap || note is SlideRelay || note is SlideCurve)
            {
                Slide slide = SlideNotes.Find(x => x.Contains(note));
                slide?.Remove(note);
            }
            else if (note is AirAction)
            {
                AirHold airHold = AirHoldNotes.Find(x => x.Contains(note));
                airHold?.Remove(note);
                if (airHold != null && !airHold.Where(x => x is AirAction).Any())
                {
                    AirHoldNotes.Remove(airHold);
                    // airHold.DetachNote();
                }
            }
            else if (note is AirableNote)
            {
                AirableNote airable = note as AirableNote;
                AirNotes.Remove(airable.Air);
                AirHoldNotes.Remove(airable.AirHold);
                ShortNotes.Remove(note);
            }
            else if (note is AttributeNote)
            {
                AttributeNotes.Remove(note as AttributeNote);
            }
            else ShortNotes.Remove(note);
        }

		public void Delete(LongNote longNote)
		{
            if (longNote == null) return;
			if (longNote is Hold) HoldNotes.Remove(longNote as Hold);
			else if (longNote is Slide) SlideNotes.Remove(longNote as Slide);
			else if (longNote is AirHold) AirHoldNotes.Remove(longNote as AirHold);
            //終点にくっついてるかもしれないAir系ノーツの破棄
            var airable = longNote.Find(x => x is AirableNote) as AirableNote;
            if (airable != null)
            {
                if (airable.IsAirAttached)
                {
                    AirNotes.Remove(airable.Air);
                }
                if (airable.IsAirHoldAttached)
                {
                    AirHoldNotes.Remove(airable.AirHold);
                }
            }
        }

        /// <summary>
        /// クリックされてるノーツがあったら投げる
        /// なかったらnullを投げる
        /// ノーツのどのへんがクリックされたかも特定する
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Note SelectedNote(PointF location, ref NoteArea noteArea)
        {
            Note selectedNote;
            //AirHold
            foreach (AirHold airHold in AirHoldNotes.Reverse<AirHold>())
            {
                if (!Status.IsAirHoldVisible) break;
                selectedNote = airHold.Find(x => x.Contains(location));
                if (selectedNote != null && !(selectedNote is AirHoldBegin))
                {
                    MyUtil.SetNoteArea(selectedNote, location, ref noteArea);
                    return selectedNote;
                }
            }
            //Air
            selectedNote = AirNotes.FindLast(x => x.Contains(location));
            if (selectedNote != null && Status.IsAirVisible)
            {
                MyUtil.SetNoteArea(selectedNote, location, ref noteArea);
                return selectedNote;
            }
            //ShortNote
            selectedNote = ShortNotes.FindLast(x => x.Contains(location));
            if (selectedNote != null && Status.IsShortNoteVisible)
            {
                MyUtil.SetNoteArea(selectedNote, location, ref noteArea);
                return selectedNote;
            }
            //Slide
            foreach (Slide slide in SlideNotes.Reverse<Slide>())
            {
                if (!Status.IsSlideVisible) break;
                selectedNote = slide.Find(x => x.Contains(location));
                if (selectedNote != null)
                {
                    MyUtil.SetNoteArea(selectedNote, location, ref noteArea);
                    return selectedNote;
                }
            }
            //Hold
            foreach (Hold hold in HoldNotes.Reverse<Hold>())
            {
                if (!Status.IsHoldVisible) break;
                selectedNote = hold.Find(x => x.Contains(location));
                if (selectedNote != null)
                {
                    MyUtil.SetNoteArea(selectedNote, location, ref noteArea);
                    return selectedNote;
                }
            }
            //AttributeNote
            selectedNote = AttributeNotes.FindLast(x => x.Contains(location));
            if (selectedNote != null)
            {
                noteArea = NoteArea.Center;
                return selectedNote;
            }
            return null;
        }

        /// <summary>
        /// クリックされてるノーツがあったら投げる
        /// なかったらnullを投げる
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public Note SelectedNote(PointF location)
        {
            //この変数は使用しない
            NoteArea noteArea = NoteArea.None;
            return SelectedNote(location, ref noteArea);
        }

        public Slide SelectedSlide(PointF locationVirtual, LaneBook laneBook)
        {
            return SlideNotes.FindLast(x => x.Contains(locationVirtual, laneBook));
        }

        public AirHold SelectedAirHold(PointF locationVirtual, LaneBook laneBook)
        {
            return AirHoldNotes.FindLast(x => x.Contains(locationVirtual, laneBook));
        }

        public void UpdateNoteLocation(LaneBook laneBook)
        {
            ShortNotes.ForEach(x => x.UpdateLocation(laneBook));
            HoldNotes.ForEach(x => x.UpdateLocation(laneBook));
            SlideNotes.ForEach(x => x.UpdateLocation(laneBook));
            AirHoldNotes.ForEach(x => x.UpdateLocation(laneBook));
            AirNotes.ForEach(x => x.UpdateLocation(laneBook));
            AttributeNotes.ForEach(x => x.UpdateLocation(laneBook));
        }

        public void RelocateNoteTickAfterScoreTick(int scoreTick, int deltaTick)
        {
            ShortNotes.
                Where(x => x.Position.Tick >= scoreTick).ToList().
                ForEach(x => x.RelocateOnly(new Position(x.Position.Lane, x.Position.Tick + deltaTick)));
            HoldNotes.ForEach(x => x.RelocateNoteTickAfterScoreTick(scoreTick, deltaTick));
            SlideNotes.ForEach(x => x.RelocateNoteTickAfterScoreTick(scoreTick, deltaTick));
            AirHoldNotes.ForEach(x => x.RelocateNoteTickAfterScoreTick(scoreTick, deltaTick));
            AirNotes.
                Where(x => x.Position.Tick >= scoreTick).ToList().
                ForEach(x => x.RelocateOnly(new Position(x.Position.Lane, x.Position.Tick + deltaTick)));
            AttributeNotes.
                Where(x => x.Position.Tick >= scoreTick).ToList().
                ForEach(x => x.RelocateOnly(new Position(x.Position.Lane, x.Position.Tick + deltaTick)));
        }

        public List<Note> GetNotesFromTickRange(int startTick, int endTick)
        {
            var notes = ShortNotes.Where(x => startTick <= x.Position.Tick && x.Position.Tick <= endTick);
            SlideNotes.ForEach(
                x =>
                {
                    var list = x.Where(y => startTick <= y.Position.Tick && y.Position.Tick <= endTick);
                    if(!(list.Where(y => y is SlideBegin || y is SlideEnd).Any()))
                    {
                        notes = notes.Union(list);
                    }
                });
            AirHoldNotes.ForEach(
                x =>
                {
                    var list = x.Where(y => startTick <= y.Position.Tick && y.Position.Tick <= endTick);
                    if (!(list.Where(y => y is AirHoldBegin || y is AirHoldEnd).Any()))
                    {
                        notes = notes.Union(list);
                    }
                });
            notes = notes.Union(AirNotes.Where(x => startTick <= x.Position.Tick && x.Position.Tick <= endTick));
            notes = notes.Union(AttributeNotes.Where(x => startTick <= x.Position.Tick && x.Position.Tick <= endTick));
            return notes.ToList();
        }

        public List<LongNote> GetLongNotesFromTickRange(int startTick, int endTick)
        {
            var longNotes = new List<LongNote>();
            HoldNotes.ForEach(
                x =>
                {
                    if ((startTick <= x.StartTick && x.StartTick <= endTick) || (startTick <= x.EndTick && x.EndTick <= endTick))
                    {
                        longNotes.Add(x);
                    }
                });
            SlideNotes.ForEach(
                x =>
                {
                    if ((startTick <= x.StartTick && x.StartTick <= endTick) || (startTick <= x.EndTick && x.EndTick <= endTick))
                    {
                        longNotes.Add(x);
                    }
                });
            AirHoldNotes.ForEach(
                x =>
                {
                    if ((startTick <= x.StartTick && x.StartTick <= endTick) || (startTick <= x.EndTick && x.EndTick <= endTick))
                    {
                        longNotes.Add(x);
                    }
                });
            return longNotes;
        }

        public void Paint(Graphics g, Point drawLocation, LaneBook laneBook)
		{
            //AttributeNote
            if (AttributeNotes != null)
            {
                var drawNotes = AttributeNotes
                    .Where(x => x.Position.Tick >= Status.DrawTickFirst && x.Position.Tick <= Status.DrawTickLast);
                foreach(var note in drawNotes)
                {
                    note.Draw(g, drawLocation);
                }
            }
            //Hold
            if (HoldNotes != null && Status.IsHoldVisible)
            {
                var drawNotes = HoldNotes.Where(x => x.IsDrawable());
                foreach(var note in drawNotes)
                {
                    note.Draw(g, drawLocation, laneBook);
                }
            }
            //Slide
            if (SlideNotes != null && Status.IsSlideVisible)
            {
                var drawNotes = SlideNotes.Where(x => x.IsDrawable());
                foreach(var note in drawNotes)
                {
                    note.Draw(g, drawLocation, laneBook);
                }
            }
            //ShortNote
            if (ShortNotes != null && Status.IsShortNoteVisible)
            {
                var drawNotes = ShortNotes
                    .Where(x => x.Position.Tick >= Status.DrawTickFirst && x.Position.Tick <= Status.DrawTickLast);
                foreach(var note in drawNotes)
                {
                    note.Draw(g, drawLocation);
                }
            }
            //Air
            if (AirNotes != null && Status.IsAirVisible)
            {
                var drawNotes = AirNotes
                    .Where(x => x.Position.Tick >= Status.DrawTickFirst && x.Position.Tick <= Status.DrawTickLast);
                foreach(var note in drawNotes)
                {
                    note.Draw(g, drawLocation);
                }
            }
            //AirHold
            if (AirHoldNotes != null && Status.IsAirHoldVisible)
            {
                var drawNotes = AirHoldNotes.Where(x => x.IsDrawable());
                foreach(var note in drawNotes)
                {
                    note.Draw(g, drawLocation, laneBook);
                }
            }
        }
	}
}
