using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using AWSLambda1.Extensions;
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
                context.Log("SkillRequest",JsonConvert.SerializeObject(input));
                context.Log("Context",JsonConvert.SerializeObject(context));

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

                context.Log("ResponseBody",JsonConvert.SerializeObject(response));
                return response;
            }
            catch (Exception ex)
            {
                context.Log("Error","error :" + ex.Message);
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
                
                speech.Ssml = "<speak>I can tell you about aircraft flying near your position or over a specific place. You can ask questions like what is nearby? how many flights are within 20 miles? and, what is south of me? You can also set your specific location for more accurate results.</speak>";
            }

            if (intentRequest.Intent.Name.Equals("AIRTRAFFICnearbyAircraft"))
            {
                // get the slots
                var firstValue = intentRequest.Intent.Slots?["flight"]?.Value;
                speech.Ssml = "<speak>I currently have no chuffing idea what aircraft are nearby as I am not finished yet.</speak>";
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






    }


}
