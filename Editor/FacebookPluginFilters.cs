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
            var buildParams = context.BuildChannels.Get<BuildFacebookParams>("Facebook");
            if (!buildParams.Enable)
            {
                Debug.Log("Filter lib file :::::::  " + s);
            }
                
            return !buildParams.Enable;
        }
        
        private static bool FilterGameRoom(string s, BuildProcessorContext context)
        {
            var buildParams = context.BuildChannels.Get<BuildFacebookParams>("Facebook");
            if (!buildParams.Enable)
            {
                Debug.Log("Filter lib file :::::::  " + s);
            }
                
            return !buildParams.Enable || !buildParams.EnableGameRoom;
        }
        
        static FacebookPluginFilters()
        {
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Android", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Android/libs", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/iOS", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Canvas", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Settings", FilterPlugins);
            PluginFilter.RegisterFilter("com.pluginset.facebook/Plugins/Gameroom", FilterGameRoom);
        }
    }
}