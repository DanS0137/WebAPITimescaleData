using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System.Globalization;
using WebAPITimescaleData.Data;
using WebAPITimescaleData.Model;

namespace WebAPITimescaleData.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly WebApiAppDbContext _context;

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

            var config = new CsvConfiguration(CultureInfo.InvariantCulture);
            config.Delimiter = ";";
            config.MissingFieldFound = null;
            config.HeaderValidated = null;

            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<Record>().ToList();
            int recordsCount = records.Count;

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
                records.ElementAt(i).FileName = csvFile.FileName;
            }

            Result? result = _context.Results.FirstOrDefault(v => v.FileName == csvFile.FileName);
            if (result != null)
            {
                _context.Results.Remove(result);
                var values = _context.Values.Where(v =>  v.FileName == csvFile.FileName).ToList();
                _context.Values.RemoveRange(values);
            }

            Result newResult = new Result
            {
                FileName = csvFile.FileName,
                TimeDelta = (records.Max(x => x.Date) - records.Min(x => x.Date)).TotalSeconds,
                StartDateTime = records.Min(x => x.Date),
                AverageExecutionTime = records.Average(x => x.ExecutionTime),
                AverageValue = records.Average(x => x.Value),
                MaxValue = records.Max(x => x.Value),
                MinValue = records.Min(x => x.Value)
            };
            var orderedByValueRecords = records.OrderBy(x => x.Value);
            if (recordsCount % 2 == 0)
            {
                newResult.MedianValue = (orderedByValueRecords.ElementAt(recordsCount / 2).Value +
                    orderedByValueRecords.ElementAt((recordsCount - 1) / 2).Value) / 2;
            }
            else
            {
                newResult.MedianValue = orderedByValueRecords.ElementAt(recordsCount / 2).Value;
            }

            _context.Values.AddRange(records);
            _context.Results.Add(newResult);
            _context.SaveChanges();
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

        public ValuesController(WebApiAppDbContext context)
        {
            _context = context;
        }
    }
}
