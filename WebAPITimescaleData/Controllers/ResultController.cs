using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPITimescaleData.Data;
using WebAPITimescaleData.Model;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebAPITimescaleData.Controllers
{
    /// <summary>
    /// Класс для методов, направленных на работу с таблицей Results.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ResultController : ControllerBase
    {
        /// <summary>
        /// База данных.
        /// </summary>
        private readonly WebApiAppDbContext _context;
        /*
        /// <summary>
        /// Метод для фильтрации записей таблицы Results по имени файла.
        /// </summary>
        /// <param name="filename">Имя файла, записи из которого нужно получить.</param>
        /// <returns>Возвращает массив записей с результатами.</returns>
        [HttpGet("/results/filterbyfilename/{filename}")]
        public Result[] GetResultsByFilename(string filename)
        {
            return _context.Results.Where(r => r.FileName == filename).ToArray();
        }

        /// <summary>
        /// Метод для фильтрации записей таблицы Results по времени запуска первой операции.
        /// </summary>
        /// <param name="startDiap">Начало диапазона, в котором будет вестись поиск записей.</param>
        /// <param name="endDiap"> Конец диапазона, в котором будет вестись поиск записей.</param>
        /// <returns>Возвращает массив записей с результатами, удовлетворяющим условиям фильтра.
        /// Если в метод будет передана значение начала диапазона большее, чем его конец, то вернёт null.</returns>
        [HttpGet("/results/filterbystartdatetime/{startDiap}-{endDiap}")]
        public Result[] GetResultsByStartDateTime(DateTime startDiap, DateTime endDiap)
        {
            if (startDiap > endDiap) return null;
            return _context.Results.Where(r => (r.StartDateTime < endDiap && r.StartDateTime > startDiap)).ToArray();
        }

        /// <summary>
        /// Метод для фильтрации записей таблицы Results по среднему значению показателя.
        /// </summary>
        /// <param name="startDiap">Начало диапазона, в котором будет вестись поиск записей.</param>
        /// <param name="endDiap"> Конец диапазона, в котором будет вестись поиск записей.</param>
        /// <returns>Возвращает массив записей с результатами, удовлетворяющим условиям фильтра.
        /// Если в метод будет передана значение начала диапазона большее, чем его конец, то вернёт null.</returns>
        [HttpGet("/results/filterbyaveragevalue/{startDiap}-{endDiap}")]
        public Result[] GetResultsByAverageValue(double startDiap, double endDiap)
        {
            if (startDiap > endDiap) return null;
            return _context.Results.Where(r => (r.AverageValue < endDiap && r.AverageValue > startDiap)).ToArray();
        }

        /// <summary>
        /// Метод для фильтрации записей таблицы Results по среднему значению времени выполнения.
        /// </summary>
        /// <param name="startDiap">Начало диапазона, в котором будет вестись поиск записей.</param>
        /// <param name="endDiap"> Конец диапазона, в котором будет вестись поиск записей.</param>
        /// <returns>Возвращает массив записей с результатами, удовлетворяющим условиям фильтра.
        /// Если в метод будет передана значение начала диапазона большее, чем его конец, то вернёт null.</returns>
        [HttpGet("/results/filterbyaverageexecutiontime/{startDiap}-{endDiap}")]
        public Result[] GetResultsByAverageExecutionTime(double startDiap, double endDiap)
        {
            return _context.Results.Where(r => (r.AverageExecutionTime < endDiap && r.AverageExecutionTime > startDiap)).ToArray();
        }*/

        /// <summary>
        /// Метод фильтрации записей таблицы Results.
        /// </summary>
        /// <param name="filename">Название файла. Необязательный параметр.</param>
        /// <param name="dateFrom">Левая граница диапазона времени запуска первой операции. Необязательный параметр.</param>
        /// <param name="dateTo">Правая граница диапазона времени запуска первой операции. Необязательный параметр.</param>
        /// <param name="minValue">Левая граница диапазона среднего значения показателя. Необязательный параметр.</param>
        /// <param name="maxValue">Правая граница диапазона среднего значения показателя. Необязательный параметр.</param>
        /// <param name="minExecTime">Левая граница диапазона среднего времени выполнения. Необязательный параметр.</param>
        /// <param name="maxExecTime">Правая граница диапазона среднего времени выполнения.Необязательный параметр.</param>
        /// <returns>Возвращает ошибки, если левая граница какого-либо диапазона больше правой.
        /// И возвращает коллекцию записей таблицы Results, если всё прошло успешно.</returns>
        [HttpGet]
        public async Task<IActionResult> Get(
                    string? filename = null,
                    DateTime? dateFrom = null,
                    DateTime? dateTo = null,
                    double? minValue = null,
                    double? maxValue = null,
                    double? minExecTime = null,
                    double? maxExecTime = null)
        {
            if (dateFrom.HasValue && dateTo.HasValue && dateFrom > dateTo)
            {
                return BadRequest("Левая граница диапазона времени не может быть больше правой.");
            }
            if (minValue.HasValue && maxValue.HasValue && minValue > maxValue)
            {
                return BadRequest("Левая граница диапазона среднего значения показателя не может быть больше правой.");
            }
            if (minExecTime.HasValue && maxExecTime.HasValue && minExecTime > maxExecTime)
            {
                return BadRequest("Левая граница диапазона среднего значения времени выполнения не может быть больше правой.");
            }

            var query = _context.Results.AsQueryable();

            if (!string.IsNullOrEmpty(filename))
            {
                query = query.Where(x => x.FileName == filename);
            }
            if (dateFrom.HasValue)
            {
                query = query.Where(x => x.StartDateTime >= dateFrom.Value);
            }
            if (dateTo.HasValue)
            {
                query = query.Where(x => x.StartDateTime <= dateTo.Value);
            }
            if (minValue.HasValue)
            {
                query = query.Where(x => x.AverageValue >= minValue.Value);
            }
            if (maxValue.HasValue)
            {
                query = query.Where(x => x.AverageValue <= maxValue.Value);
            }
            if (minExecTime.HasValue)
            {
                query = query.Where(x => x.AverageExecutionTime >= minExecTime.Value);
            }
            if (maxExecTime.HasValue)
            {
                query = query.Where(x => x.AverageExecutionTime <= maxExecTime.Value);
            }
            var records = await query.ToListAsync();

            return Ok(records);
        }

        /// <summary>
        /// Конструктор класса.
        /// </summary>
        /// <param name="context">Ссылка на представление базы данных.</param>
        public ResultController(WebApiAppDbContext context)
        {
            _context = context;
        }
    }
}
