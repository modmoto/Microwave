using System;
using System.Runtime.CompilerServices;
using NCrontab;

[assembly: InternalsVisibleTo("Microwave.Queries.UnitTests")]
namespace Microwave.Queries
{
    public class UpdateEveryAttribute : Attribute
    {
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

        public UpdateEveryAttribute(int second = 1)
        {
            if (second < 1 || second > 60) throw new InvalidTimeNotationException();

            _second = second;
        }

        internal UpdateEveryAttribute(int secondsInput, int secondsForTest)
        {
            _nowTime = new DateTime(1, 1, 1, 1, 0, secondsForTest);
            _second = secondsInput;
        }

        public static UpdateEveryAttribute Default()
        {
            return new UpdateEveryAttribute();
        }
    }
}