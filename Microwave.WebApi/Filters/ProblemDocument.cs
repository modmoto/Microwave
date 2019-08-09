using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microwave.Domain.Validation;
using Newtonsoft.Json;

namespace Microwave.WebApi.Filters
{
    public class ProblemDocument
    {
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; }

        [JsonProperty("type")]
        public string Type { get; }

        [JsonProperty("status",NullValueHandling = NullValueHandling.Ignore)]
        public HttpStatusCode? Status { get; }

        [JsonProperty("detail", NullValueHandling = NullValueHandling.Ignore)]
        public string Detail { get; }

        [JsonProperty("problem-details", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ProblemDocument> ProblemDetails { get; }

        [JsonIgnore]
        public IEnumerable<DomainError> DomainErrors => ProblemDetails.Select(p => DomainError.Create(p.Type, p.Detail));

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
            IEnumerable<DomainError> domainErrors)
        {
            Type = type;
            Title = title;
            Status = HttpStatusCode.BadRequest;
            ProblemDetails =
                domainErrors.Select(error => new ProblemDocument(error.ErrorType, error.Detail));
        }

        [JsonConstructor]
        private ProblemDocument(
            string title,
            string type,
            HttpStatusCode? status,
            string detail,
            IEnumerable<ProblemDocument> problemDetails)
        {
            Title = title;
            Type = type;
            Status = status;
            Detail = detail;
            ProblemDetails = problemDetails;
        }
    }
}