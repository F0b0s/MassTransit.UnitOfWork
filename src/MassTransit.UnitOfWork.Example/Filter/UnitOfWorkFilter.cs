using GreenPipes;
using System;
using System.Threading.Tasks;

namespace MassTransit.UnitOfWork.Example.Filter
{
    public class UnitOfWorkFilter<TContext, TMessage> : IFilter<TContext>
            where TContext : class, ConsumeContext<TMessage>
            where TMessage : class
    {
        public void Probe(ProbeContext context)
        {
        }

        public async Task Send(TContext context, IPipe<TContext> next)
        {
            Console.WriteLine("Before uow execution....");

            await next.Send(context);

            context.TryGetPayload(out IServiceProvider serviceProvider);
            var dependency = (IDependency)serviceProvider.GetService(typeof(IDependency));
            dependency.DoSomething();

            Console.WriteLine("After uow execution....");
        }
    }
}
