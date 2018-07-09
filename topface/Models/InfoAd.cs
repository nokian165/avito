using System;

namespace topface.Models
{
    public class InfoAd
    {
        public int Id { get; set; }

        public string AdId { get; set; }

        public string Rooms { get; set; }

        public int Price { get; set; }

        public float Area { get; set; }

        public int Floor { get; set; }

        public int Floors { get; set; }

        public string Address { get; set; }

        public string NearestMetro { get; set; }

        public int NearestMetroDistance { get; set; }

        public DateTime AdAdded { get; set; }

        public string Description { get; set; }

        public long TelNumber { get; set; }

    }
}