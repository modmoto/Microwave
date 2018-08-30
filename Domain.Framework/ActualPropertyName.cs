using System;

namespace Domain.Framework
{
    public class ActualPropertyName : Attribute
    {
        public string Path { get; }

        public ActualPropertyName(string path)
        {
            Path = path;
        }
    }
}