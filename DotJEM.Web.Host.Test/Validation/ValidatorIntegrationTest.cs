﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotJEM.Web.Host.Validation;
using DotJEM.Web.Host.Validation.Results;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace DotJEM.Web.Host.Test.Validation
{
    [TestFixture]
    public class ValidatorIntegrationTest
    {
        [TestCase("{ }")]
        [TestCase("{ foo: { barMax: 'this is way to long' } }")]
        [TestCase("{ foo: { barMin: 'to short' } }")]
        [TestCase("{ foo: { barMin: 'to short', barMax: 'this is way to long' } }")]
        [TestCase("{ combi: 'what' }")]
        public void Validate_InvalidData_ShouldReturnErrors(string json)
        {
            IValidator validator = new TestValidator();

            ValidationResult result = validator.Validate(JObject.Parse(json));

            Assert.That(result.HasErrors, Is.True);
        }

        [TestCase("{ arr: [] }")]
        [TestCase("{ arr: [ '42' ] }")]
        [TestCase("{ arr: [ '42', '24' ] }")]
        [TestCase("{ arr: [ { foo: 'bar' }] }")]
        [TestCase("{ arr: [ { foo: 'bar' }, { foo: 'bob' }] }")]
        public void Validate_Arrays_ShouldReturnErrors(string json)
        {
            IValidator validator = new TestArraysValidator();

            ValidationResult result = validator.Validate(JObject.Parse(json));

            Assert.That(result.HasErrors, Is.True);
            Debug.WriteLine(result);
        }
    }

    public class TestValidator : Validator
    {
        public TestValidator()
        {
            Field("foo", Is.Required());
            Field("foo.barMax", Must.HaveMaxLength(10));
            Field("foo.barMin", Must.HaveMinLength(10));
            Field("combi", Must.HaveLength(10, 20).Match("^[0-9]*$"));
        }
    }

    public class TestArraysValidator : Validator
    {
        public TestArraysValidator()
        {
            Field("arr[*].foo", Must.Match("^\\d+$").Required());
        }
    }
}