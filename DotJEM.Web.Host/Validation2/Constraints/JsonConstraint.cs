﻿using System.Linq;
using DotJEM.Web.Host.Validation2.Constraints.Descriptive;
using DotJEM.Web.Host.Validation2.Constraints.Results;
using DotJEM.Web.Host.Validation2.Context;
using Newtonsoft.Json.Linq;

namespace DotJEM.Web.Host.Validation2.Constraints
{
    public abstract class JsonConstraint
    {
        private readonly JsonConstraintDescriptionAttribute description;

        protected JsonConstraint()
        {
            description = GetType()
                .GetCustomAttributes(typeof (JsonConstraintDescriptionAttribute), false)
                .OfType<JsonConstraintDescriptionAttribute>()
                .SingleOrDefault();
        }

        public abstract JsonConstraintResult Matches(IJsonValidationContext context, JToken token);

        public virtual JsonConstraintDescription Describe(IJsonValidationContext context, JToken token)
        {
            return new JsonConstraintDescription(this, description.Format);
        }

        public virtual JsonConstraint Optimize()
        {
            return this;
        }

        #region Operator Overloads
        public static JsonConstraint operator &(JsonConstraint x, JsonConstraint y)
        {
            return new AndJsonConstraint(x, y);
        }

        public static JsonConstraint operator |(JsonConstraint x, JsonConstraint y)
        {
            return new OrJsonConstraint(x, y);
        }

        public static JsonConstraint operator !(JsonConstraint x)
        {
            return new NotJsonConstraint(x);
        }
        #endregion
    }

    public abstract class TypedJsonConstraint<TTokenType> : JsonConstraint
    {
        public override JsonConstraintResult Matches(IJsonValidationContext context, JToken token)
        {
            return token == null
                ? Matches(context, default(TTokenType), true)
                : Matches(context, token.ToObject<TTokenType>());
        }

        protected virtual bool Matches(IJsonValidationContext context, TTokenType value)
        {
            return Matches(context, value, false);
        }

        protected virtual bool Matches(IJsonValidationContext context, TTokenType value, bool wasNull)
        {
            return true;
        }
    }
}