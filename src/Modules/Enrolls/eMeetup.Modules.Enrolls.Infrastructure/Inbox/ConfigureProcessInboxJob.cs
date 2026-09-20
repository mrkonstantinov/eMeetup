using Microsoft.Extensions.Options;
using Quartz;

namespace eMeetup.Modules.Enrolls.Infrastructure.Inbox;

internal sealed class ConfigureProcessInboxJob(IOptions<InboxOptions> outboxOptions)
    : IConfigureOptions<IQuartzBuilder>
{
    private readonly InboxOptions _inboxOptions = outboxOptions.Value;

    public void Configure(IQuartzBuilder options)
    {
        string jobName = typeof(ProcessInboxJob).FullName!;

        options
            .AddJob<ProcessInboxJob>(configure => configure.WithIdentity(jobName))
            .AddTrigger(configure =>
                configure
                    .ForJob(jobName)
                    .WithSimpleSchedule(schedule =>
                        schedule
                            .WithInterval(TimeSpan.FromSeconds(_inboxOptions.IntervalInSeconds))
                            .RepeatForever()));
    }
}
