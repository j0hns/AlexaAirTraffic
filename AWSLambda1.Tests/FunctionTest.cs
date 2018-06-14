using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;

using AWSLambda1;
using Newtonsoft.Json;

namespace AWSLambda1.Tests
{
    public class FunctionTest
    {
        [Fact]
        public void TestToUpperFunction()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Function();
            var context = new TestLambdaContext();
            var response = function.FunctionHandler(GetNearbySkillsRequest(), context);

            //Assert.Equal(""HELLO WORLD"", upperCase);
        }

        [Fact]
        public void TestNearbyAircraft()
        {

            // Invoke the lambda function and confirm the string was upper cased.
            var function = new Function();
            var context = new TestLambdaContext();
            var output = new SsmlOutputSpeech();
           var speech= function.HandleNearbyAircraftIntent(new IntentRequest{Intent=new Intent()});

            //Assert.Equal(""HELLO WORLD"", upperCase);
        }


        private static SkillRequest GetNearbySkillsRequest()
        {
            var request = @"{
 ""version"": ""1.0"",
 ""session"": {
                ""new"": true,
  ""sessionId"": ""amzn1.echo-api.session.xxx"",
  ""application"": {
                    ""applicationId"": ""amzn1.ask.skill.xxx""
  },
  ""user"": {
                    ""userId"": ""amzn1.ask.account.xxx""
  }
            },
 ""context"": {
                ""AudioPlayer"": {
                    ""playerActivity"": ""IDLE""
                },
  ""Display"": { },
  ""System"": {
                    ""application"": {
                        ""applicationId"": ""amzn1.ask.skill.7xxx""
                    },
   ""user"": {
                        ""userId"": ""amzn1.ask.account.xxx""
   },
   ""device"": {
                        ""deviceId"": ""amzn1.ask.device.xxx"",
    ""supportedInterfaces"": {
                            ""AudioPlayer"": { },
     ""Display"": {
                                ""templateVersion"": ""1.0"",
      ""markupVersion"": ""1.0""
     }
                        }
                    },
   ""apiEndpoint"": ""https://api.eu.amazonalexa.com"",
   ""apiAccessToken"": ""xxx""
  }
            },
 ""request"": {
                ""type"": ""IntentRequest"",
  ""requestId"": ""amzn1.echo-api.request.xxx"",
  ""timestamp"": ""2018-06-07T21:23:46Z"",
  ""locale"": ""en-GB"",
  ""intent"": {
                    ""name"": ""AIRTRAFFICnearbyAircraft"",
   ""confirmationStatus"": ""NONE""
  }
            }
        }";


            return JsonConvert.DeserializeObject<SkillRequest>(request);
        }
    }
}
