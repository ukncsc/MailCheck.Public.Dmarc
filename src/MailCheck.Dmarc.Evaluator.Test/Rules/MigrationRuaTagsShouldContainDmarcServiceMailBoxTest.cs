using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Dmarc.Contracts.SharedDomain;
using MailCheck.Dmarc.Evaluator.Rules;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MailCheck.Dmarc.Evaluator.Test.Rules
{
    [TestFixture]
    public class MigrationRuaTagsShouldContainDmarcServiceMailBoxTest
    {
        private MigrationRuaTagsShouldContainDmarcServiceMailBox _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new MigrationRuaTagsShouldContainDmarcServiceMailBox();
        }

        public async Task Test(DmarcRecord dmarcRecord, bool isErrorExpected, MessageType? expectedError = null, string markDown = null)
        {
            List<Message> messages = await _rule.Evaluate(dmarcRecord);

            Assert.That(messages.Any(), Is.EqualTo(isErrorExpected));

            Assert.That(messages.FirstOrDefault()?.MessageType, Is.EqualTo(expectedError));

            if (markDown != null)
            {
                Assert.That(messages.FirstOrDefault()?.MarkDown, Is.EqualTo(markDown));
            }
        }

        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasCorrectlyFormattedMarkdown1()
        {
            string record = "v=DMARC1; p=none; rua=mailto:DMARC-rua@DMARC.service.gov.uk; sp=none; fo=0:1:d:s";
            ReportUriAggregate tag = CreateReportUriAggregate(
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk"));
            DmarcRecord testDmarcRecord = new DmarcRecord(record, new List<Tag> { tag }, null, "test.gov.uk", null, false, false);

            string actualMarkdown = (await _rule.Evaluate(testDmarcRecord)).First().MarkDown;

            string expectedMarkdown = "Your DMARC record contains the wrong email address for Mail Check aggregate report processing."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** Please change your DMARC record to be the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk; sp=none; fo=0:1:d:s;`"
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk; sp=none; fo=0:1:d:s;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasCorrectlyFormattedMarkdown2()
        {
            string record = "v=DMARC1; p=none; rua=mailto:DMARC-rua@DMARC.service.gov.uk,mailto:DMARC-rua@DMARC.service.gov.uk,mailto:DMARC-rua@DMARC.service.gov.uk; sp=none; fo=0:1:d:s";
            ReportUriAggregate tag = CreateReportUriAggregate(
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk"),
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk"),
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk"));
            DmarcRecord testDmarcRecord = new DmarcRecord(record, new List<Tag> { tag }, null, "test.gov.uk", null, false, false);

            string actualMarkdown = (await _rule.Evaluate(testDmarcRecord)).First().MarkDown;

            string expectedMarkdown = "Your DMARC record contains the wrong email address for Mail Check aggregate report processing."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** Please change your DMARC record to be the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk; sp=none; fo=0:1:d:s;`"
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk; sp=none; fo=0:1:d:s;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasCorrectlyFormattedMarkdown3()
        {
            string record = "v=DMARC1; p=none; rua=mailto:DMARC-rua@DMARC.service.gov.uk,mailto:something-else@dmarc.service.gov.uk,mailto:dmarc-rua@dmarc.service.gov.uk; sp=none; fo=0:1:d:s";
            ReportUriAggregate tag = CreateReportUriAggregate(
                new Uri("mailto:dmarc-rua@dmarc.service.gov.uk"),
                new Uri("mailto:something-else@dmarc.service.gov.uk"),
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk"));
            DmarcRecord testDmarcRecord = new DmarcRecord(record, new List<Tag> { tag }, null, "test.gov.uk", null, false, false);

            string actualMarkdown = (await _rule.Evaluate(testDmarcRecord)).First().MarkDown;

            string expectedMarkdown = "Your DMARC record contains the wrong email address for Mail Check aggregate report processing."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** Please change your DMARC record to be the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk; sp=none; fo=0:1:d:s;`"
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk; sp=none; fo=0:1:d:s;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasCorrectlyFormattedMarkdown4()
        {
            string record = "v=DMARC1; p=none; rua=mailto:DMARC-rua@DMARC.service.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s";
            ReportUriAggregate tag = CreateReportUriAggregate(
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk"),
                new Uri("mailto:0007756a@mxtoolbox.dmarc-report.com"),
                new Uri("mailto:4fvdev87@ag.dmarcian-eu.com"));
            DmarcRecord testDmarcRecord = new DmarcRecord(record, new List<Tag> {tag}, null, "test.gov.uk", null, false, false);

            string actualMarkdown =  (await _rule.Evaluate(testDmarcRecord)).First().MarkDown;
            string expectedMarkdown = "Your DMARC record contains the wrong email address for Mail Check aggregate report processing."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** Please change your DMARC record to be the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`"
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasDuplicateNcscAuthorityRemoved1()
        {
            string record = "v=DMARC1; p=none; rua=mailto:DMARC-rua@DMARC.service.gov.uk,mailto:DMARC-rua@DMARC.service.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s";
            ReportUriAggregate tag = CreateReportUriAggregate(
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk"),
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk"),
                new Uri("mailto:0007756a@mxtoolbox.dmarc-report.com"),
                new Uri("mailto:4fvdev87@ag.dmarcian-eu.com"));
            DmarcRecord testDmarcRecord = new DmarcRecord(record, new List<Tag> { tag }, null, "test.gov.uk", null, false, false);

            string actualMarkdown = (await _rule.Evaluate(testDmarcRecord)).First().MarkDown;

            string expectedMarkdown = "Your DMARC record contains the wrong email address for Mail Check aggregate report processing."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** Please change your DMARC record to be the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`"
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasDuplicateNcscAuthorityRemoved2()
        {
            string record = "v=DMARC1; p=none; rua=mailto:DMARC-rua@DMARC.service.gov.uk,mailto:dmarc-rua@dmarc.service.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s";
            ReportUriAggregate tag = CreateReportUriAggregate(
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk"),
                new Uri("mailto:dmarc-rua@dmarc.service.gov.uk"),
                new Uri("mailto:0007756a@mxtoolbox.dmarc-report.com"),
                new Uri("mailto:4fvdev87@ag.dmarcian-eu.com"));
            DmarcRecord testDmarcRecord = new DmarcRecord(record, new List<Tag> { tag }, null, "test.gov.uk", null, false, false);

            string actualMarkdown = (await _rule.Evaluate(testDmarcRecord)).First().MarkDown;

            string expectedMarkdown = "Your DMARC record contains the wrong email address for Mail Check aggregate report processing."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** Please change your DMARC record to be the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`"
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasDuplicateNcscAuthorityRemoved3()
        {
            string record = "v=DMARC1; p=none; rua=mailto:DMARC-rua@DMARC.service.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s";
            ReportUriAggregate tag = CreateReportUriAggregate(
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk"),
                new Uri("mailto:0007756a@mxtoolbox.dmarc-report.com"),
                new Uri("mailto:4fvdev87@ag.dmarcian-eu.com"));
            DmarcRecord testDmarcRecord = new DmarcRecord(record, new List<Tag> { tag }, null, "test.gov.uk", null, false, false);

            string actualMarkdown = (await _rule.Evaluate(testDmarcRecord)).First().MarkDown;

            string expectedMarkdown = "Your DMARC record contains the wrong email address for Mail Check aggregate report processing."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** Please change your DMARC record to be the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`"
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasDuplicateNcscAuthorityRemoved4()
        {
            string record = "v=DMARC1; p=none; rua=mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com,mailto:DMARC-rua@DMARC.service.gov.uk; sp=none; fo=0:1:d:s";
            ReportUriAggregate tag = CreateReportUriAggregate(
              new Uri("mailto:0007756a@mxtoolbox.dmarc-report.com"),
                new Uri("mailto:4fvdev87@ag.dmarcian-eu.com"),
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk"));
            DmarcRecord testDmarcRecord = new DmarcRecord(record, new List<Tag> { tag }, null, "test.gov.uk", null, false, false);

            string actualMarkdown = (await _rule.Evaluate(testDmarcRecord)).First().MarkDown;

            string expectedMarkdown = "Your DMARC record contains the wrong email address for Mail Check aggregate report processing."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** Please change your DMARC record to be the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`"
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasDuplicateNcscAuthorityRemoved5()
        {
            string record = "v=DMARC1; p=none; rua=mailto:DMARC-rua@DMARC.service.gov.uk,mailto:DMARC-rua@DMARC.service.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s";
            ReportUriAggregate tag = CreateReportUriAggregate(
                new Uri("mailto:0007756a@mxtoolbox.dmarc-report.com"),
                new Uri("mailto:4fvdev87@ag.dmarcian-eu.com"),
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk"),
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk")
                );
            DmarcRecord testDmarcRecord = new DmarcRecord(record, new List<Tag> { tag }, null, "test.gov.uk", null, false, false);

            string actualMarkdown = (await _rule.Evaluate(testDmarcRecord)).First().MarkDown;

            string expectedMarkdown = "Your DMARC record contains the wrong email address for Mail Check aggregate report processing."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** Please change your DMARC record to be the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`"
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task ErrorForIncorrectMailCheckUriHasDuplicateNcscAuthorityRemoved6()
        {
            string record = "v=DMARC1; p=none; rua=mailto:DMARC-rua@DMARC.service.gov.uk, mailto:DMARC-rua@DMARC.service.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s";
            ReportUriAggregate tag = CreateReportUriAggregate(
                new Uri("mailto:0007756a@mxtoolbox.dmarc-report.com"),
                new Uri("mailto:4fvdev87@ag.dmarcian-eu.com"),
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk"),
                new Uri("mailto:DMARC-rua@DMARC.service.gov.uk")
            );
            DmarcRecord testDmarcRecord = new DmarcRecord(record, new List<Tag> { tag }, null, "test.gov.uk", null, false, false);

            string actualMarkdown = (await _rule.Evaluate(testDmarcRecord)).First().MarkDown;

            string expectedMarkdown = "Your DMARC record contains the wrong email address for Mail Check aggregate report processing."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** Please change your DMARC record to be the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`"
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=none; rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk,mailto:4fvdev87@ag.dmarcian-eu.com,mailto:0007756a@mxtoolbox.dmarc-report.com; sp=none; fo=0:1:d:s;`";

            Assert.AreEqual(expectedMarkdown, actualMarkdown);
        }

        [Test]
        public async Task ErrorWhenIncorrectMailCheckMailbox()
        {
            var testDmarcRecord = CreateDmarcRecord(
                CreateReportUriAggregate(new Uri("mailto:rua@dmarc.service.gov.uk")));

            await Test(testDmarcRecord, true, MessageType.error);
        }

        [Test]
        public async Task NoErrorWhenDuplicateTags()
        {
            var testDmarcRecord = CreateDmarcRecord(
                CreateReportUriAggregate(new Uri("mailto:dmarc-rua@dmarc.service.gov.uk")),
                CreateReportUriAggregate(new Uri("mailto:blah@somewhere.co.uk")));

            await Test(testDmarcRecord, false);
        }

        [Test]
        public async Task WarningWhenSameMailboxMentionedMoreThanOnce()
        {
            var testDmarcRecord = CreateDmarcRecord(
                CreateReportUriAggregate(
                    new Uri("mailto:a@b.com"),
                    new Uri("mailto:a@b.com"),
                    new Uri("mailto:dmarc-rua@dmarc.service.gov.uk")));

            await Test(testDmarcRecord, true, MessageType.warning);
        }

        [Test]
        public async Task ErrorWhenMailCheckRufMailboxUsed()
        {
            var testDmarcRecord = CreateDmarcRecord(
                CreateReportUriAggregate(new Uri("mailto:dmarc-ruf@dmarc.service.gov.uk")));

            await Test(testDmarcRecord, true, MessageType.error);
        }

        [Test]
        public async Task WarningWhenNoMailCheckMailbox()
        {
            var testDmarcRecord = CreateDmarcRecord("",
                CreateReportUriAggregate(new Uri("mailto:a@b.com")));

            await Test(testDmarcRecord, true, MessageType.info);
        }


        [Test]
        public async Task WarningWhenRuaDoesNotHaveMailCheckMailbox()
        {
            string record = "v=DMARC1;p=reject;adkim=s;aspf=s;fo=1;rua=mailto:rua@service.gov.uk;ruf=mailto:dmarc-ruf@dmarc.service.gov.uk;";

            string markDown = "The DMARC record does not contain the NCSC Mail Check email address, it's fine to use other tools but be aware that we won't be able to help you investigate email abuse, and you won't see any reporting in Mail Check."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=reject; adkim=s; aspf=s; fo=1; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk,mailto:rua@service.gov.uk; ruf=mailto:dmarc-ruf@dmarc.service.gov.uk; `" 
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=reject; adkim=s; aspf=s; fo=1; rua=mailto:dmarc-rua@dmarc.service.gov.uk,mailto:rua@service.gov.uk; ruf=mailto:dmarc-ruf@dmarc.service.gov.uk; `";

            var testDmarcRecord = CreateDmarcRecord(record);

            await Test(testDmarcRecord, true, MessageType.info, markDown);
        }

        [Test]
        public async Task WarningWhenNoRuaExistAddMailCheckMailbox()
        {
            string record = "v=DMARC1;p=reject;adkim=s;aspf=s;fo=1;ruf=mailto:dmarc-ruf@dmarc.service.gov.uk;";

            string markDown = "The DMARC record does not contain the NCSC Mail Check email address, it's fine to use other tools but be aware that we won't be able to help you investigate email abuse, and you won't see any reporting in Mail Check."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=reject; adkim=s; aspf=s; fo=1; ruf=mailto:dmarc-ruf@dmarc.service.gov.uk; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk;`"
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=reject; adkim=s; aspf=s; fo=1; ruf=mailto:dmarc-ruf@dmarc.service.gov.uk; rua=mailto:dmarc-rua@dmarc.service.gov.uk;`";

            var testDmarcRecord = CreateDmarcRecord(record);

            await Test(testDmarcRecord, true, MessageType.info, markDown);
        }

        [TestCase("v=DMARC1;p=quarantine")]
        [TestCase("v=DMARC1;;p=quarantine")]
        [TestCase("v=DMARC1;p=quarantine;;")]
        [TestCase("v=DMARC1;     p=quarantine;;  ;")]
        [TestCase("v=DMARC1 ; p=quarantine ;")]
        public async Task UntidyRecordWithNoRuaSuggestsNeatRecordWithAppendedRua(string record)
        {
            string markDown = "The DMARC record does not contain the NCSC Mail Check email address, it's fine to use other tools but be aware that we won't be able to help you investigate email abuse, and you won't see any reporting in Mail Check."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=quarantine; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk;`"
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=quarantine; rua=mailto:dmarc-rua@dmarc.service.gov.uk;`";

            var testDmarcRecord = CreateDmarcRecord(record);

            await Test(testDmarcRecord, true, MessageType.info, markDown);
        }

        [TestCase("v=DMARC1;p=reject;adkim=s;aspf=s;fo=1;rua=mailto:rua@service.gov.uk;ruf=mailto:dmarc-ruf@dmarc.service.gov.uk;")]
        [TestCase("v=DMARC1;   ;  p=reject;adkim=s;aspf=s;fo=1;rua=mailto:rua@service.gov.uk;ruf=mailto:dmarc-ruf@dmarc.service.gov.uk;")]
        [TestCase("v=DMARC1 ;p=reject;adkim=s;aspf=s;fo=1;rua=mailto:rua@service.gov.uk;ruf=mailto:dmarc-ruf@dmarc.service.gov.uk;")]
        public async Task UntidyRecordWithNonMailCheckRuaSuggestsNeatRecordWithMailCheckRua(string record)
        {
            string markDown = "The DMARC record does not contain the NCSC Mail Check email address, it's fine to use other tools but be aware that we won't be able to help you investigate email abuse, and you won't see any reporting in Mail Check."
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in Mail Check"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Obtain a Verification Token.** In the Manage Workspaces area, add the domain test.gov.uk to a Workspace and click Verify."
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following, ensuring you replace **INSERT_TOKEN_HERE** with the Verification Token:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=reject; adkim=s; aspf=s; fo=1; rua=mailto:`**`INSERT_TOKEN_HERE`**`@dmarc-rua.mailcheck.service.ncsc.gov.uk,mailto:rua@service.gov.uk; ruf=mailto:dmarc-ruf@dmarc.service.gov.uk; `"
                + $"{Environment.NewLine}{Environment.NewLine}------"
                + $"{Environment.NewLine}{Environment.NewLine}### If you manage your domains in MyNCSC"
                + $"{Environment.NewLine}{Environment.NewLine}Reports of email received should be sent to the NCSC Mail Check service `(rua=mailto:dmarc-rua@dmarc.service.ncsc.gov.uk)`"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 1: Ensure domain is verified in MyNCSC.** Ensure this domain is added and verified in the [asset area of MyNCSC.](https://my.ncsc.gov.uk/)"
                + $"{Environment.NewLine}{Environment.NewLine}**Step 2: Alter your DMARC record.** If you would like Mail Check to receive a copy of your reports, then please change your record to the following:"
                + $"{Environment.NewLine}{Environment.NewLine}`v=DMARC1; p=reject; adkim=s; aspf=s; fo=1; rua=mailto:dmarc-rua@dmarc.service.gov.uk,mailto:rua@service.gov.uk; ruf=mailto:dmarc-ruf@dmarc.service.gov.uk; `";

            var testDmarcRecord = CreateDmarcRecord(record);

            await Test(testDmarcRecord, true, MessageType.info, markDown);
        }

        [Test]
        public async Task NoErrorWhenNewRuaMailboxFoundWithOtherInvalidMailbox()
        {
            
            ReportUriAggregate tag = CreateReportUriAggregate(
                new Uri("mailto:rua@DMARC.service.gov.uk"), 
                new Uri("mailto:dmarc123@dmarc-rua.mailcheck.service.ncsc.gov.uk"));
            
            string record = "v=DMARC1;p=reject;adkim=s;aspf=s;fo=1;rua=mailto:dmarc-ruf@dmarc.service.gov.uk;";

            var testDmarcRecord = CreateDmarcRecord(record, tag);

            await Test(testDmarcRecord, false);
        }

        [Test]
        public async Task NoErrorWhenNewRuaMailboxFoundByItSelf()
        {

            ReportUriAggregate tag = CreateReportUriAggregate(
                new Uri("mailto:dmarc123@dmarc-rua.mailcheck.service.ncsc.gov.uk"));

            string record = "v=DMARC1;p=reject;adkim=s;aspf=s;fo=1;rua=mailto:dmarc-ruf@dmarc.service.gov.uk;";

            var testDmarcRecord = CreateDmarcRecord(record, tag);

            await Test(testDmarcRecord, false);
        }

        [Test]
        public async Task NoErrorWhenCorrectMailboxIsUsed()
        {
            var testDmarc = CreateDmarcRecord(
                CreateReportUriAggregate(new Uri("mailto:dmarc-rua@dmarc.service.gov.uk")));

            await Test(testDmarc, false);
        }

        [Test]
        public async Task WarningWhenNoUris()
        {
            var testDmarc = CreateDmarcRecord(CreateReportUriAggregate());

            await Test(testDmarc, true, MessageType.info);
        }

        [Test]
        public void NoExceptionWhenNullUri()
        {
            var testDmarc = CreateDmarcRecord(
                CreateReportUriAggregate(new Uri("mailto:dmarc-rua@dmarc.service.gov.uk"), null));

            Assert.DoesNotThrowAsync(async () => await Test(testDmarc, false));
        }

        private static DmarcRecord CreateDmarcRecord(params Tag[] tags)
        {
            return new DmarcRecord("", tags.ToList(), new List<Message>(), string.Empty, string.Empty, false, false);
        }

        private static DmarcRecord CreateDmarcRecord(string record = "", params Tag[] tags)
        {
            return new DmarcRecord(record, tags.ToList(), new List<Message>(),  "test.gov.uk", string.Empty, false, false);
        }

        private static ReportUriAggregate CreateReportUriAggregate(params Uri[] uris)
        {
            return new ReportUriAggregate("", uris?.Select(_ => new UriTag("", new DmarcUri(_, true), new MaxReportSize(1000, Unit.K, true), true)).ToList(), true);
        }
    }
}
