namespace FoodTruckApi.Models
{
    using Microsoft.Azure.Documents.Spatial;
    using Newtonsoft.Json;
    
    public class FoodTruck
    {
        [JsonProperty(PropertyName = "id")]
        public string locationid { get; set; }
        public string Applicant { get; set; }
        public string FacilityType { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
        public string FoodItems { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        [JsonProperty(PropertyName = "location")]
        public Point Location { get; set; }
        public string Schedule { get; set; }
        public string dayshours { get; set; }
    }
}