using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Notifiers;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Entity.Config;
using MailCheck.Dmarc.Entity.Entity;
using NUnit.Framework;
using LocalNotifier = MailCheck.Dmarc.Entity.Entity.Notifiers.FindingsChangedNotifier;
using ErrorMessage = MailCheck.Dmarc.Contracts.SharedDomain.Message;
using MailCheck.Dmarc.Contracts;
using MailCheck.Dmarc.Contracts.Evaluator;
using MailCheck.Common.Contracts.Findings;
using System.Linq;

namespace MailCheck.Dmarc.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class FindingsChangedNotifierTests
    {
        private IMessageDispatcher _messageDispatcher;
        private IFindingsChangedNotifier _findingsChangedNotifier;
        private IDmarcEntityConfig _dmarcEntityConfig;

        private LocalNotifier _notifier;

        private const string Id = "test.gov.uk";

        [SetUp]
        public void SetUp()
        {
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _findingsChangedNotifier = new FindingsChangedNotifier();
            _dmarcEntityConfig = A.Fake<IDmarcEntityConfig>();
            _notifier = new LocalNotifier(_messageDispatcher, _findingsChangedNotifier, _dmarcEntityConfig);
        }

        [TestCaseSource(nameof(ExerciseFindingsChangedNotifierTestPermutations))]
        public void ExerciseFindingsChangedNotifier(FindingsChangedNotifierTestCase testCase)
        {
            A.CallTo(() => _dmarcEntityConfig.WebUrl).Returns("testurl.com");

            DmarcRecords stateDmarcRecords = CreateDmarcRecords();
            stateDmarcRecords.Messages = testCase.StateRecordsMessages;
            stateDmarcRecords.Records[0].Messages = testCase.StateRecordsRecordMessages ?? new List<ErrorMessage>();
            stateDmarcRecords.Records[0].Tags[0].Explanation = "Explanation";

            DmarcEntityState state = new DmarcEntityState(Id, 2, Contracts.Entity.DmarcState.PollPending, DateTime.Now)
            {
                LastUpdated = DateTime.Now.AddDays(-1),
                DmarcRecords = stateDmarcRecords,
                Messages = testCase.StateMessages
            };

            DmarcRecords resultDmarcRecords = CreateDmarcRecords();
            resultDmarcRecords.Messages = testCase.ResultRecordsMessages ?? new List<ErrorMessage>();
            resultDmarcRecords.Records[0].Messages = testCase.ResultRecordsRecordMessages ?? new List<ErrorMessage>();
            resultDmarcRecords.Records[0].Tags[0].Explanation = "Explanation";

            DmarcRecordsEvaluated dmarcRecordsEvaluated = new DmarcRecordsEvaluated(Id, resultDmarcRecords, null, testCase.ResultMessages, DateTime.MinValue);

            _notifier.Handle(state, dmarcRecordsEvaluated);

            A.CallTo(() => _messageDispatcher.Dispatch(
                A<FindingsChanged>.That.Matches(x =>
                    x.Added.Count == testCase.ExpectedAdded.Count &&
                    x.Removed.Count == testCase.ExpectedRemoved.Count &&
                    x.Sustained.Count == testCase.ExpectedSustained.Count), A<string>._)).MustHaveHappenedOnceExactly();

            for (int i = 0; i < testCase.ExpectedAdded.Count; i++)
            {
                A.CallTo(() => _messageDispatcher.Dispatch(
                    A<FindingsChanged>.That.Matches(x =>
                        x.Added[i].Name == testCase.ExpectedAdded[i].Name &&
                        x.Added[i].EntityUri == testCase.ExpectedAdded[i].EntityUri &&
                        x.Added[i].SourceUrl == testCase.ExpectedAdded[i].SourceUrl &&
                        x.Added[i].Severity == testCase.ExpectedAdded[i].Severity &&
                        x.Added[i].Title == testCase.ExpectedAdded[i].Title), A<string>._)).MustHaveHappenedOnceExactly();
            };

            for (int i = 0; i < testCase.ExpectedRemoved.Count; i++)
            {
                A.CallTo(() => _messageDispatcher.Dispatch(
                    A<FindingsChanged>.That.Matches(x =>
                        x.Removed[i].Name == testCase.ExpectedRemoved[i].Name &&
                        x.Removed[i].EntityUri == testCase.ExpectedRemoved[i].EntityUri &&
                        x.Removed[i].SourceUrl == testCase.ExpectedRemoved[i].SourceUrl &&
                        x.Removed[i].Severity == testCase.ExpectedRemoved[i].Severity &&
                        x.Removed[i].Title == testCase.ExpectedRemoved[i].Title), A<string>._)).MustHaveHappenedOnceExactly();
            };

            for (int i = 0; i < testCase.ExpectedSustained.Count; i++)
            {
                A.CallTo(() => _messageDispatcher.Dispatch(
                    A<FindingsChanged>.That.Matches(x =>
                        x.Sustained[i].Name == testCase.ExpectedSustained[i].Name &&
                        x.Sustained[i].EntityUri == testCase.ExpectedSustained[i].EntityUri &&
                        x.Sustained[i].SourceUrl == testCase.ExpectedSustained[i].SourceUrl &&
                        x.Sustained[i].Severity == testCase.ExpectedSustained[i].Severity &&
                        x.Sustained[i].Title == testCase.ExpectedSustained[i].Title), A<string>._)).MustHaveHappenedOnceExactly();
            };
        }

        private static IEnumerable<FindingsChangedNotifierTestCase> ExerciseFindingsChangedNotifierTestPermutations()
        {
            ErrorMessage evalError1 = new ErrorMessage(Guid.NewGuid(), "mailcheck.dmarc.testName1", MessageSources.DmarcEvaluator, MessageType.error, "EvaluationError", string.Empty);
            ErrorMessage evalError2 = new ErrorMessage(Guid.NewGuid(), "mailcheck.dmarc.testName2", MessageSources.DmarcEvaluator, MessageType.error, "EvaluationError", string.Empty);
            ErrorMessage pollerWarn1 = new ErrorMessage(Guid.NewGuid(), "mailcheck.dmarc.testName3", MessageSources.DmarcPoller, MessageType.warning, "PollerError", string.Empty);

            Finding findingEvalError1 = new Finding
            {
                EntityUri = "domain:test.gov.uk",
                Name = "mailcheck.dmarc.testName1",
                SourceUrl = $"https://testurl.com/app/domain-security/{Id}/dmarc",
                Severity = "Urgent",
                Title = "EvaluationError"
            };

            Finding findingEvalError2 = new Finding
            {
                EntityUri = "domain:test.gov.uk",
                Name = "mailcheck.dmarc.testName2",
                SourceUrl = $"https://testurl.com/app/domain-security/{Id}/dmarc",
                Severity = "Urgent",
                Title = "EvaluationError"
            };

            Finding findingPollerWarn1 = new Finding
            {
                EntityUri = "domain:test.gov.uk",
                Name = "mailcheck.dmarc.testName3",
                SourceUrl = $"https://testurl.com/app/domain-security/{Id}/dmarc",
                Severity = "Advisory",
                Title = "PollerError"
            };

            FindingsChangedNotifierTestCase test1 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1 },
                StateRecordsMessages = new List<ErrorMessage> { evalError2 },
                StateRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ResultMessages = new List<ErrorMessage>(),
                ResultRecordsMessages = new List<ErrorMessage>(),
                ResultRecordsRecordMessages = new List<ErrorMessage>(),
                ExpectedAdded = new List<Finding>(),
                ExpectedRemoved = new List<Finding> { findingEvalError1, findingEvalError2, findingPollerWarn1 },
                ExpectedSustained = new List<Finding>(),
                Description = "3 removed messages should produce 3 findings removed"
            };

            FindingsChangedNotifierTestCase test2 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1 },
                StateRecordsMessages = new List<ErrorMessage> { evalError2 },
                StateRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ResultMessages = new List<ErrorMessage> { evalError1 },
                ResultRecordsMessages = new List<ErrorMessage> { evalError2 },
                ResultRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ExpectedAdded = new List<Finding>(),
                ExpectedRemoved = new List<Finding>(),
                ExpectedSustained = new List<Finding> { findingEvalError1, findingEvalError2, findingPollerWarn1 },
                Description = "3 sustained messages should produce 3 findings sustained"
            };

            FindingsChangedNotifierTestCase test3 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage>(),
                StateRecordsMessages = new List<ErrorMessage>(),
                StateRecordsRecordMessages = new List<ErrorMessage>(),
                ResultMessages = new List<ErrorMessage> { evalError1 },
                ResultRecordsMessages = new List<ErrorMessage> { evalError2 },
                ResultRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ExpectedAdded = new List<Finding>{ findingEvalError1, findingEvalError2, findingPollerWarn1 },
                ExpectedRemoved = new List<Finding>(),
                ExpectedSustained = new List<Finding>(),
                Description = "3 added messages should produce 3 findings added"
            };

            FindingsChangedNotifierTestCase test4 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1 },
                StateRecordsMessages = new List<ErrorMessage>(),
                StateRecordsRecordMessages = new List<ErrorMessage>(),
                ResultMessages = new List<ErrorMessage> { evalError1 },
                ResultRecordsMessages = new List<ErrorMessage> { evalError2 },
                ResultRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ExpectedAdded = new List<Finding> { findingEvalError2, findingPollerWarn1 },
                ExpectedRemoved = new List<Finding>(),
                ExpectedSustained = new List<Finding> { findingEvalError1 },
                Description = "2 added messages and 1 sustained should produce 2 findings added and 1 finding sustained"
            };

            FindingsChangedNotifierTestCase test5 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1 },
                StateRecordsMessages = new List<ErrorMessage> { evalError2 },
                StateRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ResultMessages = null,
                ResultRecordsMessages = null,
                ResultRecordsRecordMessages = null,
                ExpectedAdded = new List<Finding>(),
                ExpectedRemoved = new List<Finding> { findingEvalError1, findingEvalError2, findingPollerWarn1 },
                ExpectedSustained = new List<Finding>(),
                Description = "3 removed messages due to nulls should produce 3 findings removed"
            };

            FindingsChangedNotifierTestCase test6 = new FindingsChangedNotifierTestCase
            {
                StateMessages = null,
                StateRecordsMessages = null,
                StateRecordsRecordMessages = null,
                ResultMessages = new List<ErrorMessage> { evalError1 },
                ResultRecordsMessages = new List<ErrorMessage> { evalError2 },
                ResultRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ExpectedAdded = new List<Finding> { findingEvalError1, findingEvalError2, findingPollerWarn1 },
                ExpectedRemoved = new List<Finding>(),
                ExpectedSustained = new List<Finding>(),
                Description = "3 added messages from nulls should produce 3 findings added"
            };

            FindingsChangedNotifierTestCase test7 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1 },
                StateRecordsMessages = null,
                StateRecordsRecordMessages = null,
                ResultMessages = new List<ErrorMessage> { evalError1 },
                ResultRecordsMessages = new List<ErrorMessage> { evalError2 },
                ResultRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ExpectedAdded = new List<Finding> { findingEvalError2, findingPollerWarn1 },
                ExpectedRemoved = new List<Finding>(),
                ExpectedSustained = new List<Finding> { findingEvalError1 },
                Description = "2 added messages from nulls and 1 sustained should produce 2 findings added and 1 finding sustained"
            };

            yield return test1;
            yield return test2;
            yield return test3;
            yield return test4;
            yield return test5;
            yield return test6;
            yield return test7;
        }

        private static DmarcRecords CreateDmarcRecords(string domain = "test.gov.uk")
        {
            return new DmarcRecords(domain, new List<DmarcRecord>
            {
                new DmarcRecord("v=dmarc......", new List<Tag> { new Adkim("adkim", AlignmentType.R, false)}, null, domain, "", true, false)
            }, new List<ErrorMessage>(), 100);
        }

        public class FindingsChangedNotifierTestCase
        {
            public List<ErrorMessage> StateMessages { get; set; }
            public List<ErrorMessage> StateRecordsMessages { get; set; }
            public List<ErrorMessage> StateRecordsRecordMessages { get; set; }
            public List<ErrorMessage> ResultMessages { get; set; }
            public List<ErrorMessage> ResultRecordsMessages { get; set; }
            public List<ErrorMessage> ResultRecordsRecordMessages { get; set; }
            public List<Finding> ExpectedAdded { get; set; }
            public List<Finding> ExpectedRemoved { get; set; }
            public List<Finding> ExpectedSustained { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return Description;
            }
        }
    }
}