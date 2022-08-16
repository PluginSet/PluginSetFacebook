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
        public void Pay(string productId, Action<Result> callback = null, string jsonData = null)
        {
            FB.Canvas.PayWithProductId(productId, "purchaseiap"
                , null, null
                , delegate(IPayResult result)
                {
                    if (callback == null)
                        return;

                    var payResult = new Result();
                    payResult.PluginName = Name;
                    payResult.Data = productId;
                    if (result == null)
                    {
                        payResult.Success = false;
                        payResult.Error = "No result";
                        payResult.Code = PluginConstants.FailDefaultCode;
                    }
                    else if (!string.IsNullOrEmpty(result.Error))
                    {
                        payResult.Success = false;
                        payResult.Error = result.Error;
                        payResult.Code = PluginConstants.FailDefaultCode;
                    }
                    else if (result.Cancelled)
                    {
                        payResult.Success = false;
                        payResult.Error = "User cancelled";
                        payResult.Code = PluginConstants.CancelCode;
                    }
                    else if (!string.IsNullOrEmpty(result.RawResult))
                    {
                        var json = Json.Deserialize(result.RawResult) as Dictionary<string, object>;
                        if (json != null && json.TryGetValue("status", out var status) && "completed".Equals(status))
                        {
                            payResult.Success = true;
                            payResult.Code = PluginConstants.SuccessCode;
                            payResult.Data = result.RawResult;
//
//                            if (json.TryGetValue("payment_id", out var paymentId))
//                                payResult.TransactionId = paymentId.ToString();
//                            if (json.TryGetValue("signed_request", out var sign))
//                                payResult.Extra = sign.ToString();
                        }
                        else
                        {
                            payResult.Success = false;
                            payResult.Code = PluginConstants.FailDefaultCode;

                            if (json != null && json.TryGetValue("body", out var body))
                            {
                                var bodyDict = (Dictionary<string, object>) body;
                                if (bodyDict != null && bodyDict.TryGetValue("error", out var error))
                                {
                                    var errorDict = (Dictionary<string, object>) error;
                                    if (errorDict != null && errorDict.TryGetValue("message", out var message))
                                        payResult.Error = message.ToString();
                                }
                            }
                        }
                    }
                    else
                    {
                        payResult.Success = false;
                        payResult.Code = PluginConstants.FailDefaultCode;
                        payResult.Error = "No response";
                    }
                    callback.Invoke(payResult);
            });
        }
    }
}
#endif