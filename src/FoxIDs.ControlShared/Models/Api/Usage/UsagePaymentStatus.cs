﻿namespace FoxIDs.Models.Api
{
    public enum UsagePaymentStatus
    {
        None = 0,
        Open = 100,
        Pending = 120,
        Authorized = 140,
        Paid = 200,

        Canceled = 320,
        Expired = 340,
        Failed = 360,
    }
}
