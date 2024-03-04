﻿using FoxIDs.Infrastructure;
using FoxIDs.Models;
using Api = FoxIDs.Models.Api;
using FoxIDs.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using System;
using FoxIDs.Logic;
using FoxIDs.Infrastructure.Security;
using ITfoxtec.Identity.Util;
using ITfoxtec.Identity;

namespace FoxIDs.Controllers
{
    /// <summary>
    /// Abstract connection API.
    /// </summary>
    [TenantScopeAuthorize(Constants.ControlApi.Segment.Party)]
    public abstract class GenericPartyApiController<AParty, AClaimTransform, MParty> : ApiController where AParty : Api.INameValue, Api.IClaimTransform<AClaimTransform> where MParty : Party where AClaimTransform : Api.ClaimTransform
    {
        private readonly TelemetryScopedLogger logger;
        private readonly IMapper mapper;
        private readonly ITenantRepository tenantRepository;
        private readonly DownPartyCacheLogic downPartyCacheLogic;
        private readonly UpPartyCacheLogic upPartyCacheLogic;
        private readonly DownPartyAllowUpPartiesQueueLogic downPartyAllowUpPartiesQueueLogic;
        private readonly ValidateApiModelGenericPartyLogic validateApiModelGenericPartyLogic;
        private readonly ValidateModelGenericPartyLogic validateModelGenericPartyLogic;

        public GenericPartyApiController(TelemetryScopedLogger logger, IMapper mapper, ITenantRepository tenantRepository, DownPartyCacheLogic downPartyCacheLogic, UpPartyCacheLogic upPartyCacheLogic, DownPartyAllowUpPartiesQueueLogic downPartyAllowUpPartiesQueueLogic, ValidateApiModelGenericPartyLogic validateApiModelGenericPartyLogic, ValidateModelGenericPartyLogic validateModelGenericPartyLogic) : base(logger)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.tenantRepository = tenantRepository;
            this.downPartyCacheLogic = downPartyCacheLogic;
            this.upPartyCacheLogic = upPartyCacheLogic;
            this.downPartyAllowUpPartiesQueueLogic = downPartyAllowUpPartiesQueueLogic;
            this.validateApiModelGenericPartyLogic = validateApiModelGenericPartyLogic;
            this.validateModelGenericPartyLogic = validateModelGenericPartyLogic;
        }

        protected async Task<ActionResult<AParty>> Get(string name)
        {
            try
            {
                if (!ModelState.TryValidateRequiredParameter(name, nameof(name))) return BadRequest(ModelState);
                name = name?.ToLower();

                var mParty = await tenantRepository.GetAsync<MParty>(await GetId(IsUpParty(), name));
                return base.Ok(ModelToApiMap(mParty));
            }
            catch (CosmosDataException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.Warning(ex, $"NotFound, Get '{typeof(AParty).Name}' by name '{name}'.");
                    return NotFound(typeof(AParty).Name, name);
                }
                throw;
            }
        }

        protected async Task<ActionResult<AParty>> Post(AParty party, Func<AParty, ValueTask<bool>> apiModelActionAsync = null, Func<AParty, MParty, ValueTask<bool>> preLoadModelActionAsync = null, Func<AParty, MParty, ValueTask<bool>> postLoadModelActionAsync = null)
        {
            try
            {
                if (!await ModelState.TryValidateObjectAsync(party) || !validateApiModelGenericPartyLogic.ValidateApiModelClaimTransforms(ModelState, party.ClaimTransforms) || (apiModelActionAsync != null && !await apiModelActionAsync(party))) return BadRequest(ModelState);
                party.Name = await GetPartyNameAsync(party.Name);

                var mParty = mapper.Map<MParty>(party);
                if (mParty is UpParty)
                {
                    var count = await CountParties("party:up");
                    if (count >= Constants.Models.UpParty.PartiesMax)
                    {
                        throw new Exception($"Maximum number of authentication methods ({Constants.Models.UpParty.PartiesMax}) per environment has been reached.");
                    }
                }
                else if (mParty is DownParty)
                {
                    var count = await CountParties("party:down");
                    if (count >= Constants.Models.DownParty.PartiesMax)
                    {
                        throw new Exception($"Maximum number of application registrations ({Constants.Models.UpParty.PartiesMax}) per environment has been reached.");
                    }
                }
                else
                {
                    throw new NotSupportedException($"{mParty?.GetType()?.Name} type not supported.");
                }

                if (mParty is SamlDownParty samlDownParty && samlDownParty.Issuer.IsNullOrWhiteSpace())
                {
                    samlDownParty.Issuer = GetSamlIssuer(party.Name);
                }

                if (!(party is Api.IDownParty downParty ? await validateModelGenericPartyLogic.ValidateModelAllowUpPartiesAsync(ModelState, nameof(downParty.AllowUpPartyNames), mParty as DownParty) : true)) return BadRequest(ModelState);
                if (!validateModelGenericPartyLogic.ValidateModelClaimTransforms(ModelState, mParty)) return BadRequest(ModelState);
                if (preLoadModelActionAsync != null && !await preLoadModelActionAsync(party, mParty)) return BadRequest(ModelState);
                if (postLoadModelActionAsync != null && !await postLoadModelActionAsync(party, mParty)) return BadRequest(ModelState);

                await tenantRepository.CreateAsync(mParty);

                if (mParty is UpParty)
                {
                    await upPartyCacheLogic.InvalidateUpPartyCacheAsync(party.Name);
                    await downPartyAllowUpPartiesQueueLogic.UpdateUpParty(null, mParty as UpParty);
                }
                else if (mParty is DownParty)
                {
                    await downPartyCacheLogic.InvalidateDownPartyCacheAsync(party.Name);
                }
                else
                {
                    throw new NotSupportedException($"{mParty?.GetType()?.Name} type not supported.");
                }

                return Created(ModelToApiMap(mParty));
            }
            catch (CosmosDataException ex)
            {
                if (ex.StatusCode == HttpStatusCode.Conflict)
                {
                    logger.Warning(ex, $"Conflict, Create '{typeof(AParty).Name}' by name '{party.Name}'.");
                    return Conflict(typeof(AParty).Name, party.Name, nameof(party.Name));
                }
                throw;
            }
        }

        protected async Task<ActionResult<AParty>> Put(AParty party, Func<AParty, ValueTask<bool>> apiModelActionAsync = null, Func<AParty, MParty, ValueTask<bool>> preLoadModelActionAsync = null, Func<AParty, MParty, ValueTask<bool>> postLoadModelActionAsync = null)
        {
            try
            {
                if (!await ModelState.TryValidateObjectAsync(party) || !validateApiModelGenericPartyLogic.ValidateApiModelClaimTransforms(ModelState, party.ClaimTransforms) || (apiModelActionAsync != null && !await apiModelActionAsync(party))) return BadRequest(ModelState);
                party.Name = await GetPartyNameAsync(party.Name);

                var mParty = mapper.Map<MParty>(party);

                if (!(party is Api.IDownParty downParty ? await validateModelGenericPartyLogic.ValidateModelAllowUpPartiesAsync(ModelState, nameof(downParty.AllowUpPartyNames), mParty as DownParty) : true)) return BadRequest(ModelState);
                if (!validateModelGenericPartyLogic.ValidateModelClaimTransforms(ModelState, mParty)) return BadRequest(ModelState);
                if (preLoadModelActionAsync != null && !await preLoadModelActionAsync(party, mParty)) return BadRequest(ModelState);

                if (mParty is OidcDownParty mOidcDownParty)
                {
                    var tempMParty = await tenantRepository.GetAsync<OidcDownParty>(mParty.Id);
                    if(tempMParty.Client != null && mOidcDownParty.Client != null)
                    {
                        mOidcDownParty.Client.Secrets = tempMParty.Client.Secrets;
                    }
                }
                else if (mParty is OAuthDownParty mOAuthDownParty)
                {
                    var tempMParty = await tenantRepository.GetAsync<OAuthDownParty>(mParty.Id);
                    if (tempMParty.Client != null && mOAuthDownParty.Client != null)
                    {
                        mOAuthDownParty.Client.Secrets = tempMParty.Client.Secrets;
                    }
                }
                else if (mParty is OidcUpParty mOidcUpParty)
                {
                    var tempMParty = await tenantRepository.GetAsync<OidcUpParty>(mParty.Id);
                    mOidcUpParty.Client.ClientSecret = tempMParty.Client.ClientSecret;
                    mOidcUpParty.Client.ClientKeys = tempMParty.Client.ClientKeys;
                }

                if (mParty is SamlDownParty samlDownParty && samlDownParty.Issuer.IsNullOrWhiteSpace())
                {
                    samlDownParty.Issuer = GetSamlIssuer(party.Name);
                }

                if (postLoadModelActionAsync != null && !await postLoadModelActionAsync(party, mParty)) return BadRequest(ModelState);

                var oldMUpParty = (mParty is UpParty mUpParty) ? await tenantRepository.GetAsync<UpParty>(await UpParty.IdFormatAsync(RouteBinding, mParty.Name)) : null;
                await tenantRepository.UpdateAsync(mParty);

                if (mParty is UpParty)
                {
                    await upPartyCacheLogic.InvalidateUpPartyCacheAsync(party.Name);
                    await downPartyAllowUpPartiesQueueLogic.UpdateUpParty(oldMUpParty, mParty as UpParty);
                }
                else if (mParty is DownParty)
                {
                    await downPartyCacheLogic.InvalidateDownPartyCacheAsync(party.Name);
                }
                else
                {
                    throw new NotSupportedException($"{mParty?.GetType()?.Name} type not supported.");
                }

                return Ok(ModelToApiMap(mParty));
            }
            catch (CosmosDataException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.Warning(ex, $"NotFound, Update '{typeof(AParty).Name}' by name '{party.Name}'.");
                    return NotFound(typeof(AParty).Name, party.Name, nameof(party.Name));
                }
                throw;
            }
        }

        protected async Task<IActionResult> Delete(string name)
        {
            try
            {
                if (!ModelState.TryValidateRequiredParameter(name, nameof(name))) return BadRequest(ModelState);
                name = name?.ToLower();

                if (Constants.DefaultLogin.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception($"The default login with the name '{Constants.DefaultLogin.Name}' can not be deleted.");
                }

                var isUpParty = IsUpParty();
                await tenantRepository.DeleteAsync<MParty>(await GetId(isUpParty, name));

                if (isUpParty)
                {
                    await upPartyCacheLogic.InvalidateUpPartyCacheAsync(name);
                    await downPartyAllowUpPartiesQueueLogic.DeleteUpParty(name);
                }
                else
                {
                    await downPartyCacheLogic.InvalidateDownPartyCacheAsync(name);
                }

                return NoContent();
            }
            catch (CosmosDataException ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.Warning(ex, $"NotFound, Delete '{typeof(AParty).Name}' by id '{name}'.");
                    return NotFound(typeof(AParty).Name, name);
                }
                throw;
            }
        }

        private AParty ModelToApiMap(MParty mParty)
        {
            var arParty = mapper.Map<AParty>(mParty);
            if (arParty is Api.OidcUpParty arOidcUpParty)
            {
                if (arOidcUpParty.Client?.ClientSecret != null)
                {
                    if (arOidcUpParty.Client.ClientSecret.Length > 20)
                    {
                        arOidcUpParty.Client.ClientSecret = arOidcUpParty.Client.ClientSecret.Substring(0, 3);
                    }
                }
            }
            return arParty;
        }

        private bool IsUpParty()
        {
            if (EqualsBaseType(0, typeof(MParty), (typeof(UpParty))))
            {
                return true;
            }
            else if (EqualsBaseType(0, typeof(MParty), (typeof(DownParty))))
            {
                return false;
            }
            else
            {
                throw new NotSupportedException($"{typeof(MParty)} type not supported.");
            }
        }

        private Task<string> GetId(bool isUpParty, string name)
        {
            if(isUpParty)
            {
                return UpParty.IdFormatAsync(RouteBinding, name);
            }
            else 
            {
                return DownParty.IdFormatAsync(RouteBinding, name);
            }
        }

        private bool EqualsBaseType(int recursivCount, Type type, Type baseType)
        {
            var bt = type.BaseType;
            if (bt.Equals(baseType)) return true;

            if (recursivCount > 2) return false;

            recursivCount++;
            return EqualsBaseType(recursivCount, bt, baseType);
        }

        private async Task<int> CountParties(string dataType)
        {
            return await tenantRepository.CountAsync<Party>(new Party.IdKey { TenantName = RouteBinding.TenantName, TrackName = RouteBinding.TrackName }, whereQuery: p => p.DataType.Equals(dataType));
        }

        private async Task<string> GetPartyNameAsync(string name = null, int count = 0)
        {
            if (name.IsNullOrWhiteSpace())
            {
                name = RandomGenerator.GenerateCode(Constants.ControlApi.DefaultNameLength).ToLower();
                if (count < 3)
                {
                    var mParty = await tenantRepository.GetAsync<MParty>(await GetId(IsUpParty(), name), required: false);
                    if (mParty != null)
                    {
                        count++;
                        return await GetPartyNameAsync(count: count);
                    }
                }
                return name;
            }
            else
            {
                return name.ToLower();
            }
        }

        private string GetSamlIssuer(string name)
        {
            return $"uri:{name}";
        }
    }
}
