﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MailCheck.Dmarc.Evaluator.Rules {
    using System;
    using System.Reflection;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class DmarcRulesMarkDownResource {
        
        private static System.Resources.ResourceManager resourceMan;
        
        private static System.Globalization.CultureInfo resourceCulture;
        
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal DmarcRulesMarkDownResource() {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager {
            get {
                if (object.Equals(null, resourceMan)) {
                    System.Resources.ResourceManager temp = new System.Resources.ResourceManager("MailCheck.Dmarc.Evaluator.Rules.DmarcRulesMarkDownResource", typeof(DmarcRulesMarkDownResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static string InvestigateRejectedEmailsOnDomainsWithRejectPolicy {
            get {
                return ResourceManager.GetString("InvestigateRejectedEmailsOnDomainsWithRejectPolicy", resourceCulture);
            }
        }
        
        internal static string PctValueShouldBe100ErrorMessage {
            get {
                return ResourceManager.GetString("PctValueShouldBe100ErrorMessage", resourceCulture);
            }
        }
        
        internal static string PercentageOfRejectedEmailsOnDate {
            get {
                return ResourceManager.GetString("PercentageOfRejectedEmailsOnDate", resourceCulture);
            }
        }
        
        internal static string PolicyShouldBeQuarantineOrRejectErrorMessage {
            get {
                return ResourceManager.GetString("PolicyShouldBeQuarantineOrRejectErrorMessage", resourceCulture);
            }
        }
        
        internal static string RuaTagShouldNotContainDuplicateUrisErrorMessage {
            get {
                return ResourceManager.GetString("RuaTagShouldNotContainDuplicateUrisErrorMessage", resourceCulture);
            }
        }
        
        internal static string RuaTagShouldNotHaveMisconfiguredMailCheckMailboxErrorMessage {
            get {
                return ResourceManager.GetString("RuaTagShouldNotHaveMisconfiguredMailCheckMailboxErrorMessage", resourceCulture);
            }
        }
        
        internal static string MigrationRuaTagShouldNotHaveMisconfiguredMailCheckMailboxErrorMessage {
            get {
                return ResourceManager.GetString("MigrationRuaTagShouldNotHaveMisconfiguredMailCheckMailboxErrorMessage", resourceCulture);
            }
        }
        
        internal static string RuaTagsShouldContainDmarcServiceMailBoxErrorMessage {
            get {
                return ResourceManager.GetString("RuaTagsShouldContainDmarcServiceMailBoxErrorMessage", resourceCulture);
            }
        }
        
        internal static string MigrationRuaTagsShouldContainDmarcServiceMailBoxErrorMessage {
            get {
                return ResourceManager.GetString("MigrationRuaTagsShouldContainDmarcServiceMailBoxErrorMessage", resourceCulture);
            }
        }
        
        internal static string SubdomainPolicyMustBeQuarantineOrRejectErrorMessage {
            get {
                return ResourceManager.GetString("SubdomainPolicyMustBeQuarantineOrRejectErrorMessage", resourceCulture);
            }
        }
    }
}
