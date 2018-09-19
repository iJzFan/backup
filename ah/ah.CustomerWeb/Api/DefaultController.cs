using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ahWeb.Api
{
    [Produces("application/json")]
    [Route("api/Default")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class Default : Controller
    {
        // GET: api/Default
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Default/5
        [HttpGet("{id}", Name = "Get")]
        public object Get(int id)
        {
            return  new { type = "Get", Id = id ,s="1234134",DateTime=DateTime.Now};
        }
        
        // POST: api/Default
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
        
        // PUT: api/Default/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }
        
        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
