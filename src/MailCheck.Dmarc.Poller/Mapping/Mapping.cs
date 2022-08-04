using System.Linq;
using MailCheck.Dmarc.Contracts;
using MailCheck.Dmarc.Contracts.Poller;

using ContractTag = MailCheck.Dmarc.Contracts.SharedDomain.Tag;
using ContractAdkim = MailCheck.Dmarc.Contracts.SharedDomain.Adkim;
using ContractAlignmentType = MailCheck.Dmarc.Contracts.SharedDomain.AlignmentType;
using ContractAspf = MailCheck.Dmarc.Contracts.SharedDomain.Aspf;
using ContractDmarcRecord = MailCheck.Dmarc.Contracts.SharedDomain.DmarcRecord;
using ContractDmarcRecords = MailCheck.Dmarc.Contracts.SharedDomain.DmarcRecords;
using ContractDmarcUri = MailCheck.Dmarc.Contracts.SharedDomain.DmarcUri;
using ContractFailureOption = MailCheck.Dmarc.Contracts.SharedDomain.FailureOption;
using ContractFailureOptionType = MailCheck.Dmarc.Contracts.SharedDomain.FailureOptionType;
using ContractMaxReportSize = MailCheck.Dmarc.Contracts.SharedDomain.MaxReportSize;
using ContractMessage = MailCheck.Dmarc.Contracts.SharedDomain.Message;
using ContractMessageType = MailCheck.Dmarc.Contracts.SharedDomain.MessageType;
using ContractPercent = MailCheck.Dmarc.Contracts.SharedDomain.Percent;
using ContractPolicy = MailCheck.Dmarc.Contracts.SharedDomain.Policy;
using ContractPolicyType = MailCheck.Dmarc.Contracts.SharedDomain.PolicyType;
using ContractReportFormat = MailCheck.Dmarc.Contracts.SharedDomain.ReportFormat;
using ContractReportFormatType = MailCheck.Dmarc.Contracts.SharedDomain.ReportFormatType;
using ContractReportInterval = MailCheck.Dmarc.Contracts.SharedDomain.ReportInterval;
using ContractReportUriAggregate = MailCheck.Dmarc.Contracts.SharedDomain.ReportUriAggregate;
using ContractReportUriForensic = MailCheck.Dmarc.Contracts.SharedDomain.ReportUriForensic;
using ContractSubDomainPolicy = MailCheck.Dmarc.Contracts.SharedDomain.SubDomainPolicy;
using ContractUnit = MailCheck.Dmarc.Contracts.SharedDomain.Unit;
using ContractUriTag = MailCheck.Dmarc.Contracts.SharedDomain.UriTag;
using ContractUnknownTag = MailCheck.Dmarc.Contracts.SharedDomain.UnknownTag;
using ContractVersion = MailCheck.Dmarc.Contracts.SharedDomain.Version;

using Adkim = MailCheck.Dmarc.Poller.Domain.Adkim;
using AlignmentType = MailCheck.Dmarc.Poller.Domain.AlignmentType;
using Aspf = MailCheck.Dmarc.Poller.Domain.Aspf;
using DmarcPollResult = MailCheck.Dmarc.Poller.Domain.DmarcPollResult;
using DmarcRecord = MailCheck.Dmarc.Poller.Domain.DmarcRecord;
using DmarcRecords = MailCheck.Dmarc.Poller.Domain.DmarcRecords;
using DmarcUri = MailCheck.Dmarc.Poller.Domain.DmarcUri;
using Error = MailCheck.Dmarc.Poller.Domain.Error;
using ErrorType = MailCheck.Dmarc.Poller.Domain.ErrorType;
using FailureOption = MailCheck.Dmarc.Poller.Domain.FailureOption;
using FailureOptionType = MailCheck.Dmarc.Poller.Domain.FailureOptionType;
using MaxReportSize = MailCheck.Dmarc.Poller.Domain.MaxReportSize;
using Percent = MailCheck.Dmarc.Poller.Domain.Percent;
using Policy = MailCheck.Dmarc.Poller.Domain.Policy;
using PolicyType = MailCheck.Dmarc.Poller.Domain.PolicyType;
using ReportFormat = MailCheck.Dmarc.Poller.Domain.ReportFormat;
using ReportFormatType = MailCheck.Dmarc.Poller.Domain.ReportFormatType;
using ReportInterval = MailCheck.Dmarc.Poller.Domain.ReportInterval;
using ReportUriAggregate = MailCheck.Dmarc.Poller.Domain.ReportUriAggregate;
using ReportUriForensic = MailCheck.Dmarc.Poller.Domain.ReportUriForensic;
using SubDomainPolicy = MailCheck.Dmarc.Poller.Domain.SubDomainPolicy;
using Tag = MailCheck.Dmarc.Poller.Domain.Tag;
using Unit = MailCheck.Dmarc.Poller.Domain.Unit;
using UriTag = MailCheck.Dmarc.Poller.Domain.UriTag;
using Version = MailCheck.Dmarc.Poller.Domain.Version;
using System;
using System.Collections.Generic;

namespace MailCheck.Dmarc.Poller.Mapping
{
    public static class Mapping
    {
        public static DmarcRecordsPolled ToDmarcRecordsPolled(this DmarcPollResult dmarcPollResult)
        {
            return new DmarcRecordsPolled(dmarcPollResult.Records.Domain,
                dmarcPollResult.Records.ToContract(),
                dmarcPollResult.Elapsed,
                dmarcPollResult.Errors.Select(_ => _.ToContract()).ToList());
        }

        private static ContractDmarcRecords ToContract(this DmarcRecords dmarcRecords)
        {
            return new ContractDmarcRecords(dmarcRecords.Domain,
                dmarcRecords.Records.Select(x => x.ToContractDmarcRecord()).ToList(),
                dmarcRecords.Errors.Select(_ => _.ToContract()).ToList(), dmarcRecords.MessageSize,
                dmarcRecords.OrgDomain, dmarcRecords.IsTld, dmarcRecords.IsInherited);
        }

        private static ContractDmarcRecord ToContractDmarcRecord(this DmarcRecord dmarcRecord)
        {
            return new ContractDmarcRecord(dmarcRecord.Record, dmarcRecord.Tags.Select(x => x.ToContract()).ToList(),
                dmarcRecord.AllErrors.Select(_ => _.ToContract()).ToList(),
                dmarcRecord.Domain, dmarcRecord.OrgDomain,
                dmarcRecord.IsTld, dmarcRecord.IsInherited);
        }

        private static ContractTag ToContract(this Tag tag)
        {
            switch (tag)
            {
                case Adkim adkim:
                    return adkim.ToContract();
                case Aspf aspf:
                    return aspf.ToContract();
                case FailureOption failureOption:
                    return failureOption.ToContract();
                case Percent percent:
                    return percent.ToContract();
                case Policy policy:
                    return policy.ToContract();
                case ReportFormat reportFormat:
                    return reportFormat.ToContract();
                case ReportInterval reportInterval:
                    return reportInterval.ToContract();
                case ReportUriAggregate reportUriAggregate:
                    return reportUriAggregate.ToContract();
                case ReportUriForensic reportUriForensic:
                    return reportUriForensic.ToContract();
                case SubDomainPolicy subDomainPolicy:
                    return subDomainPolicy.ToContract();
                case Version version:
                    return version.ToContract();
            }

            return new ContractUnknownTag(tag.GetType().ToString(), tag.Value, tag.AllValid);
        }

        private static ContractMessage ToContract(this Error error)
        {
            return new ContractMessage(error.Id, error.Name, MessageSources.DmarcPoller, error.ErrorType.ToContract(), error.Message, error.Markdown);
        }

        private static ContractMessageType ToContract(this ErrorType errorType)
        {
            switch (errorType)
            {
                case ErrorType.Error:
                    return ContractMessageType.error;
                case ErrorType.Warning:
                    return ContractMessageType.warning;
                case ErrorType.Info:
                    return ContractMessageType.info;
                default:
                    return ContractMessageType.error;
            }
        }

        private static ContractAdkim ToContract(this Adkim adkim)
        {
            return new ContractAdkim(adkim.Value,adkim.AlignmentType.ToContract(), adkim.AllValid, adkim.IsImplicit);
        }

        private static ContractAlignmentType ToContract(this AlignmentType alignmentType)
        {
            switch (alignmentType)
            {
                case AlignmentType.R:
                    return ContractAlignmentType.R;
                case AlignmentType.S:
                    return ContractAlignmentType.S;
                default:
                    return ContractAlignmentType.Unknown;
            }
        }

        private static ContractAspf ToContract(this Aspf aspf)
        {
            return new ContractAspf(aspf.Value, aspf.AlignmentType.ToContract(), aspf.AllValid, aspf.IsImplicit);
        }

        private static ContractDmarcUri ToContract(this DmarcUri uriTag)
        {
            return new ContractDmarcUri(uriTag.Uri, uriTag.AllValid);
        }

        private static ContractFailureOption ToContract(this FailureOption failureOption)
        {
            return new ContractFailureOption(failureOption.Value, failureOption.FailureOptionTypes.ToContract(), failureOption.AllValid,  failureOption.IsImplicit);
        }

        private static List<ContractFailureOptionType> ToContract(this List<FailureOptionType> failureOptionTypes)
        {
            List<ContractFailureOptionType> contractFailureOptionTypes = new List<ContractFailureOptionType>();

            foreach (FailureOptionType optionType in failureOptionTypes)
            {
                switch (optionType)
                {
                    case FailureOptionType.S:
                        contractFailureOptionTypes.Add(ContractFailureOptionType.S);
                        break;
                    case FailureOptionType.D:
                        contractFailureOptionTypes.Add(ContractFailureOptionType.D);
                        break;
                    case FailureOptionType.One:
                        contractFailureOptionTypes.Add(ContractFailureOptionType.One);
                        break;
                    case FailureOptionType.Zero:
                        contractFailureOptionTypes.Add(ContractFailureOptionType.Zero);
                        break;
                    default:
                        contractFailureOptionTypes.Add(ContractFailureOptionType.Unknown);
                        break;
                }
            }

            return contractFailureOptionTypes;
        }

        private static ContractMaxReportSize ToContract(this MaxReportSize maxReportSize)
        {
            return new ContractMaxReportSize(maxReportSize.Value, maxReportSize.Unit.ToContract(), maxReportSize.AllValid);
        }

        private static ContractPercent ToContract(this Percent percent)
        {
            return new ContractPercent(percent.Value, percent.PercentValue, percent.AllValid, percent.IsImplicit);
        }

        private static ContractPolicy ToContract(this Policy policy)
        {
            return new ContractPolicy(policy.Value, policy.PolicyType.ToContract(), policy.AllValid);
        }

        private static ContractPolicyType ToContract(this PolicyType policyType)
        {
            switch (policyType)
            {
                case PolicyType.None:
                    return ContractPolicyType.None;
                case PolicyType.Quarantine:
                    return ContractPolicyType.Quarantine;
                case PolicyType.Reject:
                    return ContractPolicyType.Reject;
                default:
                    return ContractPolicyType.Unknown;
            }
        }

        private static ContractReportFormat ToContract(this ReportFormat reportFormat)
        {
            return new ContractReportFormat(reportFormat.Value, reportFormat.ReportFormatType.ToContract(), reportFormat.AllValid, reportFormat.IsImplicit);
        }

        private static ContractReportInterval ToContract(this ReportInterval reportInterval)
        {
            return new ContractReportInterval(reportInterval.Value, reportInterval.Interval, reportInterval.AllValid, reportInterval.IsImplicit);
        }

        private static ContractReportUriAggregate ToContract(this ReportUriAggregate reportUriAggregate)
        {
            return new ContractReportUriAggregate(reportUriAggregate.Value, reportUriAggregate.Uris.Select(x => x.ToContract()).ToList(), reportUriAggregate.AllValid);
        }

        private static ContractReportFormatType ToContract(this ReportFormatType reportFormatType)
        {
            switch (reportFormatType)
            {
                case ReportFormatType.AFRF:
                    return ContractReportFormatType.AFRF;
                default:
                    return ContractReportFormatType.Unknown;
            }
        }

        private static ContractReportUriForensic ToContract(this ReportUriForensic reportUriForensic)
        {
            return new ContractReportUriForensic(reportUriForensic.Value, reportUriForensic.Uris.Select(x=>x.ToContract()).ToList(), reportUriForensic.AllValid);
        }

        private static ContractSubDomainPolicy ToContract(this SubDomainPolicy subDomainPolicy)
        {
            return new ContractSubDomainPolicy(subDomainPolicy.Value, subDomainPolicy.PolicyType.ToContract(), subDomainPolicy.AllValid, subDomainPolicy.IsImplicit);
        }

        private static ContractUnit ToContract(this Unit unit)
        {
            switch (unit)
            {
                case Unit.B:
                    return ContractUnit.B;
                case Unit.G:
                    return ContractUnit.G;
                case Unit.K:
                    return ContractUnit.K;
                case Unit.M:
                    return ContractUnit.M;
                case Unit.T:
                    return ContractUnit.T;
                default:
                    return ContractUnit.Unknown;
            }
        }

        private static ContractUriTag ToContract(this UriTag uriTag)
        {
            return new ContractUriTag(uriTag.Value, uriTag.Uri.ToContract(), uriTag.MaxReportSize?.ToContract(), uriTag.AllValid);
        }

        private static ContractVersion ToContract(this Version version)
        {
            return new ContractVersion(version.Value, version.AllValid);
        }
    }
}
