using System.Collections.Generic;
using System.Linq;
using DotJEM.Json.Index.Schema;
using DotJEM.Web.Host.Util;
using DotJEM.Web.Host.Validation.Constraints;
using DotJEM.Web.Host.Validation.Results;
using Newtonsoft.Json.Linq;

namespace DotJEM.Web.Host.Validation
{
    public class FieldValidator : IFieldValidator
    {
        private readonly JPath field;
        private readonly IFieldConstraint constraint;

        public FieldValidator(JPath field, IFieldConstraint constraint)
        {
            this.field = field;
            this.constraint = constraint;
        }

        public virtual IEnumerable<FieldValidationResults> Validate(JObject entity, IValidationContext context)
        {
            List<JToken> tokens = entity.SelectTokens(field.Path).ToList();
            if(tokens.Count > 1)
            {
                foreach (JToken token in tokens)
                    yield return ValidateToken(token, context);
            }
            else
            {
                yield return ValidateToken(tokens.SingleOrDefault(), context);
            }
        }

        protected virtual FieldValidationResults ValidateToken(JToken token, IValidationContext context)
        {
            ValidationCollector collector = new ValidationCollector();
            constraint.Validate(context, token, collector);
            return new FieldValidationResults(field, token, collector);
        }
    }
}