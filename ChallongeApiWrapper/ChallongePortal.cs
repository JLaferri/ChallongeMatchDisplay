using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using System.Xml.Linq;

namespace Fizzi.Libraries.ChallongeApiWrapper
{
    public sealed class ChallongePortal
    {
        private readonly RestClient client;

        public string ApiKey { get; private set; }

        public ChallongePortal(string apiKey)
        {
            client = new RestClient(@"https://api.challonge.com/v1/");
            ApiKey = apiKey;
        }

        private void throwOnError(IRestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed || response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ChallongeApiException(response);
            }
        }

        public IEnumerable<Tournament> GetTournaments()
        {
            var request = new RestRequest("tournaments.xml", Method.GET);
            request.AddParameter("api_key", ApiKey);

            var response = client.Execute<List<Tournament>>(request);
            throwOnError(response);

            return response.Data;
        }

        public Tournament ShowTournament(int tournamentId)
        {
            var request = new RestRequest(string.Format("tournaments/{0}.xml", tournamentId), Method.GET);
            request.AddParameter("api_key", ApiKey);

            var response = client.Execute<Tournament>(request);
            throwOnError(response);

            return response.Data;
        }

        public IEnumerable<Match> GetMatches(int tournamentId)
        {
            var request = new RestRequest(string.Format("tournaments/{0}/matches.xml", tournamentId), Method.GET);
            request.AddParameter("api_key", ApiKey);

            var response = client.Execute<List<Match>>(request);
            throwOnError(response);

            return response.Data;
        }

        public IEnumerable<Participant> GetParticipants(int tournamentId)
        {
            var request = new RestRequest(string.Format("tournaments/{0}/participants.xml", tournamentId), Method.GET);
            request.AddParameter("api_key", ApiKey);

            var response = client.Execute<List<Participant>>(request);
            throwOnError(response);

            return response.Data;
        }
    }
}
