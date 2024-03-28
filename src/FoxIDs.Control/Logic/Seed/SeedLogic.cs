using FoxIDs.Infrastructure;
using FoxIDs.Models.Config;
using FoxIDs.Repository;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Cosmos;

namespace FoxIDs.Logic.Seed
{
    public class SeedLogic : LogicBase
    {
        private readonly TelemetryLogger logger;
        private readonly FoxIDsControlSettings settings;
        private readonly MasterTenantDocumentsSeedLogic masterTenantDocumentsSeedLogic;

        public SeedLogic(TelemetryLogger logger, FoxIDsControlSettings settings, MasterTenantDocumentsSeedLogic masterTenantDocumentsSeedLogic, IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            this.logger = logger;
            this.settings = settings;
            this.masterTenantDocumentsSeedLogic = masterTenantDocumentsSeedLogic;
        }

        public async Task SeedAsync()
        {
            try
            {
                if (settings.MasterSeedEnabled)
                {
                    await masterTenantDocumentsSeedLogic.SeedAsync();
                }
            }
            catch (Exception ex)
            {
                logger.CriticalError(ex, "Error seeding master documents.");
                throw;
            }
        }
    }
}
