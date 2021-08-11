using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinuxQueueCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LinuxQueueApi.Controllers
{
    [Produces("text/plain")]
    [Route("api/Test")]
    public class TestController : Controller
    {


        [HttpGet]
        public string Get()
        {

            var test = "QueueFolders.rootPath : " + QueueFolders.rootPath + @"
QueueFolders.queueFolder : " + QueueFolders.queueFolder + @"
QueueFolders.runningFolder : " + QueueFolders.runningFolder + @"
QueueFolders.runlogFolder : " + QueueFolders.runlogFolder + @"
QueueFolders.finishedFolder : " + QueueFolders.finishedFolder;


            return test;

        }
    }
}