﻿using AutoMapper;
using FoxIDs.Infrastructure;
using FoxIDs.Repository;
using FoxIDs.Models;
using Api = FoxIDs.Models.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FoxIDs.Logic;
using System.Security.Claims;
using System.Collections.Generic;
using ITfoxtec.Identity;
using System;
using System.Linq.Expressions;
using FoxIDs.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;
using FoxIDs.Models.Logic;

namespace FoxIDs.Controllers
{
    [TenantScopeAuthorize(Constants.ControlApi.Segment.User)]
    public class TUserController : ApiController
    {
        private readonly TelemetryScopedLogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly IMapper mapper;
        private readonly ITenantDataRepository tenantDataRepository;
        private readonly PlanCacheLogic planCacheLogic;
        private readonly BaseAccountLogic accountLogic;

        public TUserController(TelemetryScopedLogger logger, IServiceProvider serviceProvider, IMapper mapper, ITenantDataRepository tenantDataRepository, PlanCacheLogic planCacheLogic, BaseAccountLogic accountLogic) : base(logger)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.mapper = mapper;
            this.tenantDataRepository = tenantDataRepository;
            this.planCacheLogic = planCacheLogic;
            this.accountLogic = accountLogic;
        }

        /// <summary>
        /// Get user.
        /// </summary>
        /// <param name="email">Users email.</param>
        /// <param name="phone">Users phone.</param>
        /// <param name="username">Users username.</param>
        /// <returns>User.</returns>
        [ProducesResponseType(typeof(Api.User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Api.User>> GetUser(string email = null, string phone = null, string username = null)
        {
            try
            {
                if (email.IsNullOrWhiteSpace() && phone.IsNullOrWhiteSpace() && username.IsNullOrWhiteSpace())
                {
                    ModelState.TryAddModelError(string.Empty, $"The {nameof(email)} or {nameof(phone)} or {nameof(username)} parameter is required.");
                    return BadRequest(ModelState);
                }
                email = email?.Trim().ToLower();
                phone = phone?.Trim();
                username = username?.Trim()?.ToLower();

                var mUser = await tenantDataRepository.GetAsync<User>(await Models.User.IdFormatAsync(RouteBinding, new User.IdKey { Email = email, UserIdentifier = phone ?? username }));
                return Ok(mapper.Map<Api.User>(mUser));
            }
            catch (FoxIDsDataException ex)
            {
                if (ex.StatusCode == DataStatusCode.NotFound)
                {
                    logger.Warning(ex, $"NotFound, Get '{typeof(Api.User).Name}' by email '{email}', phone '{phone}', username '{username}'.");
                    return NotFound(typeof(Api.User).Name, email ?? phone ?? username);
                }
                throw;
            }
        }

        /// <summary>
        /// Create user.
        /// </summary>
        /// <param name="createUserRequest">User.</param>
        /// <returns>User.</returns>
        [ProducesResponseType(typeof(Api.User), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Api.User>> PostUser([FromBody] Api.CreateUserRequest createUserRequest)
        {
            try
            {
                if (!await ModelState.TryValidateObjectAsync(createUserRequest)) return BadRequest(ModelState);
                createUserRequest.Email = createUserRequest.Email?.Trim().ToLower();
                createUserRequest.Phone = createUserRequest.Phone?.Trim();
                createUserRequest.Username = createUserRequest.Username?.Trim()?.ToLower();

                if (!RouteBinding.PlanName.IsNullOrEmpty())
                {
                    var plan = await planCacheLogic.GetPlanAsync(RouteBinding.PlanName);
                    if (plan.Users.LimitedThreshold > 0)
                    {
                        Expression<Func<User, bool>> whereQuery = p => p.DataType.Equals("user") && p.PartitionId.StartsWith($"{RouteBinding.TenantName}:");
                        var count = await tenantDataRepository.CountAsync(whereQuery: whereQuery, usePartitionId: false);
                        // included + one master user
                        if (count > plan.Users.LimitedThreshold)
                        {
                            throw new Exception($"Maximum number of users ({plan.Users.LimitedThreshold}) in the '{plan.Name}' plan has been reached.");
                        }
                    }
                }

                var claims = new List<Claim>();
                if (createUserRequest.Claims?.Count > 0)
                {
                    foreach (var claimAndValue in createUserRequest.Claims)
                    {
                        foreach(var value in claimAndValue.Values)
                        {
                            claims.Add(new Claim(claimAndValue.Claim, value));
                        }
                    }
                }
                
                var mUser = await accountLogic.CreateUser(new UserIdentifier { Email = createUserRequest.Email, Phone = createUserRequest.Phone, Username = createUserRequest.Username }, 
                    createUserRequest.Password, changePassword: createUserRequest.ChangePassword, claims: claims, 
                    confirmAccount: createUserRequest.ConfirmAccount, emailVerified: createUserRequest.EmailVerified, phoneVerified: createUserRequest.PhoneVerified, 
                    disableAccount: createUserRequest.DisableAccount, requireMultiFactor: createUserRequest.RequireMultiFactor);
                return Created(mapper.Map<Api.User>(mUser));
            }
            catch(UserExistsException ueex)
            {
                logger.Warning(ueex, $"Conflict, Create '{typeof(Api.User).Name}' by email '{createUserRequest.Email}', phone '{createUserRequest.Phone}', username '{createUserRequest.Username}'.");
                return Conflict(ueex.Message);
            }
            catch (AccountException aex)
            {
                ModelState.TryAddModelError(nameof(createUserRequest.Password), aex.Message);
                return BadRequest(ModelState, aex);
            }            
            catch (FoxIDsDataException ex)
            {
                if (ex.StatusCode == DataStatusCode.Conflict)
                {
                    logger.Warning(ex, $"Conflict, Create '{typeof(Api.User).Name}' by email '{createUserRequest.Email}', phone '{createUserRequest.Phone}', username '{createUserRequest.Username}'.");
                    return Conflict(typeof(Api.User).Name, createUserRequest.Email ?? createUserRequest.Phone ?? createUserRequest.Username);
                }
                throw;
            }
        }

        /// <summary>
        /// Update user.
        /// </summary>
        /// <param name="user">User.</param>
        /// <returns>User.</returns>
        [ProducesResponseType(typeof(Api.User), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Api.User>> PutUser([FromBody] Api.UserRequest user)
        {
            try
            {
                if (!await ModelState.TryValidateObjectAsync(user)) return BadRequest(ModelState);
                user.Email = user.Email?.Trim().ToLower();
                user.Phone = user.Phone?.Trim();
                user.Username = user.Username?.Trim()?.ToLower();

                var mUser = await tenantDataRepository.GetAsync<User>(await Models.User.IdFormatAsync(RouteBinding, new User.IdKey { Email = user.Email, UserIdentifier = user.Phone ?? user.Username }));

                if (user.UpdateEmail != null)
                {
                    if (user.UpdateEmail.IsNullOrWhiteSpace())
                    {
                        mUser.Email = null;
                    }
                    else
                    {
                        mUser.Email = user.UpdateEmail;
                    }
                }
                if (user.UpdatePhone != null)
                {
                    if (user.UpdatePhone.IsNullOrWhiteSpace())
                    {
                        mUser.Phone = null;
                    }
                    else
                    {
                        mUser.Phone = user.UpdatePhone;
                    }
                }
                if (user.UpdateUsername != null)
                {
                    if (user.UpdateUsername.IsNullOrWhiteSpace())
                    {
                        mUser.Username = null;
                    }
                    else
                    {
                        mUser.Username = user.UpdateUsername;
                    }
                }

                mUser.ConfirmAccount = user.ConfirmAccount;
                mUser.EmailVerified = mUser.Email.IsNullOrWhiteSpace() ? false : user.EmailVerified;
                mUser.PhoneVerified = mUser.Phone.IsNullOrWhiteSpace() ? false : user.PhoneVerified;
                mUser.ChangePassword = user.ChangePassword;
                mUser.DisableAccount = user.DisableAccount;
                if (!user.ActiveTwoFactorApp)
                {
                    if (!mUser.TwoFactorAppSecretExternalName.IsNullOrEmpty())
                    {
                        try
                        {
                            await serviceProvider.GetService<ExternalSecretLogic>().DeleteExternalSecretAsync(mUser.TwoFactorAppSecretExternalName);
                        }
                        catch (Exception ex)
                        {
                            logger.Warning(ex, $"Unable to delete external secret, secretExternalName '{mUser.TwoFactorAppSecretExternalName}'.");
                        }
                    }

                    mUser.TwoFactorAppSecret = null;
                    mUser.TwoFactorAppSecretExternalName = null;
                    mUser.TwoFactorAppRecoveryCode = null;
                }
                mUser.RequireMultiFactor = user.RequireMultiFactor;
                var mClaims = mapper.Map<List<ClaimAndValues>>(user.Claims);
                mUser.Claims = mClaims;
                await tenantDataRepository.UpdateAsync(mUser);

                return Ok(mapper.Map<Api.User>(mUser));
            }
            catch (FoxIDsDataException ex)
            {
                if (ex.StatusCode == DataStatusCode.NotFound)
                {
                    logger.Warning(ex, $"NotFound, Update '{typeof(Api.UserRequest).Name}' by email '{user.Email}', phone '{user.Phone}', username '{user.Username}'.");
                    return NotFound(typeof(Api.UserRequest).Name, user.Email ?? user.Phone ?? user.Username);
                }
                throw;
            }
        }

        /// <summary>
        /// Delete user.
        /// </summary>
        /// <param name="email">User email.</param>
        /// <param name="phone">User phone.</param>
        /// <param name="username">User username.</param>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(string email, string phone, string username)
        {
            try
            {
                if (email.IsNullOrWhiteSpace() && phone.IsNullOrWhiteSpace() && username.IsNullOrWhiteSpace())
                {
                    ModelState.TryAddModelError(string.Empty, $"The {nameof(email)} or {nameof(phone)} or {nameof(username)} parameter is required.");
                    return BadRequest(ModelState);
                }
                email = email?.Trim().ToLower();
                phone = phone?.Trim();
                username = username?.Trim()?.ToLower();

                await tenantDataRepository.DeleteAsync<User>(await Models.User.IdFormatAsync(RouteBinding, new User.IdKey { Email = email, UserIdentifier = phone ?? username }));
                return NoContent();
            }
            catch (FoxIDsDataException ex)
            {
                if (ex.StatusCode == DataStatusCode.NotFound)
                {
                    logger.Warning(ex, $"NotFound, Delete '{typeof(Api.User).Name}' by email '{email}', phone '{phone}', username '{username}'.");
                    return NotFound(typeof(Api.User).Name, email ?? phone ?? username);
                }
                throw;
            }
        }
    }
}
