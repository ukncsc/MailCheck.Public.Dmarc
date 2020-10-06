using MailCheck.Common.Data.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.Dmarc.Entity.Seeding.History
{
    public class HistoryMigratorFactory
    {
        public static IHistoryMigrator Create(string dmarcConnectionString, string newDmarcConnectionString)
        {
            return new ServiceCollection()
                .AddTransient<IHistoryReaderDao>(_ => new HistoryReaderDao(new StringConnectionInfo(dmarcConnectionString)))
                .AddTransient<IHistoryWriterDao>(_ => new HistoryWriterDao(new StringConnectionInfo(newDmarcConnectionString)))
                .AddTransient<IHistoryMigrator, HistoryMigrator>()
                .BuildServiceProvider()
                .GetRequiredService<IHistoryMigrator>();
        }
    }
}
