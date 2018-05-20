using Autofac;
using GreenPipes;
using MassTransit;

namespace ConsoleApplication
{
    public class BusModule :
        Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterConsumers(ThisAssembly);

            builder
                .Register(context =>
                {
                    return Bus.Factory.CreateUsingRabbitMq(cfg =>
                    {
                        cfg.UseSerilog();

                        var host = cfg.Host("localhost", "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.ReceiveEndpoint(host, "console-service-endpoint", ec =>
                        {
                            ec.PrefetchCount = 16;
                            ec.UseMessageRetry(r => r.Interval(2, 100));
                            ec.LoadFrom(context);
                        });
                    });
                })
                .As<IBusControl>()
                .As<IBus>()
                .SingleInstance();

            builder
                .Register(context => context.Resolve<IBus>().CreateRequestClient<DoSomething>())
                .As<IRequestClient<DoSomething>>()
                .InstancePerDependency();
        }
    }
}
