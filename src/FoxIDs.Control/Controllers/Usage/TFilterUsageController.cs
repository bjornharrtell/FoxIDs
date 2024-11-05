﻿using AutoMapper;
using FoxIDs.Infrastructure;
using FoxIDs.Repository;
using Api = FoxIDs.Models.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ITfoxtec.Identity;
using FoxIDs.Infrastructure.Security;
using FoxIDs.Infrastructure.Filters;
using System;
using FoxIDs.Models;
using FoxIDs.Logic.Usage;

namespace FoxIDs.Controllers
{
    [RequireMasterTenant]
    [MasterScopeAuthorize]
    public class TFilterUsageController : ApiController
    {
        private readonly TelemetryScopedLogger logger;
        private readonly IMapper mapper;
        private readonly ITenantDataRepository tenantDataRepository;
        private readonly UsageMolliePaymentLogic usageMolliePaymentLogic;

        public TFilterUsageController(TelemetryScopedLogger logger, IMapper mapper, ITenantDataRepository tenantDataRepository, UsageMolliePaymentLogic usageMolliePaymentLogic) : base(logger)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.tenantDataRepository = tenantDataRepository;
            this.usageMolliePaymentLogic = usageMolliePaymentLogic;
        }

        /// <summary>
        /// Filter Usage.
        /// </summary>
        /// <param name="filterTenantName">Filter by tenant name.</param>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <returns>Tenants.</returns>
        [ProducesResponseType(typeof(HashSet<Api.Tenant>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<HashSet<Api.UsedBase>>> GetFilterUsage(string filterTenantName, int year, int month)
        {
            try
            {
                if (year <= 0 || month <= 0)
                {
                    var now = DateTimeOffset.Now;
                    year = now.Year;
                    month = now.Month;
                }

                (var mUsedList, _) = filterTenantName.IsNullOrWhiteSpace() ? 
                    await tenantDataRepository.GetListAsync<Used>(whereQuery: u => u.PeriodEndDate.Month == month && u.PeriodEndDate.Year == year) :
                    await tenantDataRepository.GetListAsync<Used>(whereQuery: u => u.PeriodEndDate.Month == month && u.PeriodEndDate.Year == year && u.TenantName.Contains(filterTenantName, StringComparison.CurrentCultureIgnoreCase));

                var aUsedList = new HashSet<Api.UsedBase>(mUsedList.Count());
                foreach (var mUsed in mUsedList.OrderBy(t => t.TenantName))
                {
                    if(mUsed.PaymentStatus == UsagePaymentStatus.Open || mUsed.PaymentStatus == UsagePaymentStatus.Pending || mUsed.PaymentStatus == UsagePaymentStatus.Authorized)
                    {
                        await usageMolliePaymentLogic.UpdatePaymentAsync(mUsed);
                    }
                    var aUsed = mapper.Map<Api.UsedBase>(mUsed);
                    var mLastInvoice = mUsed.Invoices?.LastOrDefault();
                    if(mLastInvoice != null)
                    {
                        var aLastInvoice = mapper.Map<Api.Invoice>(mLastInvoice);
                        aLastInvoice.Lines = null;
                        aLastInvoice.TimeItems = null;
                        aUsed.Invoices = [aLastInvoice];

                    }
                    aUsedList.Add(aUsed);
                }
                return Ok(aUsedList);
            }
            catch (FoxIDsDataException ex)
            {
                if (ex.StatusCode == DataStatusCode.NotFound)
                {
                    logger.Warning(ex, $"NotFound, Get '{typeof(Api.Used).Name}' by filter tenant name '{filterTenantName}'.");
                    return NotFound(typeof(Api.Used).Name, filterTenantName);
                }
                throw;
            }
        }
    }
}
