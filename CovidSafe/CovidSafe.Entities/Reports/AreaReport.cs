using System;
using System.Collections.Generic;

using CovidSafe.Entities.Geospatial;
using CovidSafe.Entities.Validation;
using CovidSafe.Entities.Validation.Resources;
using Newtonsoft.Json;

namespace CovidSafe.Entities.Reports
{
    /// <summary>
    /// Area-based infection report
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [Serializable]
    public class AreaReport : IValidatable
    {
        /// <summary>
        /// Infection risk <see cref="InfectionArea"/> part of this <see cref="AreaReport"/>
        /// </summary>
        [JsonProperty("Areas", Required = Required.Always)]
        public IList<InfectionArea> Areas { get; set; } = new List<InfectionArea>();
        /// <summary>
        /// Time report alerting begins
        /// </summary>
        [JsonProperty("beginTimestamp", NullValueHandling = NullValueHandling.Ignore)]
        public long BeginTimestamp { get; set; }
        /// <summary>
        /// Time report alerting ends
        /// </summary>
        [JsonProperty("endTimestamp", NullValueHandling = NullValueHandling.Ignore)]
        public long EndTimestamp { get; set; }
        /// <summary>
        /// Internal UserMessage backing field
        /// </summary>
        [NonSerialized]
        private string _userMessage;
        /// <summary>
        /// Message displayed to user on positive match
        /// </summary>
        [JsonProperty("userMessage", NullValueHandling = NullValueHandling.Ignore)]
        public string UserMessage
        {
            get { return this._userMessage; }
            set { _userMessage = value; }
        }

        /// <inheritdoc/>
        public RequestValidationResult Validate()
        {
            RequestValidationResult result = new RequestValidationResult();

            // Validate areas
            if (this.Areas.Count > 0)
            {
                // Validate individual areas
                foreach (InfectionArea area in this.Areas)
                {
                    // Use Area.Validate()
                    result.Combine(area.Validate());
                }
            }
            else
            {
                result.Fail(
                    RequestValidationIssue.InputEmpty,
                    nameof(this.Areas),
                    ValidationMessages.EmptyAreas
                );
            }

            // Validate message
            if (String.IsNullOrEmpty(this.UserMessage))
            {
                result.Fail(
                    RequestValidationIssue.InputEmpty,
                    nameof(this.UserMessage),
                    ValidationMessages.EmptyMessage
                );
            }

            // Validate timestamps if specified
            if (this.BeginTimestamp > 0 && this.EndTimestamp > 0)
            {
                result.Combine(Validator.ValidateTimestamp(this.BeginTimestamp, parameterName: nameof(this.BeginTimestamp)));
                result.Combine(Validator.ValidateTimestamp(this.EndTimestamp, parameterName: nameof(this.EndTimestamp)));
                result.Combine(Validator.ValidateTimeRange(this.BeginTimestamp, this.EndTimestamp));
            }

            return result;
        }
    }
}
