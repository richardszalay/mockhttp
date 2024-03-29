﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RichardSzalay.MockHttp.Formatters {
    using System;
    using System.Reflection;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("RichardSzalay.MockHttp.Formatters.Resources", typeof(Resources).GetTypeInfo().Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
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
        ///   Looks up a localized string similar to matches any one of {0}.
        /// </summary>
        internal static string AnyMatcherDescriptor {
            get {
                return ResourceManager.GetString("AnyMatcherDescriptor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Backend definitions were still evaluated because the BackendDefinitionBehavior is set to Always. This can be changed by using BackendDefinitionBehavior.NoExpectations.
        /// </summary>
        internal static string BackendDefinitionFallbackHeader {
            get {
                return ResourceManager.GetString("BackendDefinitionFallbackHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The following backend definition matched and handled the result:.
        /// </summary>
        internal static string BackendDefinitionMatchSuccessHeader {
            get {
                return ResourceManager.GetString("BackendDefinitionMatchSuccessHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} backend definitions were evaluated but did not match:.
        /// </summary>
        internal static string BackendDefinitionsMatchFailedHeader {
            get {
                return ResourceManager.GetString("BackendDefinitionsMatchFailedHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to request body matches {0}.
        /// </summary>
        internal static string ContentMatcherDescriptor {
            get {
                return ResourceManager.GetString("ContentMatcherDescriptor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to request matches a custom predicate.
        /// </summary>
        internal static string CustomMatcherDescriptor {
            get {
                return ResourceManager.GetString("CustomMatcherDescriptor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to form data exacly matches (no additional keys allowed) {0}.
        /// </summary>
        internal static string FormDataMatcherDescriptor {
            get {
                return ResourceManager.GetString("FormDataMatcherDescriptor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to headers match {0}.
        /// </summary>
        internal static string HeadersMatcherDescriptor {
            get {
                return ResourceManager.GetString("HeadersMatcherDescriptor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to JSON request body matches custom {0} predicate.
        /// </summary>
        internal static string JsonContentMatcherDescriptor {
            get {
                return ResourceManager.GetString("JsonContentMatcherDescriptor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to FAILED.
        /// </summary>
        internal static string MatcherStatusFailedLabel {
            get {
                return ResourceManager.GetString("MatcherStatusFailedLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to SKIPPED.
        /// </summary>
        internal static string MatcherStatusSkippedLabel {
            get {
                return ResourceManager.GetString("MatcherStatusSkippedLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MATCHED.
        /// </summary>
        internal static string MatcherStatusSuccessLabel {
            get {
                return ResourceManager.GetString("MatcherStatusSuccessLabel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to match a mocked request for {0}.
        /// </summary>
        internal static string MatchFailureHeader {
            get {
                return ResourceManager.GetString("MatchFailureHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Matched a mocked request for {0}.
        /// </summary>
        internal static string MatchSuccessHeader {
            get {
                return ResourceManager.GetString("MatchSuccessHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to method matches {0}.
        /// </summary>
        internal static string MethodMatcherDescriptor {
            get {
                return ResourceManager.GetString("MethodMatcherDescriptor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} backend definitions were not evaluated because the BackendDefinitionBehavior is set to NoExpectations. This can be changed by using BackendDefinitionBehavior.Always.
        /// </summary>
        internal static string NoBackendDefinitionFallbackHeader {
            get {
                return ResourceManager.GetString("NoBackendDefinitionFallbackHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to request body partially matches {0}.
        /// </summary>
        internal static string PartialContentMatcherDescriptor {
            get {
                return ResourceManager.GetString("PartialContentMatcherDescriptor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to form data matches {0}.
        /// </summary>
        internal static string PartialFormDataMatcherDescriptor {
            get {
                return ResourceManager.GetString("PartialFormDataMatcherDescriptor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to query string matches {0}.
        /// </summary>
        internal static string PartialQueryStringMatcherDescriptor {
            get {
                return ResourceManager.GetString("PartialQueryStringMatcherDescriptor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to query string exacly matches (no additional keys allowed) {0}.
        /// </summary>
        internal static string QueryStringMatcherDescriptor {
            get {
                return ResourceManager.GetString("QueryStringMatcherDescriptor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The next request expectation failed to match:.
        /// </summary>
        internal static string RequestExpectationMatchFailureHeader {
            get {
                return ResourceManager.GetString("RequestExpectationMatchFailureHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The next request expectation matched and handled the result:.
        /// </summary>
        internal static string RequestExpectationMatchSuccessHeader {
            get {
                return ResourceManager.GetString("RequestExpectationMatchSuccessHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {0} additional request expectations were not evaluated because request expectations (Expect) must be matched in order. For unordered matches, use backend definitions (When)..
        /// </summary>
        internal static string SkippedRequestExpectationsHeader {
            get {
                return ResourceManager.GetString("SkippedRequestExpectationsHeader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to URL matches {0}.
        /// </summary>
        internal static string UrlMatcherDescriptor {
            get {
                return ResourceManager.GetString("UrlMatcherDescriptor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to XML request body matches custom {0} predicate.
        /// </summary>
        internal static string XmlContentMatcherDescriptor {
            get {
                return ResourceManager.GetString("XmlContentMatcherDescriptor", resourceCulture);
            }
        }
    }
}
