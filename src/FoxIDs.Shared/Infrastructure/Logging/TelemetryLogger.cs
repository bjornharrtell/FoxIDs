﻿using FoxIDs.Infrastructure.Logging;
using FoxIDs.Models.Config;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace FoxIDs.Infrastructure
{
    public class TelemetryLogger
    {
        private readonly Settings settings;
        private readonly IServiceProvider serviceProvider;

        public TelemetryClient ApplicationInsightsTelemetryClient { private get; set; }

        public TelemetryLogger(Settings settings, IServiceProvider serviceProvider)
        {
            this.settings = settings;
            this.serviceProvider = serviceProvider;
        }

        public void Warning(Exception exception, IDictionary<string, string> properties = null)
        {
            Warning(exception, null, properties);
        }
        public void Warning(Exception exception, string message, IDictionary<string, string> properties = null)
        {
            if (settings.Options.Log == LogOptions.Stdout || settings.Options.Log == LogOptions.OpenSearchAndStdoutErrors || IsDevelopment())
            {
                GetStdoutTelemetryLogger().Warning(exception, message, properties);
            }

            if (settings.Options.Log == LogOptions.OpenSearchAndStdoutErrors)
            {

            }
            else if (settings.Options.Log == LogOptions.ApplicationInsights)
            {
                GetApplicationInsightsTelemetryLogger().Warning(exception, message, properties);
            }
        }

        public void Error(Exception exception, IDictionary<string, string> properties = null)
        {
            Error(exception, null, properties);
        }
        public void Error(Exception exception, string message, IDictionary<string, string> properties = null)
        {
            if (settings.Options.Log == LogOptions.Stdout || settings.Options.Log == LogOptions.OpenSearchAndStdoutErrors || IsDevelopment())
            {
                GetStdoutTelemetryLogger().Error(exception, message, properties);
            }

            if (settings.Options.Log == LogOptions.OpenSearchAndStdoutErrors)
            {

            }
            else if (settings.Options.Log == LogOptions.ApplicationInsights)
            {
                GetApplicationInsightsTelemetryLogger().Error(exception, message, properties);
            }
        }

        public void CriticalError(Exception exception, IDictionary<string, string> properties = null)
        {
            CriticalError(exception, null, properties);
        }
        public void CriticalError(Exception exception, string message, IDictionary<string, string> properties = null)
        {
            if (settings.Options.Log == LogOptions.Stdout || settings.Options.Log == LogOptions.OpenSearchAndStdoutErrors || IsDevelopment())
            {
                GetStdoutTelemetryLogger().CriticalError(exception, message, properties);
            }

            if (settings.Options.Log == LogOptions.OpenSearchAndStdoutErrors)
            {

            }
            else if (settings.Options.Log == LogOptions.ApplicationInsights)
            {
                GetApplicationInsightsTelemetryLogger().CriticalError(exception, message, properties);
            }
        }

        public void Event(string eventName, IDictionary<string, string> properties = null)
        {
            if (settings.Options.Log == LogOptions.Stdout || IsDevelopment())
            {
                GetStdoutTelemetryLogger().Event(eventName, properties);
            }

            if (settings.Options.Log == LogOptions.OpenSearchAndStdoutErrors)
            {

            }
            else if (settings.Options.Log == LogOptions.ApplicationInsights)
            {
                GetApplicationInsightsTelemetryLogger().Event(eventName, properties);
            }
        }

        public void Trace(string message, IDictionary<string, string> properties = null)
        {
            if (settings.Options.Log == LogOptions.Stdout || IsDevelopment())
            {
                GetStdoutTelemetryLogger().Trace(message, properties);
            }

            if (settings.Options.Log == LogOptions.OpenSearchAndStdoutErrors)
            {

            }
            else if (settings.Options.Log == LogOptions.ApplicationInsights)
            {
                GetApplicationInsightsTelemetryLogger().Trace(message, properties);
            }
        }

        public void Metric(string metricName, double value, IDictionary<string, string> properties = null)
        {
            if (settings.Options.Log == LogOptions.Stdout || IsDevelopment())
            {
                GetStdoutTelemetryLogger().Metric(metricName, value, properties);
            }

            if (settings.Options.Log == LogOptions.OpenSearchAndStdoutErrors)
            {

            }
            else if (settings.Options.Log == LogOptions.ApplicationInsights)
            {
                GetApplicationInsightsTelemetryLogger().Metric(metricName, value, properties);
            }
        }

        private bool IsDevelopment()
        {
            var environment = serviceProvider.GetService<IWebHostEnvironment>();
            if (environment != null)
            {
                return environment.IsDevelopment();
            }
            else
            {
                return false;
            }
        }

        private StdoutTelemetryLogger GetStdoutTelemetryLogger()
        {
            return serviceProvider.GetService<StdoutTelemetryLogger>();
        }

        private TelemetryClient GetApplicationInsightsTelemetryClient()
        {
            if(ApplicationInsightsTelemetryClient != null)
            {
                return ApplicationInsightsTelemetryClient;
            }
            else
            {
                return serviceProvider.GetService<TelemetryClient>();
            }
        }

        private ApplicationInsightsTelemetryLogger GetApplicationInsightsTelemetryLogger()
        {
            return new ApplicationInsightsTelemetryLogger(GetApplicationInsightsTelemetryClient());
        }
    }
}
