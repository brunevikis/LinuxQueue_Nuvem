using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LinuxQueueApi.Model;
using LinuxQueueCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinuxQueueApi.Controllers
{
    [Produces("application/json")]
    [Route("api/Config")]
    public class ConfigController : Controller
    {
        // GET: api/Config
        [HttpGet]
        public List<CommItem> Get()
        {
            var configFile = Path.Combine(LinuxQueueCore.QueueFolders.rootPath, "configAuto");
            var cont = System.IO.File.ReadAllText(configFile);

            var comm = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CommItem>>(cont);

            return comm;
        }

        // PUT: api/Config    
        [HttpPut]
        public void Put([FromBody]List<CommItem> comm)
        {

            //comm.Linuxize();


            var cont = Newtonsoft.Json.JsonConvert.SerializeObject(comm);

            var configFile = Path.Combine(LinuxQueueCore.QueueFolders.rootPath, "configAuto");

            System.IO.File.WriteAllText(configFile, cont);
        }
    }
}
