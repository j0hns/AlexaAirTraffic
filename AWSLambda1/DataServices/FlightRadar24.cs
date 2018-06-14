using System;
using System.Collections.Generic;
using System.Net;
using AWSLambda1.Geo;
using Newtonsoft.Json;

namespace AWSLambda1.DataServices
{
    //NGeoHash?

    //https://data-live.flightradar24.com/zones/fcgi/feed.js?bounds=52,5,-20,10&faa=1&mlat=1&flarm=1&adsb=1&gnd=1&air=1&vehicles=1&estimated=1&maxage=14400&gliders=1&stats=1&selected=119f9a3c&ems=1
    public class FlightRadar24
    {

        public static FlightRadarResponse GetFlights(double north, double south, double east, double west)
        {
            using (WebClient client = new WebClient())
            {
                string json = client.DownloadString(new Uri($"https://data-live.flightradar24.com/zones/fcgi/feed.js?bounds={north},{south},{west},{east}&faa=1&mlat=1&flarm=1&adsb=1&gnd=1&air=1&vehicles=1&estimated=1&maxage=14400&gliders=1&stats=1&selected=119f9a3c&ems=1"));
                var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                var response = new FlightRadarResponse();
                response.stats = JsonConvert.DeserializeObject<Stats>(dictionary["stats"].ToString());

                dictionary.Remove("stats");
                dictionary.Remove("selected-aircraft");
                dictionary.Remove("full_count");
                dictionary.Remove("version");

                response.Sightings = new List<Sighting>();
                foreach (var keyPair in dictionary)
                {
                    var sightingValues = JsonConvert.DeserializeObject<List<string>>(keyPair.Value.ToString());
                    var sighting = new Sighting();
                    sighting.Latitude = Convert.ToDouble(sightingValues[1]);
                    sighting.Longitude = Convert.ToDouble(sightingValues[2]);
                    sighting.Track = Convert.ToDouble(sightingValues[3]);
                    sighting.AltitudeFt = Convert.ToDouble(sightingValues[4]);
                    sighting.GroundSpeedKts = Convert.ToDouble(sightingValues[5]);
                    sighting.Departed = sightingValues[11];
                    sighting.Arrving = sightingValues[12];
                    sighting.FlightNumber = sightingValues[13];
                    sighting.CallSign = sightingValues[16];
                    sighting.AircraftType = sightingValues[8];

                    response.Sightings.Add(sighting);
                }

                return response;
            }

        }
    }

    public class Sighting
    {
        private GeoLocation? _location;
        public double GroundSpeedKts { get; set; }
        public double Track { get; set; }
        public double AltitudeFt { get; set; }
        public string Departed { get; set; }
        public string Arrving { get; set; }
        public DateTime ArrivalEst { get; set; }
        public DateTime DepartureAct { get; set; }
        public string FlightNumber { get; set; }
        public string CallSign { get; set; }
        public string AircraftType { get; set; }
        public string AircraftRegistration { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public GeoLocation Location
        {
            get
            {
                if (_location == null)
                {
                    _location = new GeoLocation(Latitude, Longitude);
                }

                return _location.Value;
            }


        }
    }

        public class Total
        {
            [JsonProperty("ads-b")]
            public int ads_b { get; set; }
            public int mlat { get; set; }
            public int faa { get; set; }
            public int flarm { get; set; }
            public int estimated { get; set; }
        }


        public class Visible
        {
            [JsonProperty("ads-b")]
            public int ads_b { get; set; }
            public int mlat { get; set; }
            public int faa { get; set; }
            public int flarm { get; set; }
            public int estimated { get; set; }

            public int Total
            {
                get { return ads_b + mlat + faa + flarm + estimated; }
            }
        }

        public class Stats
        {
            public Total total { get; set; }
            public Visible visible { get; set; }
        }

        public class AvailableEms
        {
        }

        public class Ems
        {
        }

        public class SelectedAircraft
        {
            [JsonProperty("available-ems")]
            public AvailableEms available_ems { get; set; }
            public Ems ems { get; set; }
        }

        public class FlightRadarResponse
        {
            public int full_count { get; set; }
            public int version { get; set; }

            //geo hash buckets
            public List<Sighting> Sightings { get; set; }

            public Stats stats { get; set; }
            [JsonProperty("selected-aircraft")]
            public SelectedAircraft selected_aircraft { get; set; }
        }
    }
