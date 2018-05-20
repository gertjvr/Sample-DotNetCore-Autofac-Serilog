using Autofac;
using AutofacSerilogIntegration;

namespace ConsoleApplication
{
    public class LoggingModule :
        Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterLogger();
        }
    }
}
