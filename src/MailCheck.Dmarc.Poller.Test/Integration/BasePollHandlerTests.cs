using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DnsClient;
using FakeItEasy;
using MailCheck.Common.Logging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.Poller;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Poller.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Message = MailCheck.Dmarc.Contracts.SharedDomain.Message;

namespace MailCheck.Dmarc.Poller.Test.Integration
{
    [TestFixture]
    public class BasePollHandlerTests
    {
        protected IHandle<DmarcPollPending> Handler;
        protected ILookupClient LookupClient;
        private IMessageDispatcher _messageDispatcher;

        [SetUp]
        public void Setup()
        {
            Environment.SetEnvironmentVariable("ACTIVE_FEATURES","MigrationAdvisories");
            LookupClient = A.Fake<ILookupClient>();
            _messageDispatcher = A.Fake<IMessageDispatcher>();

            IServiceCollection serviceCollection = new ServiceCollection();
            new StartUp.StartUp().ConfigureServices(serviceCollection);

            serviceCollection
                .AddSingleton(LookupClient)
                .AddSingleton(_messageDispatcher)
                .AddSingleton(A.Fake<IDmarcPollerConfig>())
                .AddSingleton(A.Fake<ILogger>())
                .AddSerilogLogging();

            Handler = serviceCollection.BuildServiceProvider().GetRequiredService<IHandle<DmarcPollPending>>();
        }

        protected void SetUpTxtRecords(params string[] values)
        {
            A.CallTo(() => LookupClient.QueryAsync(A<string>._, A<QueryType>._, QueryClass.IN, default(CancellationToken)))
                .Returns(new TestDnsQueryResponse(values));
        }

        protected DmarcRecordsPolled GetDispatchedMessage()
        {
            return Fake.GetCalls(_messageDispatcher).First().GetArgument<DmarcRecordsPolled>(0);
        }

        protected List<Message> GetAdvisories()
        {
            DmarcRecordsPolled dmarcRecordsPolled = GetDispatchedMessage();
            return dmarcRecordsPolled.Messages.Concat(dmarcRecordsPolled.Records.Messages)
                .Concat(dmarcRecordsPolled.Records.Records.SelectMany(x => x.Messages)).ToList();
        }

        protected IEnumerable<T> GetTags<T>() where T : Tag
        {
            DmarcRecordsPolled dispatchedMessage = GetDispatchedMessage();
            IEnumerable<Tag> tags = dispatchedMessage?.Records?.Records?.SelectMany(x=>x.Tags).Where(x => x.GetType() == typeof(T));
            return tags?.Select(x => (T) x);
        }
    }
}
