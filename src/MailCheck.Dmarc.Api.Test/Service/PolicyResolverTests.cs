using System;
using System.Collections.Generic;
using System.Text;
using MailCheck.Dmarc.Api.Domain;
using MailCheck.Dmarc.Api.Service;
using MailCheck.Dmarc.Contracts.Entity;
using MailCheck.Dmarc.Contracts.SharedDomain;
using NUnit.Framework;

namespace MailCheck.Dmarc.Api.Test.Service
{
    [TestFixture]
    public class PolicyResolverTests
    {
        private PolicyResolver _policyResolver;

        [SetUp]
        public void Setup()
        {
            _policyResolver = new PolicyResolver();
        }

        [TestCase(PolicyType.None, "none")]
        [TestCase(PolicyType.Quarantine, "quarantine")]
        [TestCase(PolicyType.Reject, "reject")]
        [TestCase(PolicyType.Unknown, "unknown")]
        [TestCase((PolicyType)123, "unknown")]
        public void ResolveParsesPolicy(PolicyType actualPolicy, string expectedOutput)
        {
            DmarcInfoResponse response = GetTestResponse(actualPolicy);
            string result = _policyResolver.Resolve(response);

            Assert.AreEqual(expectedOutput, result);
        }

        [Test]
        public void ResolveReturnsNullWhenDmarcRecordsIsNull()
        {
            DmarcInfoResponse response = new DmarcInfoResponse(null, DmarcState.Created);
            string result = _policyResolver.Resolve(response);

            Assert.AreEqual(null, result);
        }

        [Test]
        public void ResolveReturnsNullWhenDmarcRecordsRecordsIsNull()
        {
            DmarcInfoResponse response = new DmarcInfoResponse(null, DmarcState.Created, new DmarcRecords(null, null, null, 0));
            string result = _policyResolver.Resolve(response);

            Assert.AreEqual(null, result);
        }

        [Test]
        public void ResolveReturnsNullWhenTagsIsNull()
        {
            DmarcInfoResponse response = new DmarcInfoResponse(null, DmarcState.Created, new DmarcRecords(null, new List<DmarcRecord> { new DmarcRecord(null, null, null, null, null, false, false) }, null, 0));
            string result = _policyResolver.Resolve(response);

            Assert.AreEqual(null, result);
        }

        private DmarcInfoResponse GetTestResponse(PolicyType dmarcRecord)
        {
            DmarcInfoResponse dmarcInfoResponse = new DmarcInfoResponse(null,
                DmarcState.Created,
                new DmarcRecords(null,
                    new List<DmarcRecord>
                    {
                        new DmarcRecord(null, new List<Tag> {new Policy(null, dmarcRecord, false)}, null, null, null,
                            false, false)
                    }, null, 0));

            return dmarcInfoResponse;
        }
    }
}