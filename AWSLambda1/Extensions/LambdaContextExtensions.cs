using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Amazon.Lambda.Core;

namespace AWSLambda1.Extensions
{
    public static class LambdaContextExtensions
    {
        public static void Log(this ILambdaContext context,string header, string text)
        {
            context.Logger.LogLine(header + " " + text);
        }
    }
}
