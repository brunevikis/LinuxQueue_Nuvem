using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinuxQueueCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinuxQueueApi.Controllers
{
    [Route("api/[controller]")]
    public class CommandController : Controller
    {

        public static QueueController queueCtl = new QueueController();

        [HttpGet]
        public IEnumerable<CommItem> Get()
        {
            var comms = queueCtl.ReadComms();
            
            return comms;//.Select(x => new LinuxQueueApi.Model.Command { Name = System.IO.Path.GetFileName(x), Folder = System.IO.Path.GetDirectoryName(x) });
        }

        [HttpGet("{name}")]
        [ProducesResponseType(200, Type = typeof(string))]
        public IActionResult Get(string name)
        {
            var comms = queueCtl.ReadComms();

           
            
            if (name == "queue" || name == "running" || name == "finished")
            {
                var esp_comms = comms.Where(x => x.Folder.ToString().Contains("queuectl/" + name));


                return Json(esp_comms);

            }
            else if (name.Contains("PLD_Mensal"))
            {
                var caso = name.Split('-').Last();
                var dados = Program.Get_Resultado(caso);

                return Json(dados);
            }
            else
            {

                var l = comms.Where(x => x.CommandName == name).FirstOrDefault();

                if (l != null)
                {

                    var txt = "";
                    if (System.IO.File.Exists(l.LogFile))
                    {
                        txt = System.IO.File.ReadAllText(l.LogFile);
                        txt = txt.Replace("\n", "\r\n").Replace("\r\r\n", "\r\n");
                    }
                    else
                    {
                        txt = "Log not found\r\n" + l.LogFile;
                    }
                    return Ok(txt);
                }
                else return NotFound();
            }

        }



        [HttpPost]
        public IActionResult Post([FromBody]CommItem comm)
        {
            try
            {
                if (comm.Command == null || comm.WorkingDirectory == null || comm.User == null)
                {
                    throw new ArgumentException();
                }

                comm.Cluster = QueueController.Clusters[0];

                queueCtl.Enqueue(comm);
                return base.CreatedAtAction("Get", comm.CommandName);
            }
            catch
            {
                return base.BadRequest();
            }
        }


        [HttpDelete("{name}")]
        public IActionResult Delete(string name)
        {
            var comms = queueCtl.ReadComms();
            var l = comms.Where(x => x.CommandName == name).FirstOrDefault();

            if (l != null)
            {
                queueCtl.Cancel(l, l.User);
                return base.Ok();
            }
            else return NotFound();
        }

        [HttpPut]
        public IActionResult Put([FromBody]CommItem comm)
        {

            try
            {
                if (comm.Command == null || comm.WorkingDirectory == null || comm.User == null)
                {
                    throw new ArgumentException();
                }

                var comms = queueCtl.ReadComms();
                var l = comms.Where(x => x.CommandName == comm.CommandName).Where(x => x.FolderType == QueueFolderEnum.Queue).FirstOrDefault();

                if (l == null)
                {
                    return base.BadRequest();
                }

                l.Cluster = comm.Cluster;
                l.Order = comm.Order;

                comm.SaveChanges();

                return base.AcceptedAtAction("Get", comm.CommandName);
            }
            catch
            {
                return base.BadRequest();
            }
        }
    }
}