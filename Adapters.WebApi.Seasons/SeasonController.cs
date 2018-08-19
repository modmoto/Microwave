using System;
using System.Collections.Generic;
using Domain.Seasons;
using Microsoft.AspNetCore.Mvc;

namespace Adapters.WebApi.Seasons
{
    [Route("api/seasons")]
    public class SeasonController : Controller
    {
        [HttpGet]
        public List<Season> GetSeasons()
        {
            return new List<Season> { new Season { Id = Guid.NewGuid()}};
        }
    }
}