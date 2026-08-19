namespace WebAPITimescaleData.Model
{
    public class Result
    {
        public string FileName { get; set; }

        public double TimeDelta { get; set; }

        public DateTime StartDateTime { get; set; }

        public double AverageExecutionTime { get; set; }

        public double AverageValue { get; set; }

        public double MedianValue { get; set; }

        public double MaxValue { get; set; }

        public double MinValue { get; set; }
    }
}
