﻿// Copyright © 2010 onwards, Andrew Whewell
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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The interface for objects that find out the external IP address of the user.
    /// </summary>
    /// <remarks>
    /// The main screen presenter normally uses this to determine the external IP address.
    /// If you are writing a plugin and would like to know the public IP address of the
    /// computer then hook <see cref="AddressUpdated"/> on the singleton in your Startup
    /// method and it will eventually be called some time after the main screen fires up.
    /// </remarks>
    public interface IExternalIPAddressService : ISingleton<IExternalIPAddressService>
    {
        /// <summary>
        /// Gets or sets the object that isolates the service from the environment.
        /// </summary>
        IExternalIPAddressServiceProvider Provider { get; set; }

        /// <summary>
        /// Gets the last address fetched by <see cref="GetExternalIPAddress"/>.
        /// </summary>
        string Address { get; }

        /// <summary>
        /// Raised when the address has been determined by a call to <see cref="GetExternalIPAddress"/>.
        /// </summary>
        event EventHandler<EventArgs<string>> AddressUpdated; 

        /// <summary>
        /// Returns the external IP address of the application.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This will block until the external IP address is determined and may throw exceptions if the IP
        /// address cannot be determined. It would be best not to call this from a GUI thread.
        /// </remarks>
        string GetExternalIPAddress();
    }
}
