using Microsoft.Extensions.Options;
using Quartz;

namespace eMeetup.Modules.Events.Infrastructure.Outbox;

internal sealed class ConfigureProcessOutboxJob(IOptions<OutboxOptions> outboxOptions)
    : IConfigureOptions<IQuartzBuilder>
{
    private readonly OutboxOptions _outboxOptions = outboxOptions.Value;

    public void Configure(IQuartzBuilder options)
    {
        string jobName = typeof(ProcessOutboxJob).FullName!;

        options
            .AddJob<ProcessOutboxJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule
                            .WithInterval(TimeSpan.FromSeconds(_outboxOptions.IntervalInSeconds))
                            .RepeatForever()));
    }
}
