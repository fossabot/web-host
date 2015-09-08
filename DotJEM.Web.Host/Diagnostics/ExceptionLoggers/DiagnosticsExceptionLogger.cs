using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using DotJEM.Web.Host.Util;
using Newtonsoft.Json.Linq;

namespace DotJEM.Web.Host.Diagnostics.ExceptionLoggers
{
    public class DiagnosticsExceptionLogger : IExceptionLogger
    {
        private readonly IDiagnosticsLogger logger;
        private readonly IJsonConverter converter;

        public DiagnosticsExceptionLogger(IDiagnosticsLogger logger, IJsonConverter converter)
        {
            this.logger = logger;
            this.converter = converter;
        }

        public Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            ReadPayload(context);
            JObject json = converter.ToJObject(new
            {
                context.Exception,
                Trace = EnumeratorUtil.Generate(new StringReader(context.Exception.StackTrace).ReadLine).ToArray(),
                Request = new
                {
                    Headers = context.Request.Headers.ToDictionary(pair => pair.Key, p => p.Value),
                    RequestUri = context.Request.RequestUri.ToString(),
                    context.Request.Method.Method,
                    Body = ReadPayload(context),
                    Version = context.Request.Version.ToString()
                }
            });
            string message = context.Exception.Message;
            return Task.Factory.StartNew(() =>
            {
                logger.LogFailure(Severity.Error, message, json);
            }, cancellationToken);
        }

        private static string ReadPayload(ExceptionLoggerContext context)
        {
            try
            {
                return context.Request.Content.ReadAsStringAsync().Result;
            }
            catch (Exception) {
                return null;
            }
        }
    }
}