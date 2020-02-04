using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperTaxiBot
{
    public class CarBooking
    {
        public string Name { get; set; }
        public string PickupLocation { get; set; }
        public string DropOffLocation { get; set; }
        public string PickupTime { get; set; }
        public string PrimaryCar { get; set; }
        public string SecondaryCar { get; set; }
        public string InterestedInSurvey { get; set; }
        public int NpsScore { get; set; }
    }
}
