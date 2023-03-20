﻿
using System.Collections.Generic;
using Logger = PluginSet.Core.Logger;
#if ENABLE_FACEBOOK
using System.Collections;
using Facebook.Unity;
using PluginSet.Core;
using UnityEngine;

namespace PluginSet.Facebook
{
    [PluginRegister]
    public partial class PluginFacebook : PluginBase, IStartPlugin
    {
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

        public void DisposePlugin(bool isAppQuit = false)
        {
#if ENABLE_FACEBOOK_LOGIN
            if (!isAppQuit)
                OnGameRestartLogout();
#endif
        }
        
        private void OnInited()
        {
            _inited = true;

#if ENABLE_FACEBOOK_REPORT_INSTALL
            if (PlayerPrefs.GetInt("com.facebook.installed") != 1)
            {
                FB.LogAppEvent("fb_mobile_first_app_launch", 1, new Dictionary<string, object>
                {
                    { "fb_auto_published", false },
                });
                PlayerPrefs.SetInt("com.facebook.installed", 1);
            }
#endif
            
            FB.ActivateApp();
#if ENABLE_FACEBOOK_ANALYTICS
            OnAnalyticsInited();
#endif
        }

        private void OnHideUnityDelegate(bool isUnityShown)
        {
            if (isUnityShown)
                SendNotification(PluginConstants.NOTIFY_APPLICATION_ENTER_FOREGROUND);
            else
                SendNotification(PluginConstants.NOTIFY_APPLICATION_ENTER_BACKGROUND);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && FB.IsInitialized)
                FB.ActivateApp();
        }
    }
}
#endif
