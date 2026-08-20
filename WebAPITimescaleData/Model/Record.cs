namespace WebAPITimescaleData.Model
{
    public class Record
    {
        public string FileName { get; set; }

        public double Value { get; set; }

        public DateTime Date { get; set; }

        public double ExecutionTime { get; set; }
    }
}
