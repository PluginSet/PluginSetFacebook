using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEngine;

namespace PluginSet.Facebook.Editor
{
    [BuildChannelsParams("Facebook", "Facebook应用设置")]
    public class BuildFacebookParams: ScriptableObject
    {
        [Tooltip("是否启用Facebook")]
        public bool Enable;
        
        [Tooltip("是否启用Facebook数据收集")]
        [VisibleCaseBoolValue("Enable", true)]
        public bool EnableAnalytics;
        
        [Tooltip("是否启用Facebook登录")]
        [VisibleCaseBoolValue("Enable", true)]
        public bool EnableLogin;
        
        [Tooltip("是否启用Facebook支付")]
        [VisibleCaseBoolValue("Enable", true)]
        public bool EnablePayment;
        
        [Tooltip("是否启用Facebook分享")]
        [VisibleCaseBoolValue("Enable", true)]
        public bool EnableShare;

        [Tooltip("Facebook提供的AppId")]
        public string AppId;
        
        [Tooltip("Facebook 参数：？")]
        public string ClientToken;

        [Tooltip("Facebook 参数：？")]
        public bool EnableCookie = true;

        [Tooltip("Facebook 参数：？")]
        public bool EnableLogging = true;

        [Tooltip("Facebook 参数：？")]
        public bool EnableStatus = true;

        [Tooltip("Facebook 参数：？")]
        public bool Xfbml = false;

        [Tooltip("Facebook 参数：？")]
        public bool FrictionlessRequests = true;

        [Tooltip("Facebook 参数：？")]
        public string AuthResponse;

        [Tooltip("Facebook 参数：？")]
        public string JavascriptSDKLocale = "en_US";
    }
}