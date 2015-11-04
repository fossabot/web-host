using Newtonsoft.Json.Linq;

namespace DotJEM.Web.Host.Providers.Pipeline
{
    public interface IPipeline
    {
        JObject ExecuteAfterGet(JObject json, string contentType, PipelineContext context);

        JObject ExecuteBeforePost(JObject json, string contentType, PipelineContext context);
        JObject ExecuteAfterPost(JObject json, string contentType, PipelineContext context);

        JObject ExecuteBeforePut(JObject json, JObject prev, string contentType, PipelineContext context);
        JObject ExecuteAfterPut(JObject json, JObject prev, string contentType, PipelineContext context);

        JObject ExecuteBeforeDelete(JObject json, string contentType, PipelineContext context);
        JObject ExecuteAfterDelete(JObject json, string contentType, PipelineContext context);
    }
}