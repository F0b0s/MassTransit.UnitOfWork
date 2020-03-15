using System;
using System.Threading.Tasks;

namespace MassTransit.UnitOfWork.Example
{
    public class Message
    {
        public string Text { get; set; }
    }

    class Program
    {
        public static async Task Main()
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host("rabbitmq://127.0.0.1");

                sbc.ConnectConsumerConfigurationObserver(new UnitOfWorkConsumerConfigurationObserver());

                sbc.ReceiveEndpoint("test_queue", ep =>
                {
                    ep.Consumer<MessageConsumer>();
                });
            });

            await bus.StartAsync(); // This is important!

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
