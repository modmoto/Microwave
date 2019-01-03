using System.Collections.Generic;
using System.Linq;
using Microwave.Domain;
using Newtonsoft.Json;

namespace Microwave.WebApi.Filters
{
    public class ProblemDocument
    {
        public string Key { get; }
        public string Detail { get; }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public IEnumerable<ProblemDocument> DomainErrors { get; }

        [JsonProperty(NullValueHandling=NullValueHandling.Ignore)]
        public IEnumerable<ProblemDocument> ApplicationErrors { get; }

        public ProblemDocument(string key, string detail)
        {
            Key = key;
            Detail = detail;
        }

        public ProblemDocument(string key, string detail, IEnumerable<DomainError> domainErrors)
        {
            Key = key;
            Detail = detail;
            DomainErrors =
                domainErrors.Select(error => new ProblemDocument(error.ErrorType, error.Description));
        }
    }
}