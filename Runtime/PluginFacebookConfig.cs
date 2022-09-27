using PluginSet.Core;
using UnityEngine;

namespace PluginSet.Facebook
{
    [PluginSetConfig("Facebook")]
    public class PluginFacebookConfig: ScriptableObject
    {
        public string AppId;
        
        public string ClientToken;

        public bool EnableCookie = true;

        public bool EnableLogging = true;

        public bool EnableStatus = true;

        public bool Xfbml = false;

        public bool FrictionlessRequests = true;

        public string AuthResponse;

        public string JavascriptSDKLocale = "en_US";
        
        public string[] LoginPermissions = new[] { "public_profile", "email" };
    }
}