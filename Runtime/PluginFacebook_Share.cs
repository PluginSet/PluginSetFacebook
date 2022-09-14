#if ENABLE_FACEBOOK_SHARE
using System;
using Facebook.Unity;
using PluginSet.Core;

namespace PluginSet.Facebook
{
    public partial class PluginFacebook: IShareWebPagePlugin
    {
        public bool IsEnableShare => _inited;

        public void ShareWebPage(string webUrl, Action success = null, Action fail = null, string title = null, string desc = null,
            string image = null)
        {
            Uri imageUri = null;
            if (!string.IsNullOrEmpty(image))
                imageUri = new Uri(image);
            FB.ShareLink(new Uri(webUrl)
                , title, desc, imageUri
                , delegate(IShareResult result)
                {
                    if (result != null && !result.Cancelled && string.IsNullOrEmpty(result.Error))
                    {
                        if (success != null)
                        {
                            try
                            {
                                success.Invoke();
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"Share callback success error:{e.Message}:{e}");
                            }
                        }
                    }
                    else
                    {
                        if (fail != null)
                        {
                            try
                            {
                                fail.Invoke();
                            }
                            catch (Exception e)
                            {
                                Logger.Error($"Share callback fail error:{e.Message}:{e}");
                            }
                        }
                    }
                });
        }
    }
}
#endif