using System;

namespace Domain.Framework
{
    public class PropertyPath : Attribute
    {
        public string Path { get; }

        public PropertyPath(string path)
        {
            Path = path;
        }
    }
}