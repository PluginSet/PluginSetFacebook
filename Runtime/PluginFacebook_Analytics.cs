#if ENABLE_FACEBOOK_ANALYTICS
using System.Collections.Generic;
using Facebook.Unity;
using PluginSet.Core;
using UnityEngine;

namespace PluginSet.Facebook
{
    public partial class PluginFacebook: IAnalytics
    {
        private struct MissingEvent
        {
            public string name;
            public Dictionary<string, object> data;
        }

        private readonly List<MissingEvent> _missingEvents = new List<MissingEvent>();

        [FacebookInitedExecutable]
        private void OnAnalyticsInited()
        {
            if (_config.AutoReportInstall && PlayerPrefs.GetInt("com.facebook.installed") != 1)
            {
                FB.LogAppEvent("fb_mobile_first_app_launch", 1, new Dictionary<string, object>
                {
                    { "fb_auto_published", false },
                });
            }
            
            FB.ActivateApp();
            
            foreach (var missingEvent in _missingEvents)
            {
                FB.LogAppEvent(missingEvent.name, 1, missingEvent.data);
            }
            _missingEvents.Clear();
        }

        
        public void FlushUserInfo()
        {
        }

        public void CustomEvent(string customEventName, Dictionary<string, object> eventData = null)
        {
            if (!_inited)
            {
                _missingEvents.Add(new MissingEvent
                {
                    name = customEventName,
                    data = eventData
                });
            }
            else
            {
                FB.LogAppEvent(customEventName, 1, eventData);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!pauseStatus && FB.IsInitialized)
                FB.ActivateApp();
        }
    }
}
#endif