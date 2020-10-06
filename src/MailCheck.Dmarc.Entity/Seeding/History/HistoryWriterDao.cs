using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Dmarc.Contracts.SharedDomain.Serialization;
using MailCheck.Dmarc.Entity.Entity;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dmarc.Entity.Seeding.History
{
    public interface IHistoryWriterDao
    {
        Task WriteHistory(List<DmarcRecordState> dmarcEntityStates);
    }

    public class HistoryWriterDao : IHistoryWriterDao
    {
        private readonly IConnectionInfo _connectionInfo;

        public HistoryWriterDao(IConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;

            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings serializerSetting = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };

                serializerSetting.Converters.Add(new StringEnumConverter());
                serializerSetting.Converters.Add(new TagConverter());

                return serializerSetting;
            };
        }

        public async Task WriteHistory(List<DmarcRecordState> dmarcEntityStates)
        {
            string commandStart = "INSERT INTO `dmarc_entity_history`\r\n(`id`,\r\n`state`)\r\nVALUES";

            string parameterNames = string.Join(",", dmarcEntityStates.Select((v, i) => $"(@domain{i}, @state{i})"));

            string command = $"{commandStart} {parameterNames};";

            MySqlParameter[] parameters = dmarcEntityStates.Select((v, i) => new List<MySqlParameter>
            {
                new MySqlParameter($"domain{i}", v.Id),
                new MySqlParameter($"state{i}", JsonConvert.SerializeObject(v))
            }).SelectMany(_ => _).ToArray();

            await MySqlHelper.ExecuteNonQueryAsync(_connectionInfo.ConnectionString, command, parameters);
        }
    }
}