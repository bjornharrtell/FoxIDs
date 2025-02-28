﻿using FoxIDs.Client.Logic;
using FoxIDs.Models.Api;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace FoxIDs.Client.Services
{
    public class DownPartyService : BaseService
    {
        private const string newPartyNameApiUri = "api/{tenant}/{track}/!newpartyname";
        private const string listApiUri = "api/{tenant}/{track}/!downparties";
        private const string oauthApiUri = "api/{tenant}/{track}/!oauthdownparty";
        private const string oauthclientsecretApiUri = "api/{tenant}/{track}/!oauthclientsecretdownparty";
        private const string oidcApiUri = "api/{tenant}/{track}/!oidcdownparty";
        private const string oidcclientsecretApiUri = "api/{tenant}/{track}/!oidcclientsecretdownparty";
        private const string samlApiUri = "api/{tenant}/{track}/!samldownparty";
        private const string trackLinkApiUri = "api/{tenant}/{track}/!tracklinkdownparty";

        public DownPartyService(IHttpClientFactory httpClientFactory, RouteBindingLogic routeBindingLogic, TrackSelectedLogic trackSelectedLogic) : base(httpClientFactory, routeBindingLogic, trackSelectedLogic)
        { }

        public async Task<string> GetNewPartyNameAsync() => (await GetAsync<NewPartyName>(newPartyNameApiUri))?.Name;

        public async Task<PaginationResponse<DownParty>> GetDownPartiesAsync(string filterName, string paginationToken = null) => await GetListAsync<DownParty>(listApiUri, filterName, paginationToken: paginationToken);

        public async Task<OAuthDownParty> GetOAuthDownPartyAsync(string name) => await GetAsync<OAuthDownParty>(oauthApiUri, name);
        public async Task<OAuthDownParty> CreateOAuthDownPartyAsync(OAuthDownParty party) => await PostResponseAsync<OAuthDownParty, OAuthDownParty>(oauthApiUri, party);
        public async Task<OAuthDownParty> UpdateOAuthDownPartyAsync(OAuthDownParty party) => await PutResponseAsync<OAuthDownParty, OAuthDownParty>(oauthApiUri, party);
        public async Task DeleteOAuthDownPartyAsync(string name) => await DeleteAsync(oauthApiUri, name);

        public async Task<List<OAuthClientSecretResponse>> GetOAuthClientSecretDownPartyAsync(string partyName) => await GetAsync<List<OAuthClientSecretResponse>>(oauthclientsecretApiUri, partyName, parmName1: nameof(partyName));
        public async Task CreateOAuthClientSecretDownPartyAsync(OAuthClientSecretRequest clientSecret) => await PostAsync(oauthclientsecretApiUri, clientSecret);
        public async Task DeleteOAuthClientSecretDownPartyAsync(string name) => await DeleteAsync(oauthclientsecretApiUri, name);

        public async Task<OidcDownParty> GetOidcDownPartyAsync(string name) => await GetAsync<OidcDownParty>(oidcApiUri, name);
        public async Task<OidcDownParty> CreateOidcDownPartyAsync(OidcDownParty party) => await PostResponseAsync<OidcDownParty, OidcDownParty>(oidcApiUri, party);
        public async Task<OidcDownParty> UpdateOidcDownPartyAsync(OidcDownParty party) => await PutResponseAsync<OidcDownParty, OidcDownParty>(oidcApiUri, party);
        public async Task DeleteOidcDownPartyAsync(string name) => await DeleteAsync(oidcApiUri, name);

        public async Task<List<OAuthClientSecretResponse>> GetOidcClientSecretDownPartyAsync(string partyName) => await GetAsync<List<OAuthClientSecretResponse>>(oauthclientsecretApiUri, partyName, parmName1: nameof(partyName));
        public async Task CreateOidcClientSecretDownPartyAsync(OAuthClientSecretRequest clientSecret) => await PostAsync(oidcclientsecretApiUri, clientSecret);
        public async Task DeleteOidcClientSecretDownPartyAsync(string name) => await DeleteAsync(oidcclientsecretApiUri, name);

        public async Task<SamlDownParty> GetSamlDownPartyAsync(string name) => await GetAsync<SamlDownParty>(samlApiUri, name);
        public async Task<SamlDownParty> CreateSamlDownPartyAsync(SamlDownParty party) => await PostResponseAsync<SamlDownParty, SamlDownParty>(samlApiUri, party);
        public async Task<SamlDownParty> UpdateSamlDownPartyAsync(SamlDownParty party) => await PutResponseAsync<SamlDownParty, SamlDownParty>(samlApiUri, party);
        public async Task DeleteSamlDownPartyAsync(string name) => await DeleteAsync(samlApiUri, name);

        public async Task<TrackLinkDownParty> GetTrackLinkDownPartyAsync(string name, string trackName = null) => await GetAsync<TrackLinkDownParty>(GetApiUrl(trackLinkApiUri, trackName), name);
        public async Task<TrackLinkDownParty> CreateTrackLinkDownPartyAsync(TrackLinkDownParty party, string trackName = null) => await PostResponseAsync<TrackLinkDownParty, TrackLinkDownParty>(GetApiUrl(trackLinkApiUri, trackName), party);
        public async Task<TrackLinkDownParty> UpdateTrackLinkDownPartyAsync(TrackLinkDownParty party) => await PutResponseAsync<TrackLinkDownParty, TrackLinkDownParty>(trackLinkApiUri, party);
        public async Task DeleteTrackLinkDownPartyAsync(string name, string trackName = null) => await DeleteAsync(GetApiUrl(trackLinkApiUri, trackName), name);
    }
}
