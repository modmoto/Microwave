using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Domain.Framework
{
    public class PropertyPath : Attribute
    {
        public string Path { get; }
        public string PropetyName => Path.Split(".").Last();
        public IEnumerable<string> CascadingProperties
        {
            get
            {
                var splitPath = Path.Split(".");
                return splitPath.Take(splitPath.Length - 1);
            }
        }

        public PropertyPath(string path)
        {
            var doesNotStartOrEndWithDots = new Regex("^[^.].*[^.]$");
            var match = doesNotStartOrEndWithDots.Match(path);
            if (!match.Success) throw new ArgumentException($"{nameof(path)} can not start or end with the character: \".\"");
            Path = path;
        }
    }
}