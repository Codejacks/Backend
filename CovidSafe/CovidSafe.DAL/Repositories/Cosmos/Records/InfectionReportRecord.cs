using System;
using System.ComponentModel.DataAnnotations;

using CovidSafe.DAL.Helpers;
using CovidSafe.Entities.Geospatial;
using CovidSafe.Entities.Reports;
using Newtonsoft.Json;

namespace CovidSafe.DAL.Repositories.Cosmos.Records
{
    /// <summary>
    /// <see cref="InfectionReport"/> implementation of <see cref="CosmosRecord{T}"/>
    /// </summary>
    public class InfectionReportRecord : CosmosRecord<InfectionReport>
    {
        /// <summary>
        /// Boundary allowed by <see cref="InfectionReport"/> region
        /// </summary>
        [JsonProperty("RegionBoundary", Required = Required.Always)]
        [Required]
        public RegionBoundary RegionBoundary { get; set; }
        /// <summary>
        /// Size of the record <see cref="InfectionReport"/>, in bytes
        /// </summary>
        [JsonProperty("size", Required = Required.Always)]
        [Required]
        public long Size { get; set; }
        /// <summary>
        /// Current version of record schema
        /// </summary>
        [JsonIgnore]
        public const string CURRENT_RECORD_VERSION = "2.1.0";

        /// <summary>
        /// Creates a new <see cref="InfectionReportRecord"/> instance
        /// </summary>
        public InfectionReportRecord()
        {
        }

        /// <summary>
        /// Creates a new <see cref="InfectionReportRecord"/> instance
        /// </summary>
        /// <param name="report"><see cref="InfectionReport"/> to store</param>
        public InfectionReportRecord(InfectionReport report) : base()
        {
            this.Size = PayloadSizeHelper.GetSize(report);
            this.Value = report;
            this.Version = CURRENT_RECORD_VERSION;

            // Create partition key, which is the last update timestamp round down to the nearest day
            DateTimeOffset timestamp = DateTimeOffset.FromUnixTimeMilliseconds(this.Timestamp);
            // Convert to Unix time (ms) in partition
            this.PartitionKey = timestamp.Date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds.ToString();
        }
    }
}