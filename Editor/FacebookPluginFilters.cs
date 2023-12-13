using PluginSet.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Facebook.Editor
{
    [InitializeOnLoad]
    public static class FacebookPluginFilters
    {
        private static bool FilterPlugins(string s, BuildProcessorContext context)
        {
            var buildParams = context.BuildChannels.Get<BuildFacebookParams>();
            if (!buildParams.Enable)
            {
                Debug.Log("Filter lib file :::::::  " + s);
            }
                
            return !buildParams.Enable;
        }
        
        static FacebookPluginFilters()
        {
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Android", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Android/libs", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/iOS", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/iOS/Swift", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Editor", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Canvas", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Settings", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Windows", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Windows/x86", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Windows/x64", FilterPlugins);
        }
    }
}