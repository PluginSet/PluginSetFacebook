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
        
        public void FlushUserInfo()
        {
        }

        public void CustomEvent(string customEventName, Dictionary<string, object> eventData = null)
        {
            if (!IsEnableLogin)
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

        private void OnAnalyticsInited()
        {
            foreach (var missingEvent in _missingEvents)
            {
                FB.LogAppEvent(missingEvent.name, 1, missingEvent.data);
            }
            _missingEvents.Clear();
            
//            AddEventListener(PluginConstants.NOTIFY_PAY_SUCCESS, OnPaymentSuccess);
        }

#if false
        private void OnPaymentSuccess(PluginsEventContext context)
        {
            var data = JsonUtility.FromJson<PaymentData>((string)context.Data);
            Logger.Info($"PluginFacebook OnPurchaseSuccessEvent with {data.productId} {data.price} {data.currency} {data.transactionId} {data.extra}");

            FB.LogPurchase((float)data.price, data.currency, new Dictionary<string, object>
            {
                {"product_id", data.productId},
                {"payment_type",Devices.GetAppData("channel", "unknown")},
                {"transaction_id", data.transactionId},
            });
        }
#endif
    }
}
#endif