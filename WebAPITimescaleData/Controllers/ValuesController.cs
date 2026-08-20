using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebAPITimescaleData.Model;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPITimescaleData.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet("/last10values/{fileName}")]
        public Record[] GetLastValues(string fileName)
        {
            var result = new Record[10];
            return result;
        }

        [HttpPost("/upload")]
        public async Task<IActionResult> Upload(IFormFile csvFile)
        {
            if (csvFile == null || csvFile.Length == 0)
                return BadRequest("Файл не выбран");

            using var stream = csvFile.OpenReadStream();
            using var reader = new StreamReader(stream);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            };

            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<Record>();
            int recordsCount = records.Count();

            if (recordsCount > 10000)
            {
                return BadRequest("Файл содержит больше 10000 строк");
            }

            for (int i = 0; i < recordsCount; i++)
            {
                string validError = IsRecordValid(records.ElementAt(i));
                if (validError != string.Empty)
                {
                    return BadRequest(validError + $"в строке {i}");
                }
            }

            Result result = new Result();
            result.FileName = csvFile.FileName;
            result.TimeDelta = (records.Max(x => x.Date) - records.Min(x => x.Date)).TotalSeconds;
            result.StartDateTime = records.Min(x => x.Date);
            result.AverageExecutionTime = records.Average(x => x.ExecutionTime);
            result.AverageValue = records.Average(x => x.Value);
            result.MaxValue = records.Max(x => x.Value);
            result.MinValue = records.Min(x => x.Value);
            var orderedByValueRecords = records.OrderBy(x => x.Value);
            if (recordsCount % 2 == 0)
            {
                result.MedianValue = (orderedByValueRecords.ElementAt(recordsCount / 2).Value +
                    orderedByValueRecords.ElementAt((recordsCount - 1) / 2).Value) / 2;
            }
            else
            {
                result.MedianValue = orderedByValueRecords.ElementAt(recordsCount / 2).Value;
            }
            return Ok();
        }

        private string IsRecordValid(Record record) 
        {
            DateTime startDiap = new DateTime(2000, 1, 1, 0, 0, 0);
            DateTime now = DateTime.Now;
            if (startDiap > record.Date || record.Date > now)
            {
                return "Некорректное время начала ";
            }
            if (record.ExecutionTime < 0)
            {
                return "Отрицательное значение времени выполнения";
            }
            if (record.Value < 0)
            {
                return "Отрицательное значения показателя";
            }
            return string.Empty;
        }
    }
}
