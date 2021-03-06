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
using VirtualRadar.Interface;
using System.Speech.Synthesis;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The .NET default implementation of <see cref="ISpeechSynthesizerWrapper"/>.
    /// </summary>
    sealed class DotNetSpeechSynthesizerWrapper : ISpeechSynthesizerWrapper
    {
        /// <summary>
        /// The speech synthesizer that this class wraps.
        /// </summary>
        private SpeechSynthesizer _SpeechSynthesizer = new SpeechSynthesizer();

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string DefaultVoiceName
        {
            get { return _SpeechSynthesizer.Voice.Name; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Rate
        {
            get { return _SpeechSynthesizer.Rate; }
            set { _SpeechSynthesizer.Rate = value; }
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~DotNetSpeechSynthesizerWrapper()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object. Note that that the class is sealed.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(_SpeechSynthesizer != null) _SpeechSynthesizer.Dispose();
                _SpeechSynthesizer = null;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetInstalledVoiceNames()
        {
            return _SpeechSynthesizer.GetInstalledVoices().Where(s => s.Enabled).Select(v => v.VoiceInfo.Name);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        public void SelectVoice(string name)
        {
            _SpeechSynthesizer.SelectVoice(name);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void SetOutputToDefaultAudioDevice()
        {
            _SpeechSynthesizer.SetOutputToDefaultAudioDevice();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="text"></param>
        public void SpeakAsync(string text)
        {
            _SpeechSynthesizer.SpeakAsync(text);
        }
    }
}
