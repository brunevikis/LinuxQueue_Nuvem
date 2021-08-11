//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using LinuxQueueApi.Model;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace LinuxQueueApi.Controllers
//{
//    [Route("api/[controller]")]
//    public class DiskController : Controller
//    {
//        // GET: api/Disk
//        [HttpGet]
//        public IEnumerable<Disk> Get()
//        {

//            var drs = System.IO.DriveInfo.GetDrives();

//            var disks = drs.Where(d=>d.Name.Contains("/home/producao")).Select(d => new Disk {
//                Name = d.VolumeLabel,
//                AvailableFreeSpace = d.AvailableFreeSpace/(1024*1024),
//                TotalSize = d.TotalSize/ (1024 * 1024)
//            });

//            return disks;
//        }

//        //// GET: api/Disk/5
//        //[HttpGet("{id}", Name = "Get")]
//        //public string Get(int id)
//        //{
//        //    return "value";
//        //}
        
//        //// POST: api/Disk
//        //[HttpPost]
//        //public void Post([FromBody]string value)
//        //{
//        //}
        
//        //// PUT: api/Disk/5
//        //[HttpPut("{id}")]
//        //public void Put(int id, [FromBody]string value)
//        //{
//        //}
        
//        //// DELETE: api/ApiWithActions/5
//        //[HttpDelete("{id}")]
//        //public void Delete(int id)
//        //{
//        //}
//    }
//}
