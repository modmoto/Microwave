using System;
using Microwave.Queries.Exceptions;
using NCrontab;

namespace Microwave.Queries.Polling
{
    public class PollingInterval<T> : IPollingInterval
    {
        public Type AsyncCallType => typeof(T);

        private readonly int _second;

        private readonly CrontabSchedule _cronNotation;
        private readonly DateTime? _nowTime;

        public DateTime Next
        {
            get
            {
                var baseTime = _nowTime ?? DateTime.UtcNow;
                if (_cronNotation != null) return _cronNotation.GetNextOccurrence(baseTime);

                var secondsAfterLastHappening = baseTime.Second % _second;
                var nextSecondHappening = baseTime.Second - secondsAfterLastHappening + _second;

                return nextSecondHappening >= 60
                    ? NextFullMinute(baseTime)
                    : NextTimeWithSecond(baseTime, nextSecondHappening);
            }
        }

        private static DateTime NextTimeWithSecond(DateTime baseTime, int nextSecondHappening)
        {
            return new DateTime(
                baseTime.Year,
                baseTime.Month,
                baseTime.Day,
                baseTime.Hour,
                baseTime.Minute,
                nextSecondHappening);
        }

        private static DateTime NextFullMinute(DateTime baseTime)
        {
            return new DateTime(
                baseTime.Year,
                baseTime.Month,
                baseTime.Day,
                baseTime.Hour,
                baseTime.Minute + 1,
                0,
                0);
        }

        public PollingInterval(string cronNotation)
        {
            var schedule = CrontabSchedule.Parse(cronNotation);
            _cronNotation = schedule;
        }

        public PollingInterval(int second = 1)
        {
            if (second < 1 || second > 60) throw new InvalidTimeNotationException();

            _second = second;
        }
    }
}