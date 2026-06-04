#if ENABLE_FACEBOOK_PAYMENT
using System;
using System.Collections.Generic;
using Facebook.MiniJSON;
using Facebook.Unity;
using PluginSet.Core;

namespace PluginSet.Facebook
{
    public partial class PluginFacebook: IPaymentPlugin
    {
        public bool IsEnablePayment => _inited;

        public void Pay(string productId, Action<PurchaseResult> callback = null, string jsonData = null)
        {
#if !UNITY_WEBGL
            InvokePayFail(callback, productId, PluginConstants.InvalidCode,
                "Facebook payment is only supported on WebGL (Canvas).");
            return;
#else
            if (!_inited)
            {
                InvokePayFail(callback, productId, PluginConstants.InvalidCode,
                    "Facebook SDK is not initialized.");
                return;
            }

            FB.Canvas.PayWithProductId(productId, "purchaseiap", null, null, result =>
            {
                if (callback == null)
                    return;

                var payResult = BuildPurchaseResult(productId, result);
                try
                {
                    callback.Invoke(payResult);
                }
                catch (Exception e)
                {
                    Logger.Error($"Pay callback error:{e.Message}:{e}");
                }
            });
#endif
        }

        private static void InvokePayFail(Action<PurchaseResult> callback, string productId, int code, string error)
        {
            callback?.Invoke(new PurchaseResult
            {
                Success = false,
                PluginName = "Facebook",
                Code = code,
                Error = error,
                Data = new PurchaseCallback { productId = productId },
            });
        }

        private PurchaseResult BuildPurchaseResult(string productId, IPayResult result)
        {
            var payResult = new PurchaseResult
            {
                PluginName = Name,
                Data = new PurchaseCallback { productId = productId },
            };

            if (result == null)
            {
                payResult.Success = false;
                payResult.Error = "No result";
                payResult.Code = PluginConstants.FailDefaultCode;
                return payResult;
            }

            if (!string.IsNullOrEmpty(result.Error))
            {
                payResult.Success = false;
                payResult.Error = result.Error;
                payResult.Code = PluginConstants.FailDefaultCode;
                return payResult;
            }

            if (result.Cancelled)
            {
                payResult.Success = false;
                payResult.Error = "User cancelled";
                payResult.Code = PluginConstants.CancelCode;
                return payResult;
            }

            if (string.IsNullOrEmpty(result.RawResult))
            {
                payResult.Success = false;
                payResult.Code = PluginConstants.FailDefaultCode;
                payResult.Error = "No response";
                return payResult;
            }

            var json = Json.Deserialize(result.RawResult) as Dictionary<string, object>;
            if (json != null && json.TryGetValue("status", out var status) && "completed".Equals(status))
            {
                payResult.Success = true;
                payResult.Code = PluginConstants.SuccessCode;
                if (json.TryGetValue("payment_id", out var paymentId))
                    payResult.Data.transactionId = paymentId.ToString();
                return payResult;
            }

            payResult.Success = false;
            payResult.Code = PluginConstants.FailDefaultCode;
            if (json != null && json.TryGetValue("body", out var body))
            {
                var bodyDict = body as Dictionary<string, object>;
                if (bodyDict != null && bodyDict.TryGetValue("error", out var error))
                {
                    var errorDict = error as Dictionary<string, object>;
                    if (errorDict != null && errorDict.TryGetValue("message", out var message))
                        payResult.Error = message.ToString();
                }
            }

            if (string.IsNullOrEmpty(payResult.Error))
                payResult.Error = result.RawResult;

            return payResult;
        }
    }
}
#endif
