using System;

namespace Domain.Framework
{
    public class ActualPropertyName : Attribute
    {
        public string[] Path { get; }

        public ActualPropertyName(string path)
        {
            var split = path.Split(".");
            if (split.Length == 0) Path = new[] {path};
            Path = split;
        }
    }
}