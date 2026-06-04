#if ENABLE_FACEBOOK_ANALYTICS
using System.Collections.Generic;
using Facebook.Unity;
using PluginSet.Core;

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
                return;
            }

            FB.LogAppEvent(customEventName, 1, eventData);
        }

        private void OnAnalyticsInited()
        {
            foreach (var missingEvent in _missingEvents)
                FB.LogAppEvent(missingEvent.name, 1, missingEvent.data);

            _missingEvents.Clear();
        }
    }
}
#endif
