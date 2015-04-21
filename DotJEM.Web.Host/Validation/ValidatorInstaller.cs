using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using DotJEM.Web.Host.Providers.Pipeline;

namespace DotJEM.Web.Host.Validation
{
    public class ValidatorInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IJsonDecorator>().ImplementedBy<JsonEntityValidator>().LifestyleTransient());
        }
    }
}