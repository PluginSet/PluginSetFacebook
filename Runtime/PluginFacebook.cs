#if ENABLE_FACEBOOK
using System;
using Logger = PluginSet.Core.Logger;
using System.Collections;
using Facebook.Unity;
using PluginSet.Core;
using UnityEngine;

namespace PluginSet.Facebook
{
    [PluginRegister]
    public partial class PluginFacebook : PluginBase, IStartPlugin
    {
        [AttributeUsage(AttributeTargets.Method)]
        private class FacebookInitedExecutableAttribute: ExecutableAttribute
        {
        }
        
        [AttributeUsage(AttributeTargets.Method)]
        private class FacebookDisposeExecutableAttribute: ExecutableAttribute
        {
        }
        
        private static readonly Logger Logger = LoggerManager.GetLogger("Facebook");
        public override string Name => "Facebook";

        public int StartOrder => PluginsStartOrder.SdkDefault;
        public bool IsRunning { get; private set; }

        private bool _inited;

        private PluginFacebookConfig _config;

        private static string CheckString(string input, string defaultOutput = null)
        {
            if (string.IsNullOrEmpty(input))
                return defaultOutput;

            return input;
        }
        
        protected override void Init(PluginSetConfig config)
        {
            _config = config.Get<PluginFacebookConfig>();
        }

        public IEnumerator StartPlugin()
        {
            if (IsRunning)
                yield break;

            try
            {
                FB.Init(
                    _config.AppId,
                    CheckString(_config.ClientToken),
                    _config.EnableCookie,
                    _config.EnableLogging,
                    _config.EnableStatus,
                    _config.Xfbml,
                    _config.FrictionlessRequests,
                    CheckString(_config.AuthResponse),
                    CheckString(_config.JavascriptSDKLocale, "en_US"),
                    OnHideUnityDelegate,
                    OnInited
                    );
            }
            catch (System.Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        public void DisposePlugin(bool isAppQuit = false)
        {
            ExecuteAll<FacebookDisposeExecutableAttribute>(isAppQuit);
        }
        
        private void OnInited()
        {
            _inited = true;
            
            ExecuteAll<FacebookInitedExecutableAttribute>();
            PlayerPrefs.SetInt("com.facebook.installed", 1);
        }

        private void OnHideUnityDelegate(bool isUnityShown)
        {
            if (isUnityShown)
                SendNotification(PluginConstants.NOTIFY_APPLICATION_ENTER_FOREGROUND);
            else
                SendNotification(PluginConstants.NOTIFY_APPLICATION_ENTER_BACKGROUND);
        }
    }
}
#endif
