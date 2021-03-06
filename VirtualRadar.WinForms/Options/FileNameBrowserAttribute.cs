﻿// Copyright © 2012 onwards, Andrew Whewell
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

namespace VirtualRadar.WinForms.Options
{
    /// <summary>
    /// An attribute that can be applied to filename properties to supply defaults to the browser.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple=false)]
    class FileNameBrowserAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets a value indicating that the default extension should be added if no extension is entered.
        /// </summary>
        public bool AddExtension { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that a warning should be displayed if the filename does not exist.
        /// </summary>
        public bool CheckFileExists { get; set; }

        /// <summary>
        /// Gets or sets the default extension.
        /// </summary>
        public string DefaultExtension { get; set; }

        /// <summary>
        /// Gets or sets the extension filter.
        /// </summary>
        public string Filter { get; set; }

        /// <summary>
        /// Gets or sets the localised string key for the browser dialog title.
        /// </summary>
        public string BrowserTitle { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FileNameBrowserAttribute() : base()
        {
            AddExtension = true;
            CheckFileExists = true;
            Filter = "All files (*.*)|*.*";
        }
    }
}
