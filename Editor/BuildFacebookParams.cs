using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEngine;

namespace PluginSet.Facebook.Editor
{
    [BuildChannelsParams("Facebook", "Facebook应用设置")]
    [VisibleCaseBoolValue("SupportAndroid", true)]
    [VisibleCaseBoolValue("SupportIOS", true)]
    public class BuildFacebookParams: ScriptableObject
    {
        [Tooltip("是否启用Facebook")]
        public bool Enable;
        
        [Tooltip("是否手动上报安装事件")]
        [VisibleCaseBoolValue("Enable", true)]
        public bool ReportInstall;
        
        [Tooltip("是否启用Facebook数据收集")]
        [VisibleCaseBoolValue("Enable", true)]
        public bool EnableAnalytics;
        
        [Tooltip("是否启用Facebook登录")]
        [VisibleCaseBoolValue("Enable", true)]
        public bool EnableLogin;
        
        [Tooltip("是否启用 Facebook Gaming/支付相关能力（Friend Finder、Game Request 等；Android 的 gamingservices 库在启用 Facebook 时已随 Core 依赖打入）")]
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

        [Tooltip("启用自动数据收集，未使用Analytics也能生效")]
        public bool EnableAutoLogAppEvents = true;

        [Tooltip("启用广告ID收集")]
        public bool EnableAdvertiserIDCollection = true;

        [Tooltip("Facebook 参数：？")]
        public bool Xfbml = false;

        [Tooltip("Facebook 参数：？")]
        public bool FrictionlessRequests = true;

        [Tooltip("Facebook 参数：？")]
        public string AuthResponse;

        [Tooltip("Facebook 参数：？")]
        public string JavascriptSDKLocale = "en_US";

        [Tooltip("是否使用受限的登录方式，以通过服务器验证登录")]
        public bool UseLimitedLogin = true;

        [Tooltip("iOS 登录 nonce，留空则运行时自动生成")]
        public string LoginNonce;

        [Tooltip("Facebook登录时的授权请求")]
        public string[] LoginPermissions;
    }
}