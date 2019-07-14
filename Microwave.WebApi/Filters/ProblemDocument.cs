using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microwave.Domain.Validation;
using Newtonsoft.Json;

namespace Microwave.WebApi.Filters
{
    public class ProblemDocument
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; }
        public string Type { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public HttpStatusCode? Status { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Detail { get; }

        [JsonProperty("problem-details", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ProblemDocument> ProblemDetails { get; }

        public ProblemDocument(
            string type,
            string title,
            HttpStatusCode status,
            string detail)
        {
            Title = title;
            Detail = detail;
            Type = type;
            Status = status;
        }

        private ProblemDocument(
            string type,
            string detail)
        {
            Detail = detail;
            Type = type;
        }

        public ProblemDocument(
            string type,
            string title,
            HttpStatusCode status,
            IEnumerable<DomainError> domainErrors)
        {
            Type = type;
            Title = title;
            Status = status;
            ProblemDetails =
                domainErrors.Select(error => new ProblemDocument(error.ErrorType, error.Description));
        }
    }
}