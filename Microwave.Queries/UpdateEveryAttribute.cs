using System;
using NCrontab;

namespace Microwave.Queries
{
    public class UpdateEveryAttribute : Attribute
    {
        private readonly int _second;

        private readonly CrontabSchedule _cronNotation;

        public DateTime Next
        {
            get
            {
                var baseTime = DateTime.UtcNow;
                if (_cronNotation != null) return _cronNotation.GetNextOccurrence(baseTime);

                var secondsAfterLastHappening = baseTime.Second % _second;
                var nextSecondHappening = baseTime.Second - secondsAfterLastHappening + _second;

                return nextSecondHappening == 60
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
                nextSecondHappening,
                0);
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

        public UpdateEveryAttribute(string cronNotation)
        {
            var schedule = CrontabSchedule.Parse(cronNotation);
            _cronNotation = schedule;
        }

        public UpdateEveryAttribute(int second)
        {
            if (second < 1 || second > 60) throw new InvalidTimeNotationException();

            _second = second;
        }

        public static UpdateEveryAttribute Default()
        {
            return new UpdateEveryAttribute(1);
        }
    }

    public class InvalidTimeNotationException : Exception
    {
        public InvalidTimeNotationException()
        : base($"Choose a valie between 1 and 60 seconds")
        {
        }
    }
}