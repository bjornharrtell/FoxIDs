﻿using FoxIDs.Client.Shared.Components;
using FoxIDs.Models.Api;

namespace FoxIDs.Client.Models.ViewModels
{
    public class GeneralUsedViewModel : UsedViewModel
    {
        public GeneralUsedViewModel()
        { }

        public GeneralUsedViewModel(UsedBase used)
        {
            TenantName = used.TenantName;
            PeriodBeginDate = used.PeriodBeginDate;
            PeriodEndDate = used.PeriodEndDate;
            IsUsageCalculated = used.IsUsageCalculated;
            IsInvoiceReady = used.IsInvoiceReady;
            PaymentStatus = used.PaymentStatus;
            IsInactive = used.IsInactive;
            IsDone = used.IsDone;
            HasItems = used.HasItems;
            Invoices = used.Invoices;
        }

        public bool Edit { get; set; }

        public bool ShowAdvanced { get; set; }

        public bool CreateMode { get; set; }

        public bool DeleteAcknowledge { get; set; }

        public bool InvoicingActionButtonDisabled { get; set; }

        public string Error { get; set; }

        public decimal HourPrice { get; set; }

        public PageEditForm<UsedViewModel> Form { get; set; }

        // From tenant
        public bool EnableUsage { get; set; }
    }
}
