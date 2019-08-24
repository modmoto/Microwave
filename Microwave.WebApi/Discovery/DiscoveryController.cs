﻿using System;
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
            await _discoveryHandler.SubscribeForEvent(null, null);
            return Ok();
        }
    }
}