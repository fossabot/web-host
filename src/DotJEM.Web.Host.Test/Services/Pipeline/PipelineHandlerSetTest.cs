﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel.Resolvers;
using DotJEM.Web.Host.Providers.Pipeline;
using NUnit.Framework;

namespace DotJEM.Web.Host.Test.Services.Pipeline
{
    [TestFixture]
    public class PipelineHandlerSetTest
    {
        [Test]
        public void Ctor_SimpleDependentModules_OrdersHandlers()
        {
            PipelineHandlerSet set = new PipelineHandlerSet(new IPipelineHandler[]
            {
                new Fourth(),
                new Fifth(), 
                new Second(), 
                new Third(),
                new First()
            });

            Assert.That(set.Select(x => x.GetType()), Is.EquivalentTo(new []
            {
                typeof(First),
                typeof(Second),
                typeof(Third),
                typeof(Fourth),
                typeof(Fifth)
            }));
        }

        [Test]
        public void Ctor_MultiDependentModules_OrdersHandlers()
        {
            PipelineHandlerSet set = new PipelineHandlerSet(new IPipelineHandler[]
            {
                new NeedsFirstAndThird(), 
                new Fourth(),
                new Fifth(), 
                new Second(), 
                new Third(),
                new First()
            });

            Assert.That(set.Select(x => x.GetType()), Is.EquivalentTo(new[]
            {
                typeof(First),
                typeof(Second),
                typeof(Third),
                typeof(NeedsFirstAndThird),
                typeof(Fourth),
                typeof(Fifth)
            }));
        }

        [Test]
        public void Ctor_MissingDependency_ThrowsException()
        {
            Assert.That(()=>new PipelineHandlerSet(new IPipelineHandler[]
            {
                new NeedsFirstAndThird(), 
                new Fourth(),
                new Fifth(), 
                new Second(), 
                new First()
            }), Throws.TypeOf<DependencyResolverException>());
        }
    }


    public class First : PipelineHandler { }

    [PipelineDepencency(typeof(First))]
    public class Second : PipelineHandler { }

    [PipelineDepencency(typeof(First))]
    [PipelineDepencency(typeof(Third))]
    public class NeedsFirstAndThird : PipelineHandler { }

    [PipelineDepencency(typeof(Second))]
    public class Third : PipelineHandler { }

    [PipelineDepencency(typeof(Third))]
    public class Fourth : PipelineHandler { }

    [PipelineDepencency(typeof(Fourth))]
    public class Fifth : PipelineHandler { }


}
