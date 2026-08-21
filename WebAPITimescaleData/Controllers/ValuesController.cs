using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WebAPITimescaleData.Data;
using WebAPITimescaleData.Model;

namespace WebAPITimescaleData.Controllers
{
    /// <summary>
    /// Класс для методов, направленных на работу с таблицей Values.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        /// <summary>
        /// База данных.
        /// </summary>
        private readonly WebApiAppDbContext _context;

        /// <summary>
        /// Возвращает последние 10 значений, отсортированных по
        /// начальному времени запуска Date по имени заданного файла.
        /// </summary>
        /// <param name="fileName">Имя файла.</param>
        /// <returns>Вернёт сообщение об ошибке, если не получится
        /// получить записи с заданным именем файла.</returns>
        [HttpGet("/last10values/{fileName}")]
        public async Task<IActionResult> GetLastValues(string fileName)
        {
            var records = _context.Values
                                    .Where(v => v.FileName == fileName)
                                    .OrderByDescending(o => o.Date)
                                    .ToArray();
            if (records.Length == 0) 
            { 
                return BadRequest("Некорректное имя файла или записи с этим именем отсутствуют."); 
            }
            return Ok(records[0..10]);
        }

        /// <summary>
        /// Добавляет новые записи в базу данных или изменяет старые.
        /// </summary>
        /// <param name="csvFile">Файл формата csv.</param>
        /// <returns>Возвращает сообщение об ошибке, если какая-то запись или файл не проходит валидацию.</returns>
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

            //Валидация полей.
            for (int i = 0; i < recordsCount; i++)
            {
                string validError = IsRecordValid(records.ElementAt(i));
                if (validError != string.Empty)
                {
                    return BadRequest(validError + $" в строке {i}");
                }
                records.ElementAt(i).FileName = csvFile.FileName;
            }

            //Если в базе данных уже есть записи с именем загруженного файла, то удаляем их.
            Result? result = _context.Results.FirstOrDefault(v => v.FileName == csvFile.FileName);
            if (result != null)
            {
                _context.Results.Remove(result);
                var values = _context.Values.Where(v =>  v.FileName == csvFile.FileName).ToList();
                _context.Values.RemoveRange(values);
            }

            // Считаем интегральные значения и формируем новую запись в таблицу Result
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
            //Медиана.
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
            //Добавляем записи в таблицы и сохраняем изменения.
            _context.Values.AddRange(records);
            _context.Results.Add(newResult);
            _context.SaveChanges();
            return Ok();
        }

        /// <summary>
        /// Проверяет валидность записи csv-файла.
        /// </summary>
        /// <param name="record">Запись csv-файла</param>
        /// <returns>Строка с описанием ошибки валидации, если она произошла,
        /// иначе - string.Empty.</returns>
        private string IsRecordValid(Record record) 
        {
            DateTime startDiap = new DateTime(2000, 1, 1, 0, 0, 0);
            DateTime now = DateTime.Now;
            if (startDiap > record.Date || record.Date > now)
            {
                return "Некорректное время начала";
            }
            if (record.ExecutionTime <= 0)
            {
                return "Неположительное или отсутствующее значение времени выполнения";
            }
            if (record.Value <= 0)
            {
                return "Неположительное или отсутствующее значение показателя";
            }
            return string.Empty;
        }

        public ValuesController(WebApiAppDbContext context)
        {
            _context = context;
        }
    }
}
