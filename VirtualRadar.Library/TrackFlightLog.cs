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
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using System.IO;
using InterfaceFactory;
using System.Threading;
using System.Windows.Forms;

namespace VirtualRadar.Library
{
    /// <summary>
    /// Default implementation of <see cref="ITrackFlightLog"/>.
    /// </summary>
    class TrackFlightLog : ITrackFlightLog
    {
        /// <summary>
        /// Default implementation of <see cref="ILogProvider"/>.
        /// </summary>
        class DefaultProvider : ILogProvider
        {
            public int CurrentThreadId                              { get { return Thread.CurrentThread.ManagedThreadId; } }
            public bool FileExists(string fullPath)                 { return File.Exists(fullPath); }
            public bool FolderExists(string folder)                 { return Directory.Exists(folder); }
            public void CreateFolder(string folder)                 { Directory.CreateDirectory(folder); }
            public void AppendAllText(string fullPath, string text) { File.AppendAllText(fullPath, text); }

            public void TruncateTo(string fullPath, int bytes)
            {
                using(var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read | FileAccess.Write)) {
                    if(stream.Length > bytes) {
                        byte[] buffer = new byte[bytes];
                        stream.Seek(-bytes, SeekOrigin.End);
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.Write(buffer, 0, bytesRead);
                        stream.SetLength(bytesRead);
                    }
                }
            }
        }

        /// <summary>
        /// The object that all threads and instances use to prevent concurrent updates.
        /// </summary>
        private static object _SyncLock = new object();

        /// <summary>
        /// The object that manages the clock for us.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// Gets the folder that <see cref="FileName"/> was based on.
        /// </summary>
        private string _Folder;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public ILogProvider Provider { get; set; }

        private static readonly ITrackFlightLog _Singleton = new TrackFlightLog();
        /// <summary>
        /// See interface docs.
        /// </summary>
        public ITrackFlightLog Singleton { get { return _Singleton; } }

        private string _FileName;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string FileName
        {
            get { Initialise(); return _FileName; }
            set { Initialise(); _FileName = Path.Combine(_Folder, value + ".log"); }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public TrackFlightLog()
        {
            Provider = new DefaultProvider();
            _Clock = Factory.Singleton.Resolve<IClock>();
        }

        /// <summary>
        /// Determines the folder and filename.
        /// </summary>
        private void Initialise()
        {
            if(_Folder == null) {
                _Folder = Path.Combine(Application.StartupPath, "TrackFlight");
                
            }

            if(_FileName == null) {
                _FileName = Path.Combine(_Folder, DateTime.Now.ToString("yyyyMMdd") + "TrackFlight.log");
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="message"></param>
        public void WriteLine(string message)
        {
            Initialise();
            //Factory.Singleton.Resolve<VirtualRadar.Interface.ILog>().Singleton.WriteLine(FileName);
            if(message != null) {
                lock(_SyncLock) {
                    if(!Provider.FolderExists(_Folder)) Provider.CreateFolder(_Folder);
                    Provider.AppendAllText(FileName, message);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public void WriteLine(string format, params object[] args)
        {
            WriteLine(String.Format(format, args));
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="kbLength"></param>
        public void Truncate(int kbLength)
        {
            if(kbLength < 0) throw new ArgumentOutOfRangeException("kbLength");
            Initialise();

            var length = kbLength * 1024;

            lock(_SyncLock) {
                if(Provider.FileExists(FileName)) Provider.TruncateTo(FileName, length);
            }
        }
    }
}
