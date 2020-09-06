﻿using FoxIDs.Models;
using FoxIDs.Repository;
using ITfoxtec.Identity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FoxIDs.Logic
{
    public  class TrackLogic : LogicBase
    {
        const string loginName = "login";
        private readonly ITenantRepository tenantRepository;
        private readonly IHttpContextAccessor httpContextAccessor;

        public TrackLogic(ITenantRepository tenantRepository, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            this.tenantRepository = tenantRepository;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task CreateTrackDocumentAsync(Track mTrack)
        {
            var certificate = await mTrack.Name.CreateSelfSignedCertificateAsync();
            mTrack.PrimaryKey = new TrackKey()
            {
                Type = TrackKeyType.Contained,
                Key = await certificate.ToJsonWebKeyAsync(true)
            };
            await tenantRepository.CreateAsync(mTrack);
        }

        public async Task CreateLoginDocumentAsync(Track mTrack)
        {
            var mLoginUpParty = new LoginUpParty
            {
                Name = loginName,
                EnableCreateUser = true,
                EnableCancelLogin = false,
                SessionLifetime = 900,
                PersistentSessionLifetimeUnlimited = false,
                LogoutConsent = LoginUpPartyLogoutConsent.IfRequered
            };
            await mLoginUpParty.SetIdAsync(new Party.IdKey { TenantName = RouteBinding.TenantName, TrackName = mTrack.Name, PartyName = loginName });

            await tenantRepository.CreateAsync(mLoginUpParty);
        }
    }
}