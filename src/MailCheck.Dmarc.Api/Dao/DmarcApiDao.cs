using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MailCheck.Dmarc.Api.Domain;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Dmarc.Api.Dao
{
    public interface IDmarcApiDao
    {
        Task<List<DmarcInfoResponse>> GetDmarcForDomains(List<string> domains);
        Task<DmarcInfoResponse> GetDmarcForDomain(string domain);
        Task<string> GetDmarcHistory(string domain);
    }

    public class DmarcApiDao : IDmarcApiDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public DmarcApiDao(IConnectionInfoAsync connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<List<DmarcInfoResponse>> GetDmarcForDomains(List<string> domain)
        {
            string query = string.Format(DmarcApiDaoResources.SelectDmarcStates,
                string.Join(',', domain.Select((_, i) => $"@domain{i}")));

            MySqlParameter[] parameters = domain
                .Select((_, i) => new MySqlParameter($"domain{i}", _))
                .ToArray();

            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(connectionString, query, parameters))
            {
                List<DmarcInfoResponse> states = new List<DmarcInfoResponse>();

                while (await reader.ReadAsync())
                {
                    if (!reader.IsDbNull("state"))
                    {
                        states.Add(JsonConvert.DeserializeObject<DmarcInfoResponse>(reader.GetString("state"), _serializerSettings));
                    }
                }

                return states;
            }
        }

        public async Task<DmarcInfoResponse> GetDmarcForDomain(string domain)
        {
            List<DmarcInfoResponse> responses = await GetDmarcForDomains(new List<string>{domain});
            return responses.FirstOrDefault();
        }

        public async Task<string> GetDmarcHistory(string domain)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            return (string) await MySqlHelper.ExecuteScalarAsync(connectionString, 
                DmarcApiDaoResources.SelectDmarcHistoryStates, new MySqlParameter("domain", domain));
        }
    }
}