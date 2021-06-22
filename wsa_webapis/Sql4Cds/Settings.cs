﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wsa_webapis.Sql4Cds
{
    /// <summary>
    /// This class can help you to store settings for your plugin
    /// </summary>
    /// <remarks>
    /// This class must be XML serializable
    /// </remarks>
    public class Settings
    {
        public static Settings Instance { get; internal set; }

        public int SelectLimit { get; set; }

        public int UpdateWarnThreshold { get; set; }

        public bool BlockUpdateWithoutWhere { get; set; } = true;

        public int DeleteWarnThreshold { get; set; }

        public bool BlockDeleteWithoutWhere { get; set; } = true;

        public bool UseBulkDelete { get; set; }

        public int BatchSize { get; set; } = 100;

        public bool ShowLocalTimes { get; set; }

        public bool QuotedIdentifiers { get; set; } = true;

        public bool UseTSQLEndpoint { get; set; }

        public bool UseRetrieveTotalRecordCount { get; set; } = true;

        public bool ShowIntellisenseTooltips { get; set; } = true;

        public int MaxDegreeOfPaallelism { get; set; } = 10;

        public bool IncludeFetchXml { get; set; }

        public bool AutoSizeColumns { get; set; } = true;

        public int MaxRetrievesPerQuery { get; set; } = 100;
    }
}