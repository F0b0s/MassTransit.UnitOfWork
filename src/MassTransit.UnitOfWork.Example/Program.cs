using MassTransit.UnitOfWork.Example.Filter;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MassTransit.UnitOfWork.Example
{
    public class Message
    {
        public string Text { get; set; }
    }

    public interface IDependency
    {
        void DoSomething() => Console.WriteLine("Hello world.");
    }

    public class DependencyImplementation : IDependency
    {
        public void DoSomething() => Console.WriteLine("Hello world from implementation.");
    }

    class Program
    {
        public static async Task Main()
        {
            var serviceCollection = new ServiceCollection()
                .AddTransient<IDependency, DependencyImplementation>();

            serviceCollection.AddMassTransit(x =>
            {
                x.AddConsumer<MessageConsumer>();

                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(sbc =>
                {
                    var host = sbc.Host("rabbitmq://127.0.0.1");

                    sbc.ConnectConsumerConfigurationObserver(new UnitOfWorkConsumerConfigurationObserver());

                    sbc.ReceiveEndpoint("test_queue", ep =>
                    {
                        ep.ConfigureConsumer<MessageConsumer>(provider);
                    });
                }));                
              });

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var bus = serviceProvider.GetRequiredService<IBusControl>();

            await bus.StartAsync();

            await bus.Publish(new Message { Text = "Hi" });

            Console.WriteLine("Press any key to exit");
            await Task.Run(() => Console.ReadKey());

            await bus.StopAsync();
        }
    }

    public class MessageConsumer : IConsumer<Message>
    {
        public Task Consume(ConsumeContext<Message> context)
        {
            return Console.Out.WriteLineAsync($"Received: {context.Message.Text}");
        }
    }
}
