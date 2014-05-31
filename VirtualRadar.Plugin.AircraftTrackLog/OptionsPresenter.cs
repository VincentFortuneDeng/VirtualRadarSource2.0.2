// Copyright © 2013 onwards, Andrew Whewell
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.IO;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.View;

namespace VirtualRadar.Plugin.AircraftTrackLog
{
    class OptionsPresenter : IOptionsPresenter
    {
        #region Fields
        IOptionsView _View;
        private bool _SuppressValueChangedEventHandler;
        /// <summary>
        /// The object that different threads synchronise on before using the contents of the fields.
        /// </summary>
        //private object _SyncLock = new object();

        #endregion

        #region Initialise
        public void Initialise(IOptionsView view)
        {
            _View = view;

            _View.SaveClicked += View_SaveClicked;
            /*_View.ResetClicked += View_ResetClicked;
            _View.SelectedInjectSettingsChanged += View_SelectedInjectSettingsChanged;*/
            _View.ValueChanged += View_ValueChanged;
            /*_View.NewInjectSettingsClicked += View_NewInjectSettingsClicked;
            _View.DeleteInjectSettingsClicked += View_DeleteInjectSettingsClicked;

            if(_View.InjectSettings.Count > 0) _View.SelectedInjectSettings = _View.InjectSettings[0];*/
        }
        #endregion



        #region
        private bool DoValidation()
        {
            var results = new List<ValidationResult>();

            if(_View.PluginEnabled) {
                //lock(_SyncLock) {
                var feedManager = Factory.Singleton.Resolve<IFeedManager>().Singleton;

                if(_View.ReceiverId == 0) {
                    results.Add(new ValidationResult(ValidationField.ReceiverIds, PluginStrings.EnabledNoReceiver));
                } else if(feedManager.GetByUniqueId(_View.ReceiverId) == null) {
                    results.Add(new ValidationResult(ValidationField.ReceiverIds, PluginStrings.EnabledBadReceiver));
                }

                //OnStatusChanged(EventArgs.Empty);

                //}
            }

            _View.ShowValidationResults(results);

            return results.Count == 0;
        }

        #endregion

        #region Events subscribed

        private void View_SaveClicked(object sender, CancelEventArgs args)
        {
            var view = (IOptionsView)sender;

            if(!DoValidation())
                args.Cancel = true;
        }

        private void View_ValueChanged(object sender, EventArgs args)
        {
            if(!_SuppressValueChangedEventHandler) {
                DoValidation();
            }
        }
    }
        #endregion
}

