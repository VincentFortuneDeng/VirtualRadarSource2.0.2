// Copyright © 2014 Honglin(宏林), Vincent Deng(邓守海)
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The top-level JSON object for reports on the flights taken by many aircraft.
    /// </summary>
    [DataContract]
    public class FlightTrailReportJson
    {
        /// <summary>
        /// Gets or sets the total number of rows that match the report criteria.
        /// </summary>
        [DataMember(Name = "countRows")]
        public int? CountRows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating how long the report took to process.
        /// </summary>
        [DataMember(Name = "processingTime")]
        public string ProcessingTime { get; set; }

        /// <summary>
        /// Gets or sets the content of any errors or exceptions that were thrown during the processing of the report.
        /// </summary>
        [DataMember(Name = "errorText", EmitDefaultValue = false)]
        public string ErrorText { get; set; }
        /// <summary>
        /// Gets or sets the first date that the report covers.
        /// </summary>
        [DataMember(Name = "startTime")]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// Gets or sets the flightID of the report.
        /// </summary>
        [DataMember(Name = "icao24")]
        public string ICAO24 { get; set; }
        /// <summary>
        /// Gets the list of flights that match the report criteria.
        /// </summary>
        [DataMember(Name="flightTrails", IsRequired=true)]
        public List<ReportFlightTrailJson> Flights { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FlightTrailReportJson()
        {
            Flights = new List<ReportFlightTrailJson>();
        }
    }
}
