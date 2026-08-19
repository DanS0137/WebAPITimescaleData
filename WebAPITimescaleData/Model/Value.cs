namespace WebAPITimescaleData.Model
{
    public class Value
    {
        public string FileName { get; set; }

        public double Val { get; set; }

        public DateTime StartDateTime { get; set; }

        public double ExecutionTime { get; set; }
    }
}
