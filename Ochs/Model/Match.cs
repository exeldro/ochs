using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Ochs
{
    public class Match
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Location { get; set; }
        public virtual Person FighterRed { get; set; }
        public virtual Person FighterBlue { get; set; }
        public virtual int RoundsRed { get; set; }
        public virtual int RoundsBlue { get; set; }
        public virtual int ScoreRed { get; set; }
        public virtual int ScoreBlue { get; set; }
        public virtual int ExchangeCount { get; set; }
        public virtual int DoubleCount { get; set; }
        public virtual int Round { get; set; } = 1;
        public virtual MatchResult Result { get; set; } = MatchResult.None;
        public virtual TimeSpan Time { get; set; }
        public virtual DateTime? TimeRunningSince { get; set; }
        public virtual DateTime? TimeOutSince { get; set; }
        public virtual DateTime? PlannedDateTime { get; set; }
        public virtual bool Planned => PlannedDateTime.HasValue;
        public virtual DateTime? StartedDateTime { get; set; }
        public virtual bool Started => StartedDateTime.HasValue;
        public virtual DateTime? FinishedDateTime { get; set; }
        public virtual bool Finished => FinishedDateTime.HasValue;
        public virtual DateTime? ValidatedDateTime { get; set; }
        public virtual bool Validated => ValidatedDateTime.HasValue;
        public virtual IList<MatchEvent> Events { get; set; } = new List<MatchEvent>();
        public virtual Competition Competition { get; set; }
        public virtual Phase Phase { get; set; }
        public virtual Pool Pool { get; set; }
        public virtual TimeSpan LiveTime => Time + (TimeRunningSince.HasValue ? DateTime.Now.Subtract(TimeRunningSince.Value) : TimeSpan.Zero);
        public virtual TimeSpan? LiveTimeOut => TimeOutSince.HasValue?DateTime.Now.Subtract(TimeOutSince.Value):(TimeSpan?)null;

        public virtual MatchRules Rules { get; set; }
        public virtual MatchRules GetRules() => Rules ?? Phase?.MatchRules ?? Competition.MatchRules;

        public virtual string GetLocation()
        {
            return !string.IsNullOrWhiteSpace(Location) ? Location
                : (!string.IsNullOrWhiteSpace(Pool?.Location) ? Pool.Location : Phase?.Location);
        }

        public virtual void UpdateMatchData()
        {
            var rules = GetRules();
            if (rules != null && rules.SplitRounds && rules.Rounds > 1)
            {
                RoundsBlue = 0;
                RoundsRed = 0;
                ExchangeCount = Events.Count(x => x.Round == Round && x.IsExchange);
                DoubleCount = Events.Count(x => x.Round == Round && x.Type == MatchEventType.DoubleHit);
                for (int round = 1; round <= Round; round++)
                {
                    if (rules.CountDownScore && rules.PointsMax > 0)
                    {
                        if (rules.SubtractPoints)
                        {
                            ScoreRed = Math.Max(rules.PointsMax - Events.Where(x => x.Round == round && x.Type != MatchEventType.MatchPointDeduction).Sum(x => (x.PointsRed < 0 || x.PointsBlue < 0) ? x.PointsRed : (x.PointsRed > x.PointsBlue ? x.PointsRed - x.PointsBlue : 0)),0);
                            ScoreBlue = Math.Max(rules.PointsMax - Events.Where(x => x.Round == round && x.Type != MatchEventType.MatchPointDeduction).Sum(x => (x.PointsBlue < 0 || x.PointsRed < 0) ? x.PointsBlue : (x.PointsBlue > x.PointsRed ? x.PointsBlue - x.PointsRed : 0)),0);
                        }
                        else
                        {
                            ScoreRed = Math.Max(rules.PointsMax - Events.Where(x => x.Round == round && x.Type != MatchEventType.MatchPointDeduction).Sum(x => x.PointsRed),0);
                            ScoreBlue = Math.Max(rules.PointsMax - Events.Where(x => x.Round == round && x.Type != MatchEventType.MatchPointDeduction).Sum(x => x.PointsBlue),0);
                        }

                    }else{
                        if (rules.SubtractPoints)
                        {
                            ScoreRed = Events.Where(x => x.Round == round && x.Type != MatchEventType.MatchPointDeduction).Sum(x => (x.PointsRed < 0 || x.PointsBlue < 0) ? x.PointsRed : (x.PointsRed > x.PointsBlue ? x.PointsRed - x.PointsBlue : 0));
                            ScoreBlue = Events.Where(x => x.Round == round && x.Type != MatchEventType.MatchPointDeduction).Sum(x => (x.PointsBlue < 0 || x.PointsRed < 0) ? x.PointsBlue : (x.PointsBlue > x.PointsRed ? x.PointsBlue - x.PointsRed : 0));
                        }
                        else
                        {
                            ScoreRed = Events.Where(x => x.Round == round && x.Type != MatchEventType.MatchPointDeduction).Sum(x => x.PointsRed);
                            ScoreBlue = Events.Where(x => x.Round == round && x.Type != MatchEventType.MatchPointDeduction).Sum(x => x.PointsBlue);
                        }
                    }
                    if (round < Round || Finished)
                    {
                        if (ScoreRed > ScoreBlue)
                        {
                            RoundsRed++;
                        }
                        else if (ScoreBlue > ScoreRed)
                        {
                            RoundsBlue++;
                        }
                        if (Finished)
                        {
                            ScoreBlue = 0;
                            ScoreRed = 0;
                        }
                    }
                }
            }
            else
            {
                RoundsBlue = 0;
                RoundsRed = 0;
                ExchangeCount = Events.Count(x => x.IsExchange);
                DoubleCount = Events.Count(x => x.Type == MatchEventType.DoubleHit);
                if (rules != null && rules.CountDownScore && rules.PointsMax > 0)
                {
                    if (rules.SubtractPoints)
                    {
                        ScoreRed = Math.Max(rules.PointsMax - Events.Where(x => x.Type != MatchEventType.MatchPointDeduction).Sum(x => (x.PointsRed < 0 || x.PointsBlue < 0) ? x.PointsRed : (x.PointsRed > x.PointsBlue ? x.PointsRed - x.PointsBlue : 0)), 0);
                        ScoreBlue = Math.Max(rules.PointsMax - Events.Where(x => x.Type != MatchEventType.MatchPointDeduction).Sum(x => (x.PointsBlue < 0 || x.PointsRed < 0) ? x.PointsBlue : (x.PointsBlue > x.PointsRed ? x.PointsBlue - x.PointsRed : 0)), 0);
                    }
                    else
                    {
                        ScoreRed = Math.Max(rules.PointsMax - Events.Where(x => x.Type != MatchEventType.MatchPointDeduction).Sum(x => x.PointsRed), 0);
                        ScoreBlue = Math.Max(rules.PointsMax - Events.Where(x => x.Type != MatchEventType.MatchPointDeduction).Sum(x => x.PointsBlue), 0);
                    }
                }
                else
                {
                    if (rules != null && rules.SubtractPoints)
                    {
                        ScoreRed = Events.Where(x => x.Type != MatchEventType.MatchPointDeduction).Sum(x => (x.PointsRed < 0 || x.PointsBlue < 0) ? x.PointsRed : (x.PointsRed > x.PointsBlue ? x.PointsRed - x.PointsBlue : 0));
                        ScoreBlue = Events.Where(x => x.Type != MatchEventType.MatchPointDeduction).Sum(x => (x.PointsBlue < 0 || x.PointsRed < 0) ? x.PointsBlue : (x.PointsBlue > x.PointsRed ? x.PointsBlue - x.PointsRed : 0));
                    }
                    else
                    {
                        ScoreRed = Events.Where(x => x.Type != MatchEventType.MatchPointDeduction).Sum(x => x.PointsRed);
                        ScoreBlue = Events.Where(x => x.Type != MatchEventType.MatchPointDeduction).Sum(x => x.PointsBlue);
                    }
                }
            }
        }
    }

    public enum MatchResult
    {
        None,
        WinRed,
        WinBlue,
        Draw,
        ForfeitRed,
        ForfeitBlue,
        DisqualificationBlue,
        DisqualificationRed,
        Skipped
    }
}