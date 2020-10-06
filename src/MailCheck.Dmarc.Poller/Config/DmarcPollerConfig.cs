﻿using System;
using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Dmarc.Poller.Config
{
    public interface IDmarcPollerConfig
    {
        string SnsTopicArn { get; }
        TimeSpan DnsRecordLookupTimeout { get; }
        string NameServer { get; }
        bool AllowNullResults { get; }
    }

    public class DmarcPollerConfig : IDmarcPollerConfig
    {
        public DmarcPollerConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            DnsRecordLookupTimeout = TimeSpan.FromSeconds(environmentVariables.GetAsLong("DnsRecordLookupTimeoutSeconds"));
            NameServer = environmentVariables.Get("NameServer", false);
            AllowNullResults = environmentVariables.GetAsBoolOrDefault("AllowNullResults");
        }

        public string SnsTopicArn { get; }
        public TimeSpan DnsRecordLookupTimeout { get; }
        public string NameServer { get; }
        public bool AllowNullResults { get; }
    }
}
