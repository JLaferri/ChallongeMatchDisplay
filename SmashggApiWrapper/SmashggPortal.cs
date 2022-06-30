using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using RestSharp;

using SimpleJson = Newtonsoft.Json.JsonConvert;

namespace Fizzi.Libraries.SmashggApiWrapper;

public sealed class SmashggPortal
{
	private readonly RestClient client;

	public string AccessToken { get; private set; }

	public string Slug { get; private set; }

	public SmashggPortal(string accessToken, string slug)
	{
		client = new RestClient("https://api.smash.gg/gql/alpha");
		AccessToken = accessToken;
		Slug = slug;
	}

	private void throwOnError(RestResponse response)
	{
		if (response.ResponseStatus != ResponseStatus.Completed || response.StatusCode != HttpStatusCode.OK)
		{
			throw new SmashggApiException(response);
		}
	}

	public IEnumerable<SmashggTournament> GetTournaments()
	{
		RestRequest restRequest = new RestRequest("/", Method.Post);
		restRequest.RequestFormat = DataFormat.Json;
		restRequest.AddHeader("Authorization", "Bearer " + AccessToken);
		restRequest.AddParameter("query", "\n                query GetUser($slug: String!) {\n                    user(slug:$slug) {\n                        id\n                    }\n                }\n            ");
		restRequest.AddParameter("variables", SimpleJson.SerializeObject(new
		{
			slug = Slug.Trim()
		}));
		dynamic val = SimpleJson.DeserializeObject(client.ExecuteAsync(restRequest).GetAwaiter().GetResult().Content);
		throwOnError(val);
		long ownerId = val["data"]["user"]["id"];
		restRequest.RemoveParameter(restRequest.Parameters.TryFind("query"));
		restRequest.RemoveParameter(restRequest.Parameters.TryFind("variables"));
		restRequest.AddParameter("query", "\n                query TournamentsByOwner($perPage: Int!, $ownerId: ID!) {\n                    tournaments(query: {\n                        perPage: $perPage\n                        filter: {\n                            past: false                            ownerId: $ownerId\n                        }\n                    }) {\n                    nodes {\n                        createdAt\n                        startAt\n                        name\n                        id\n                        state\n                        tournamentType\n                        url(tab: \"details\", relative: false)\n                        events(limit: 12) {\n                            id\n                            name\n                            numEntrants\n                            phases(state: ACTIVE) {\n                                id\n                                name\n                                phaseGroups {\n                                    nodes {\n                                        id\n                                        displayIdentifier\n                                        state\n                                        bracketType\n                                    }\n                                }\n                            }\n                        }\n                    }\n                  }\n                }\n            ");
		restRequest.AddParameter("variables", SimpleJson.SerializeObject(new
		{
			perPage = 4,
			ownerId = ownerId
		}));
		val = client.ExecuteAsync(restRequest).GetAwaiter().GetResult();
		throwOnError(val);
		List<JToken> list = ((JObject)JObject.Parse(val.Content))["data"]!["tournaments"]!["nodes"]!.Children().ToList();
		IList<SmashggTournament> list2 = new List<SmashggTournament>();
		foreach (JToken item2 in (IEnumerable<JToken>)list)
		{
			SmashggTournament item = item2.ToObject<SmashggTournament>();
			list2.Add(item);
		}
		return list2;
	}

	public SmashggTournament ShowTournament(long tournamentId)
	{
		RestRequest restRequest = new RestRequest("/", Method.Post);
		restRequest.AddHeader("Authorization", "Bearer " + AccessToken);
		restRequest.AddParameter("query", "\n                query($tournamentId: ID!) {\n                    tournament(id: $tournamentId) {\n                        createdAt\n                        startAt\n                        name\n                        id\n                        state\n                        tournamentType\n                        url(tab: \"details\", relative: false)\n                        events(limit: 12) {\n                            id\n                            name\n                            numEntrants\n                            phases(state: ACTIVE) {\n                                id\n                                name\n                                phaseGroups {\n                                    nodes {\n                                        id\n                                        displayIdentifier\n                                        state\n                                        bracketType\n                                    }\n                                }\n                            }\n                        }\n                    }\n                }\n            ");
		restRequest.AddParameter("variables", SimpleJson.SerializeObject(new { tournamentId }));
		RestResponse restResponse = client.ExecuteAsync(restRequest).GetAwaiter().GetResult();
		throwOnError(restResponse);
		SmashggTournament result = JObject.Parse(restResponse.Content)["data"]!["tournament"]!.ToObject<SmashggTournament>();
		return result;
	}

	public SmashggPhaseGroup ShowPhaseGroup(long phaseGroupId)
	{
		RestRequest restRequest = new RestRequest("/", Method.Post);
		restRequest.AddHeader("Authorization", "Bearer " + AccessToken);
		restRequest.AddParameter("query", "\n                query($phaseGroupId: ID!) {\n                    phaseGroup(id: $phaseGroupId) {\n                        id\n                        displayIdentifier\n                        state\n                        bracketType\n                    }\n                }\n            ");
		restRequest.AddParameter("variables", SimpleJson.SerializeObject(new { phaseGroupId }));
		RestResponse restResponse = client.ExecuteAsync(restRequest).GetAwaiter().GetResult();
		throwOnError(restResponse);
		SmashggPhaseGroup result = JObject.Parse(restResponse.Content)["data"]!["phaseGroup"]!.ToObject<SmashggPhaseGroup>();
		return result;
	}

	public SmashggPhase ShowPhase(long phaseId)
	{
		RestRequest restRequest = new RestRequest("/", Method.Post);
		restRequest.AddHeader("Authorization", "Bearer " + AccessToken);
		restRequest.AddParameter("query", "\n                query($phaseId: ID!) {\n                    phase(id: $phaseId) {\n                        id\n                        name\n                        phaseGroups {\n                            nodes {\n                                id\n                                displayIdentifier\n                                state\n                            }\n                        }\n                    }\n                }\n            ");
		restRequest.AddParameter("variables", SimpleJson.SerializeObject(new { phaseId }));
		RestResponse restResponse = client.ExecuteAsync(restRequest).GetAwaiter().GetResult();
		throwOnError(restResponse);
		SmashggPhase result = JObject.Parse(restResponse.Content)["data"]!["phase"]!.ToObject<SmashggPhase>();
		return result;
	}

	public IEnumerable<SmashggMatch> GetMatches(long phaseGroupId)
	{
		RestRequest restRequest = new RestRequest("/", Method.Post);
		restRequest.AddHeader("Authorization", "Bearer " + AccessToken);
		restRequest.AddParameter("query", "\n                query($phaseGroupId: ID!) {\n                    phaseGroup(id: $phaseGroupId) {\n                        sets(page: 1, perPage: 100, sortType: MAGIC) {\n                            nodes {\n                                id\n                                slots {\n                                    id\n                                    entrant {\n                                        id\n                                        name\n                                        participants {\n                                            id\n                                            gamerTag\n                                            checkedIn\n                                        }\n                                    }\n                                    prereqType\n                                    prereqId\n                                    prereqPlacement\n                                }\n                                winnerId\n                                round\n                                state\n                                identifier\n                                fullRoundText\n                                station {\n                                    id\n                                    number\n                                    identifier\n                                }\n                                startedAt\n                                createdAt\n                            }\n                        }\n                    }\n                }\n            ");
		restRequest.AddParameter("variables", SimpleJson.SerializeObject(new { phaseGroupId }));
		RestResponse restResponse = client.ExecuteAsync(restRequest).GetAwaiter().GetResult();
		throwOnError(restResponse);
		List<JToken> list = JObject.Parse(restResponse.Content)["data"]!["phaseGroup"]!["sets"]!["nodes"]!.Children().ToList();
		IList<SmashggMatch> list2 = new List<SmashggMatch>();
		foreach (JToken item in (IEnumerable<JToken>)list)
		{
			SmashggMatch smashggMatch = item.ToObject<SmashggMatch>();
			list2.Add(smashggMatch);
			Console.WriteLine(smashggMatch.StartedAt);
		}
		return list2;
	}

	public void ReportMatchWinner(long tournamentId, string matchId, int winnerId, params SetScore[] scores)
	{
	}

	public void EndPhaseGroup(long phaseGroupId)
	{
	}
}
