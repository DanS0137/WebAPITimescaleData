using Microsoft.AspNetCore.Mvc;
using WebAPITimescaleData.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPITimescaleData.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet("/last10values/{fileName}")]
        public Value[] GetLastValues(string fileName)
        {
            var result = new Value[10];
            return result;
        }

        [HttpPost("/upload")]
        public async Task<IActionResult> Upload(IFormFile csvFile)
        {

            return Ok();
        }
    }
}
