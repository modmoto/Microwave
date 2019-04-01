using System.Collections.Generic;

namespace Microwave.Application
{
    public class PublishedEventCollection : List<string>
    {
        public string ServiceName { get; set; }
    }
}