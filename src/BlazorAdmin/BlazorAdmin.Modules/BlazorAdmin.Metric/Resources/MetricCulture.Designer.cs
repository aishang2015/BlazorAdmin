﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace BlazorAdmin.Metric.Resources {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class MetricCulture {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal MetricCulture() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("BlazorAdmin.Metric.Resources.MetricCulture", typeof(MetricCulture).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性，对
        ///   使用此强类型资源类的所有资源查找执行重写。
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
        ///   查找类似 活动的数据库上下文数量 的本地化字符串。
        /// </summary>
        internal static string ActiveDbContexts {
            get {
                return ResourceManager.GetString("ActiveDbContexts", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 运行的定时器 的本地化字符串。
        /// </summary>
        internal static string ActiveTimerCount {
            get {
                return ResourceManager.GetString("ActiveTimerCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 应用指标 的本地化字符串。
        /// </summary>
        internal static string AppMetric {
            get {
                return ResourceManager.GetString("AppMetric", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 已加载程序集个数 的本地化字符串。
        /// </summary>
        internal static string AssemblyCount {
            get {
                return ResourceManager.GetString("AssemblyCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 接收总字节数 的本地化字符串。
        /// </summary>
        internal static string BytesReceived {
            get {
                return ResourceManager.GetString("BytesReceived", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 发送总字节数 的本地化字符串。
        /// </summary>
        internal static string BytesSent {
            get {
                return ResourceManager.GetString("BytesSent", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 应用Cpu使用率 的本地化字符串。
        /// </summary>
        internal static string CpuUsage {
            get {
                return ResourceManager.GetString("CpuUsage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 执行中的请求数 的本地化字符串。
        /// </summary>
        internal static string CurrentRequests {
            get {
                return ResourceManager.GetString("CurrentRequests", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 堆内存 的本地化字符串。
        /// </summary>
        internal static string GcHeapSize {
            get {
                return ResourceManager.GetString("GcHeapSize", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 每秒数据库查询次数 的本地化字符串。
        /// </summary>
        internal static string QueryPerSecond {
            get {
                return ResourceManager.GetString("QueryPerSecond", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 全部查询次数 的本地化字符串。
        /// </summary>
        internal static string TotalQueries {
            get {
                return ResourceManager.GetString("TotalQueries", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 全部请求数 的本地化字符串。
        /// </summary>
        internal static string TotalRequests {
            get {
                return ResourceManager.GetString("TotalRequests", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 全部保存次数 的本地化字符串。
        /// </summary>
        internal static string TotalSaveChanges {
            get {
                return ResourceManager.GetString("TotalSaveChanges", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 工作集内存 的本地化字符串。
        /// </summary>
        internal static string WorkingSet {
            get {
                return ResourceManager.GetString("WorkingSet", resourceCulture);
            }
        }
    }
}
