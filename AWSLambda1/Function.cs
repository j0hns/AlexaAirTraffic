using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using AWSLambda1.DataServices;
using AWSLambda1.Extensions;
using AWSLambda1.Geo;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSLambda1
{

    public class Function
    {

        //https://data-live.flightradar24.com/zones/fcgi/feed.js?bounds=52.19,51.84,0.72,1.65&faa=1&mlat=1&flarm=1&adsb=1&gnd=1&air=1&vehicles=1&estimated=1&maxage=14400&gliders=1&stats=1&selected=119f9a3c&ems=1



        /// <summary>
        /// Air Traffic Alexa custom skill entry point
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            try
            {
                context.Log("SkillRequest", JsonConvert.SerializeObject(input));
                context.Log("Context", JsonConvert.SerializeObject(context));

                SkillResponse response = null;

                // check what type of a request it is like an IntentRequest or a LaunchRequest
                var requestType = input.GetRequestType();

                if (requestType == typeof(IntentRequest))
                {
                    response = HandleIntentRequest(input, context);
                }
                else if (requestType == typeof(Alexa.NET.Request.Type.LaunchRequest))
                {
                    response = HandleLaunchRequest(input, context);
                }
                //else if (requestType == typeof(AudioPlayerRequest))
                //{
                //    return HandleAudioPlayerRequest(input, context);
                //}

                context.Log("ResponseBody", JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                context.Log("Error", "error :" + ex.Message);
            }
            return null;

        }

        private SkillResponse HandleLaunchRequest(SkillRequest input, ILambdaContext context)
        {
            // build the speech response 
            var speech = new Alexa.NET.Response.SsmlOutputSpeech
            {
                Ssml = "<speak>This is Air Traffic. Go ahead with your request, or ask for help</speak>"
            };
            // create the response using the ResponseBuilder
            var finalResponse = ResponseBuilder.Tell(speech);
            finalResponse.Response.ShouldEndSession = false;
            return finalResponse;

        }

        //private SkillResponse HandleAudioPlayerRequest(SkillRequest input, ILambdaContext context)
        //{
        //    // do some audio response stuff
        //    var audioRequest = input.Request as AudioPlayerRequest;

        //    // these are events sent when the audio state has changed on the device
        //    // determine what exactly happened
        //    if (audioRequest.AudioRequestType == AudioRequestType.PlaybackNearlyFinished)
        //    {
        //        // queue up another audio file
        //    }
        //}

        private SkillResponse HandleIntentRequest(SkillRequest input, ILambdaContext context)
        {
            // do some intent-based stuff
            var intentRequest = input.Request as IntentRequest;
            // build the speech response 
            var speech = new Alexa.NET.Response.SsmlOutputSpeech();
            // check the name to determine what you should do
            if (intentRequest.Intent.Name.Equals("AMAZON.HelpIntent"))
            {
                // build the speech response 

                speech.Ssml = "<speak>I can tell you about aircraft flying near your position or over a specific place. You can ask questions like what is nearby? how many flights are within 20 miles? and, what is south of me? You can also set your specific location for more accurate results.</speak>";
            }

            if (intentRequest.Intent.Name.Equals("AIRTRAFFICnearbyAircraft"))
            {
                speech = HandleNearbyAircraftIntent(intentRequest);
            }



            //with card response
            //var finalResponse = ResponseBuilder.TellWithCard(speech, "Your Card Title", "Your card content text goes here, no HTML formatting honored");

            // create the response using the ResponseBuilder
            var finalResponse = ResponseBuilder.Tell(speech);
            return finalResponse;

            //Build a simple response with a reprompt
            //// create the speech response
            //var speech = new Alexa.NET.Response.SsmlOutputSpeech();
            //speech.Ssml = "<speak>Today is <say-as interpret-as=\"date\">????0922</say-as>.</speak>";

            //// create the speech reprompt
            //var repromptMessage = new Alexa.NET.Response.PlainTextOutputSpeech();
            //repromptMessage.Text = "Would you like to know what tomorrow is?";

            //// create the reprompt
            //var repromptBody = new Alexa.NET.Response.Reprompt();
            //repromptBody.OutputSpeech = repromptMessage;

            //// create the response
            //var finalResponse = ResponseBuilder.Ask(speech, repromptBody);
            //return finalResponse;
        }

        public SsmlOutputSpeech HandleNearbyAircraftIntent(IntentRequest intentRequest)
        {
            // build the speech response 
            var speech = new Alexa.NET.Response.SsmlOutputSpeech();

            Slot distanceSlot = null;
            Slot distanceUnitsSlot = null;

            intentRequest.Intent.Slots?.TryGetValue("distance", out distanceSlot);
            intentRequest.Intent.Slots?.TryGetValue("distanceUnits", out distanceUnitsSlot);

            double distanceValue = distanceSlot != null ? Convert.ToDouble(distanceSlot.Value) : 20;
            string distanceUnitsValue = distanceUnitsSlot != null ? distanceUnitsSlot.Value : "Kilometres";

            var distance = Distance.FromKilometres(distanceValue);

            var location = new GeoLocation(52.041808, 1.208131);
            var north = GeoLocation.FindPointAtDistanceFrom(location, Angle.FromDegrees(0), distance);
            var south = GeoLocation.FindPointAtDistanceFrom(location, Angle.FromDegrees(180), distance);
            var east = GeoLocation.FindPointAtDistanceFrom(location, Angle.FromDegrees(90), distance);
            var west = GeoLocation.FindPointAtDistanceFrom(location, Angle.FromDegrees(270), distance);
            var flights = FlightRadar24.GetFlights(north.Latitude, south.Latitude, east.Longitude, west.Longitude);

            if (flights == null)
            {
                speech.Ssml = "I'm unable to contact Flight Radar.";
                return speech;
            }

            var withinRangeOrderedByClosest = (from sighting in flights.Sightings
                                               let d = GeoLocation.DistanceBetween(sighting.Location, location)
                                               where d.Kilometres < distance.Kilometres
                                               orderby d.Kilometres descending
                                               select new { sighting, d }).ToArray();



            if (withinRangeOrderedByClosest.Length == 0)
            {
                speech.Ssml = "There are no flights within {distanceValue} {distanceUnitsValue}.";
                return speech;
            }

            var closest = withinRangeOrderedByClosest.First();
            var bearing = GeoLocation.RhumbBearing(location, closest.sighting.Location);

            var id = AddIndefiniteArticle(closest.sighting.AircraftType) + (closest.sighting.FlightNumber != null
                ? $" flight {closest.sighting.FlightNumber}"
                : "");
            var destination = closest.sighting.Arrving != null ? $" to {closest.sighting.Arrving}" : "";
            speech.Ssml = $"<speak>There are {withinRangeOrderedByClosest.Length} within {distanceValue:N0} {distanceUnitsValue}. The closest is {id} {destination}, range {closest.d.Miles:N0} miles, bearing {bearing:N0}, at altitude {closest.sighting.AltitudeFt:N0} feet and {closest.sighting.GroundSpeedKts} knots</speak>";
            return speech;


        }

        private bool IsVowel(char c)
        {
            return "aeiouAEIOU".IndexOf(c) >= 0;
        }

        private string AddIndefiniteArticle(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
                
            return IsVowel(value[0]) ? "an {value}" : "a {value}";
        }

    }


}
