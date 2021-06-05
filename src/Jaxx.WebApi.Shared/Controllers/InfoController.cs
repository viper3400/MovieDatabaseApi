using Jaxx.WebApi.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Jaxx.WebApi.Shared.Controllers
{
    [Route("/[controller]")]
    [Authorize]
    public class InfoController : Controller
    {
        private readonly ILogger _logger;

        public InfoController(ILogger<InfoController> logger)
        {           
            _logger = logger;
        }

        /**
        * @api {get} /info Get informational data from api server
        * @apiVersion 1.2.1
        * @apiName Get Server Infos
        * @apiGroup Server
        * 
        * 
        * @apiExample Example usage:
        * http://localhost:50647/info
        * 
        * @apiHeader {String} Content-Type Request type, must be "application/json".
        * @apiHeader {String} Authorization You need to provide a token (see Authorization): "Bearer [TOKEN]".
        * @apiHeaderExample {json} Request-Example:
        * {
        *   "Content-Type": "application/json"
        *   "Authorization": "Bearer ewrjfjfoweffefo98098"
        * }
        * @apiError 401 Unauthorized
        * @apiSuccessExample Success-Response:
        *     HTTP/1.1 200 OK
        *  {
        *  	 "ApiServerVersion" : "1.2.1"
        *  }
        */
        [HttpGet]
        public IActionResult Index()
        {
            var ver = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            var info = new InfoRessource { ApiServerVersion = ver };
            var json = JsonConvert.SerializeObject(info);
            return Ok(json);
        }

    }
}
