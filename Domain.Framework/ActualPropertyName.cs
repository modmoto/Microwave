using System;
using System.Text.RegularExpressions;

namespace Domain.Framework
{
    public class ActualPropertyName : Attribute
    {
        public string[] Path { get; }

        public ActualPropertyName(string path)
        {
            var doesNotStartOrEndWithDots = new Regex("^[^.].*[^.]$");
            var match = doesNotStartOrEndWithDots.Match(path);
            if (!match.Success) throw new ArgumentException($"{nameof(path)} can not start or end with the character: \".\"");
            var split = path.Split(".");
            if (split.Length == 0) Path = new[] {path};
            Path = split;
        }
    }
}