﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.18449
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace VirtualRadar.Plugin.AircraftTrackLog {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class PluginStrings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal PluginStrings() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("VirtualRadar.Plugin.AircraftTrackLog.PluginStrings", typeof(PluginStrings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   使用此强类型资源类，为所有资源查找
        ///   重写当前线程的 CurrentUICulture 属性。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 Cancel 的本地化字符串。
        /// </summary>
        internal static string Cancel {
            get {
                return ResourceManager.GetString("Cancel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Disabled 的本地化字符串。
        /// </summary>
        internal static string Disabled {
            get {
                return ResourceManager.GetString("Disabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Enabled 的本地化字符串。
        /// </summary>
        internal static string Enabled {
            get {
                return ResourceManager.GetString("Enabled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Enabled, receiver not enabled or invalid 的本地化字符串。
        /// </summary>
        internal static string EnabledBadReceiver {
            get {
                return ResourceManager.GetString("EnabledBadReceiver", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Enabled, Browse /VirtualRadar/Example/Index.html 的本地化字符串。
        /// </summary>
        internal static string EnabledDescription {
            get {
                return ResourceManager.GetString("EnabledDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Enabled, receiver not enabled or invalid 的本地化字符串。
        /// </summary>
        internal static string EnabledNoReceiver {
            get {
                return ResourceManager.GetString("EnabledNoReceiver", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Exception caught: {0} 的本地化字符串。
        /// </summary>
        internal static string ExceptionCaught {
            get {
                return ResourceManager.GetString("ExceptionCaught", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Ok 的本地化字符串。
        /// </summary>
        internal static string Ok {
            get {
                return ResourceManager.GetString("Ok", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Aircraft Track Log 的本地化字符串。
        /// </summary>
        internal static string OptionsViewTitle {
            get {
                return ResourceManager.GetString("OptionsViewTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Receiver 的本地化字符串。
        /// </summary>
        internal static string Receiver {
            get {
                return ResourceManager.GetString("Receiver", resourceCulture);
            }
        }
    }
}
