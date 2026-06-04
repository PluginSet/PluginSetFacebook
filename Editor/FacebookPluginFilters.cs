using PluginSet.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Facebook.Editor
{
    [InitializeOnLoad]
    public static class FacebookPluginFilters
    {
        private static bool FilterWhenDisabled(string s, BuildProcessorContext context)
        {
            var buildParams = context.BuildChannels.Get<BuildFacebookParams>();
            if (!buildParams.Enable)
                Debug.Log("Filter lib file :::::::  " + s);

            return !buildParams.Enable;
        }

        private static bool FilterCanvasPlugins(string s, BuildProcessorContext context)
        {
            var buildParams = context.BuildChannels.Get<BuildFacebookParams>();
            var enable = buildParams.Enable && buildParams.EnablePayment
                && context.BuildTarget == BuildTarget.WebGL;
            if (!enable)
                Debug.Log("Filter lib file :::::::  " + s);

            return !enable;
        }

        private static bool FilterGamingPlugins(string s, BuildProcessorContext context)
        {
            var buildParams = context.BuildChannels.Get<BuildFacebookParams>();
            var enable = buildParams.Enable && buildParams.EnablePayment;
            if (!enable)
                Debug.Log("Filter lib file :::::::  " + s);

            return !enable;
        }

        static FacebookPluginFilters()
        {
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins", FilterWhenDisabled);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Android", FilterWhenDisabled);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Android/libs", FilterWhenDisabled);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/iOS", FilterWhenDisabled);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Settings", FilterWhenDisabled);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Windows", FilterWhenDisabled);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Windows/x86", FilterWhenDisabled);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Windows/x64", FilterWhenDisabled);

            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Canvas", FilterCanvasPlugins);

            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/iOS/Swift", FilterGamingPlugins);
        }
    }
}
