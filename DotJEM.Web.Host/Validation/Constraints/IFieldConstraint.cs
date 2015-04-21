using DotJEM.Web.Host.Validation.Results;
using Newtonsoft.Json.Linq;

namespace DotJEM.Web.Host.Validation.Constraints
{
    public interface IFieldConstraint
    {
        void Validate(JToken token, IValidationCollector collector);
        bool Matches(JToken token);
    }
}