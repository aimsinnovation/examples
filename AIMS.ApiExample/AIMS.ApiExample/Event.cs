using System;

namespace AIMS.ApiExample
{
    public class Event
    {
        public long Id { get; set; }

        public DateTime Time { get; set; }

        public string Type { get; set; }

        public string Status { get; set; }

        public string Text { get; set; }
    }
}