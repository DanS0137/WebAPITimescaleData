using System.ComponentModel.DataAnnotations;

namespace WebAPITimescaleData.Model
{
    public class Record
    {
        public int Id { get; set; }

        public string FileName { get; set; }

        public double Value { get; set; }

        public DateTime Date { get; set; }

        public double ExecutionTime { get; set; }
    }
}
