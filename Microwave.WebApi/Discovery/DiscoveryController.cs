using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microwave.Discovery;

namespace Microwave.WebApi.Discovery
{
    [Route("Dicovery")]
    public class DiscoveryController : Controller
    {
        private readonly IDiscoveryHandler _discoveryHandler;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DiscoveryController(IDiscoveryHandler discoveryHandler, IHttpContextAccessor httpContextAccessor)
        {
            _discoveryHandler = discoveryHandler;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("PublishedEvents")]
        public async Task<ActionResult> GetPublishedEvents()
        {
            var publishedEvents = await _discoveryHandler.GetPublishedEvents();
            var dto = new PublishedEventsByServiceDto();
            dto.PublishedEvents.AddRange(publishedEvents.PublishedEventTypes);
            dto.ServiceName = publishedEvents.ServiceEndPoint.Name;
            return Ok(dto);
        }

        [HttpGet("ConsumingServices")]
        public async Task<ActionResult> GetConsumingServices()
        {
            var consumingServices = await _discoveryHandler.GetConsumingServices();
            return Ok(consumingServices);
        }

        [HttpGet("ServiceDependencies")]
        public async Task<ActionResult> GetServiceDependencies()
        {
            var consumingServices = await _discoveryHandler.GetConsumingServiceNodes();
            return Ok(consumingServices);
        }

        [HttpPut("ServiceMap/Update")]
        public async Task<ActionResult> UpdateServiceMap()
        {
            await _discoveryHandler.DiscoverServiceMap();
            return Ok();
        }

        [HttpPut("ServiceMap")]
        public async Task<ActionResult> GetServiceMap()
        {
            var serviceMap = await _discoveryHandler.GetServiceMap();
            return Ok(serviceMap);
        }

        [HttpPut("ConsumingServices/Update")]
        public async Task<ActionResult> UpdateConsumingServices()
        {
            await _discoveryHandler.DiscoverConsumingServices();
            return Ok();
        }

        [HttpPut("SubscribeEvent")]
        public async Task<ActionResult> SubscribeEvent()
        {
            var requestIp = GetRequestIP();
            await _discoveryHandler.SubscribeForEvent(null, null);
            return Ok();
        }

        public string GetRequestIP(bool tryUseXForwardHeader = true)
        {
            string ip = null;

            // todo support new "Forwarded" header (2014) https://en.wikipedia.org/wiki/X-Forwarded-For

            // X-Forwarded-For (csv list):  Using the First entry in the list seems to work
            // for 99% of cases however it has been suggested that a better (although tedious)
            // approach might be to read each IP from right to left and use the first public IP.
            // http://stackoverflow.com/a/43554000/538763
            //
            if (tryUseXForwardHeader)
                ip = SplitCsv(GetHeaderValueAs<string>("X-Forwarded-For")).FirstOrDefault();

            // RemoteIpAddress is always null in DNX RC1 Update1 (bug).
            if (IsNullOrWhitespace(ip) && _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress != null)
                ip = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

            if (IsNullOrWhitespace(ip))
                ip = GetHeaderValueAs<string>("REMOTE_ADDR");

            // _httpContextAccessor.HttpContext?.Request?.Host this is the local host.

            if (IsNullOrWhitespace(ip))
                throw new Exception("Unable to determine caller's IP.");

            return ip;
        }

        public T GetHeaderValueAs<T>(string headerName)
        {
            StringValues values;

            if (_httpContextAccessor.HttpContext?.Request?.Headers?.TryGetValue(headerName, out values) ?? false)
            {
                string rawValues = values.ToString();   // writes out as Csv when there are multiple.

                if (!IsNullOrWhitespace(rawValues))
                    return (T)Convert.ChangeType(values.ToString(), typeof(T));
            }
            return default(T);
        }

        public static List<string> SplitCsv(string csvList, bool nullOrWhitespaceInputReturnsNull = false)
        {
            if (string.IsNullOrWhiteSpace(csvList))
                return nullOrWhitespaceInputReturnsNull ? null : new List<string>();

            return csvList
                .TrimEnd(',')
                .Split(',')
                .AsEnumerable()
                .Select(s => s.Trim())
                .ToList();
        }

        public static bool IsNullOrWhitespace(string s)
        {
            return String.IsNullOrWhiteSpace(s);
        }
    }
}