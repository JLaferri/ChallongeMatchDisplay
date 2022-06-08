using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using System.Xml.Linq;
using RestSharp.Serializers.Json;
using RestSharp.Serializers.NewtonsoftJson;
using System.Text.Json;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;

namespace Fizzi.Libraries.ChallongeApiWrapper
{
    public sealed class ChallongePortal
    {
        private readonly RestClient client;

        public string ApiKey { get; private set; }
        public string Subdomain { get; private set; }

        public ChallongePortal(string apiKey) : this(apiKey, null) { }

        public ChallongePortal(string apiKey, string subdomain)
        {
            client = new RestClient(@"https://api.challonge.com/v1/");
            ApiKey = apiKey;
            Subdomain = subdomain;
        }

        private void throwOnError(RestResponse response)
        {
            if (response.ResponseStatus != ResponseStatus.Completed || response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new ChallongeApiException(response);
            }
        }

        [DataContract(Name = "tournament")]
        private class TournamentWrapper
        {
            [DataMember(Name = "tournament")]
            public ChallongeTournament Tournament;
        }

        public IEnumerable<ChallongeTournament> GetTournaments()
        {
            var request = new RestRequest("tournaments.json", Method.Get);
            request.AddParameter("api_key", ApiKey);
            if (!string.IsNullOrWhiteSpace(Subdomain)) request.AddParameter("subdomain", Subdomain);

            // work around challonge api bug
            var response = client.ExecuteAsync(request).GetAwaiter().GetResult();
            throwOnError(response);

            var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(response.Content));
            var ser = new DataContractJsonSerializer(typeof(List<TournamentWrapper>));
            var tournaments = ser.ReadObject(ms) as List<TournamentWrapper>;
            ms.Close();

            return tournaments.Select(x => x.Tournament).Reverse();
        }

        public ChallongeTournament ShowTournament(int tournamentId)
        {
            var request = new RestRequest(string.Format("tournaments/{0}.xml", tournamentId), Method.Get);
            request.AddParameter("api_key", ApiKey);

            var response = client.ExecuteAsync<ChallongeTournament>(request).GetAwaiter().GetResult();
            throwOnError(response);

            return response.Data;
        }

        public IEnumerable<ChallongeMatch> GetMatches(int tournamentId)
        {
            var request = new RestRequest(string.Format("tournaments/{0}/matches.xml", tournamentId), Method.Get);
            request.AddParameter("api_key", ApiKey);

            var response = client.ExecuteAsync<List<ChallongeMatch>>(request).GetAwaiter().GetResult();
            throwOnError(response);
            
            return response.Data;
        }

        [DataContract(Name ="participant")]
        private class ParticipantWrapper
        {
            [DataMember(Name = "participant")]
            public ChallongeParticipant Participant;
        }

        public IEnumerable<ChallongeParticipant> GetParticipants(int tournamentId)
        {
            var request = new RestRequest(string.Format("tournaments/{0}/participants.json", tournamentId), Method.Get);
            request.AddParameter("api_key", ApiKey);

            // work around challonge api bug
            var response = client.ExecuteAsync(request).GetAwaiter().GetResult();
            throwOnError(response);

            var ms = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(response.Content));
            var ser = new DataContractJsonSerializer(typeof(List<ParticipantWrapper>));
            var participants = ser.ReadObject(ms) as List<ParticipantWrapper>;
            ms.Close();

            return participants.Select(x => x.Participant);
        }

        public void SetParticipantMisc(int tournamentId, int participantId, string misc)
        {
            var request = new RestRequest(string.Format("tournaments/{0}/participants/{1}.xml", tournamentId, participantId), Method.Put);
            request.AddParameter("api_key", ApiKey);
            request.AddParameter("participant[misc]", misc);

            var response = client.ExecuteAsync(request).GetAwaiter().GetResult();
            throwOnError(response);
        }

        public void ReportMatchWinner(int tournamentId, int matchId, int winnerId, params SetScore[] scores)
        {
            var request = new RestRequest(string.Format("tournaments/{0}/matches/{1}.xml", tournamentId, matchId), Method.Put);
            request.AddParameter("api_key", ApiKey);
            request.AddParameter("match[winner_id]", winnerId);

            if (scores == null || scores.Length == 0) scores = new SetScore[] { new SetScore { Player1Score = 0, Player2Score = 0 } };

            request.AddParameter("match[scores_csv]", scores.Select(ss => ss.ToString()).Aggregate((one, two) => string.Format("{0},{1}", one, two)));

            var response = client.ExecuteAsync(request).GetAwaiter().GetResult();
            throwOnError(response);
        }

        public void EndTournament(int tournamentId)
		{
			var request = new RestRequest(string.Format("tournaments/{0}/finalize.xml", tournamentId), Method.Post);
			request.AddParameter("api_key", ApiKey);

			var response = client.ExecuteAsync<List<ChallongeParticipant>>(request).GetAwaiter().GetResult();
			throwOnError(response);
		}
    }
}
