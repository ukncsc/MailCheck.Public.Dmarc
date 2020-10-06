using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Dmarc.EntityHistory.Config;
using Microsoft.Extensions.Logging;

namespace MailCheck.Dmarc.EntityHistory.Service
{
    public interface IDmarcRuaService
    {
        void Process(string id, string record);
    }

    public class DmarcRuaService : IDmarcRuaService
    {
        private readonly IDmarcEntityHistoryConfig _dmarcEntityHistoryConfig;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IDmarcRuaValidator _ruaValidator;
        private readonly ILogger<DmarcRuaService> _log;

        public DmarcRuaService(IDmarcEntityHistoryConfig dmarcEntityHistoryConfig,
            IMessageDispatcher dispatcher, IDmarcRuaValidator ruaValidator, ILogger<DmarcRuaService> log)
        {

            _dmarcEntityHistoryConfig = dmarcEntityHistoryConfig;
            _dispatcher = dispatcher;
            _ruaValidator = ruaValidator;
            _log = log;
        }

        public void Process(string id, string record)
        {
            RuaResult result = _ruaValidator.Validate(record);

            if (result.Valid)
            {
                result.Tokens.ForEach(x => _dispatcher.Dispatch(new RuaVerificationChangeFound(id, "DMARC", x),
                    _dmarcEntityHistoryConfig.SnsTopicArn));

                _log.LogInformation("Published Rua found for {Id}", id);
            }
            else
            {
                _log.LogInformation("Invalid Rua found for {Id}", id);
            }
        }
    }
}
