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
                        success?.Invoke();
                    else
                        fail?.Invoke();
                });
        }
    }
}
#endif