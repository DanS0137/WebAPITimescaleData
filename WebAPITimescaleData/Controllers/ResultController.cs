using Microsoft.AspNetCore.Mvc;
using WebAPITimescaleData.Model;

namespace WebAPITimescaleData.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResultController : ControllerBase
    {
        private readonly ILogger<ResultController> _logger;

        public ResultController(ILogger<ResultController> logger)
        {
            _logger = logger;
        }

        [HttpGet("/results/filterbyfilename/{filename}")]
        public Result GetResultsByFilename(string filename)
        {
            return new Result();
        }

        [HttpGet("/results/filterbystartdatetime/{startDiap}-{endDiap}")]
        public Result GetResultsByStartDateTime(DateTime startDiap, DateTime endDiap)
        {
            return new Result();
        }

        [HttpGet("/results/filterbyaveragevalue/{startDiap}-{endDiap}")]
        public Result GetResultsByAverageValue(double startDiap, double endDiap)
        {
            return new Result();
        }

        [HttpGet("/results/filterbyaverageexecutiontime/{startDiap}-{endDiap}")]
        public Result GetResultsByAverageExecutionTime(double startDiap, double endDiap)
        {
            return new Result();
        }
    }
}
