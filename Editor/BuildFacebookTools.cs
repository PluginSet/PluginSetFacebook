using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Facebook.Editor
{
    [BuildTools]
    public static class BuildFacebookTools
    {
        [OnSyncEditorSetting]
        public static void OnSyncEditorSetting(BuildProcessorContext context)
        {
            if (context.BuildTarget != BuildTarget.Android && context.BuildTarget != BuildTarget.iOS)
                return;
                
            var buildParams = context.BuildChannels.Get<BuildFacebookParams>("Facebook");
            if (!buildParams.Enable)
                return;
            
            context.Symbols.Add("ENABLE_FACEBOOK");

            var dependenciesPath = Path.Combine(Global.GetPackageFullPath("com.pluginset.facebook"), "Dependencies");
            var targetDepPath = context.Get<string>("pluginDependenciesPath");
            
            if (!Directory.Exists(dependenciesPath))
                throw new Exception($"Cannot find dependenciesPath from {dependenciesPath}");
            if (!Directory.Exists(targetDepPath))
                throw new Exception($"Cannot find targetDepPath to {targetDepPath}");
            
            Global.CopyFileFromDirectory(dependenciesPath, targetDepPath, "FacebookDependencies.xml");

            if (buildParams.ReportInstall)
            {
                context.Symbols.Add("ENABLE_FACEBOOK_REPORT_INSTALL");
            }
            
            if (buildParams.EnablePayment)
            {
                context.Symbols.Add("ENABLE_FACEBOOK_PAYMENT");
                context.AddLinkAssembly("Facebook.Unity.Canvas");
            }

            if (buildParams.EnableLogin)
            {
                context.Symbols.Add("ENABLE_FACEBOOK_LOGIN");
                if (buildParams.UseLimitedLogin)
                    context.Symbols.Add("FACEBOOK_LOGIN_LIMITED");
            }

            if (buildParams.EnableShare)
            {
                context.Symbols.Add("ENABLE_FACEBOOK_SHARE");
            }

            if (buildParams.EnableAnalytics)
            {
                context.Symbols.Add("ENABLE_FACEBOOK_ANALYTICS");
            }

            context.AddLinkAssembly("PluginSet.Facebook");
#if UNITY_ANDROID
            context.AddLinkAssembly("Facebook.Unity.Android");
#elif UNITY_IOS
            context.AddLinkAssembly("Facebook.Unity.IOS");
#endif

            var pluginConfig = context.Get<PluginSetConfig>("pluginsConfig");
            var config = pluginConfig.Get<PluginFacebookConfig>("Facebook");
            config.AppId = buildParams.AppId;
            config.ClientToken = buildParams.ClientToken;
            config.EnableCookie = buildParams.EnableCookie;
            config.EnableLogging = buildParams.EnableLogging;
            config.EnableStatus = buildParams.EnableStatus;
            config.Xfbml = buildParams.Xfbml;
            config.FrictionlessRequests = buildParams.FrictionlessRequests;
            config.AuthResponse = buildParams.AuthResponse;
            config.JavascriptSDKLocale = buildParams.JavascriptSDKLocale;
            config.LoginPermissions = buildParams.LoginPermissions;
        }
        
        [iOSXCodeProjectModify]
        public static void OnIosXcodeProjectModify(BuildProcessorContext context, PBXProjectManager project)
        {
            var buildParams = context.BuildChannels.Get<BuildFacebookParams>("Facebook");
            if (!buildParams.Enable)
                return;

            var plist = project.PlistDocument;
            plist.AddXcodeURLType("facebook", "Editor", $"fb{buildParams.AppId}");
            plist.SetPlistValue("FacebookAppID", buildParams.AppId);
            plist.SetPlistValue("FacebookClientToken", buildParams.ClientToken);
            plist.SetPlistValue("FacebookDisplayName", PlayerSettings.productName);
            if (buildParams.EnableAutoLogAppEvents)
                plist.SetPlistValue("FacebookAutoLogAppEventsEnabled", true);
            if (buildParams.EnableAdvertiserIDCollection)
                plist.SetPlistValue("FacebookAdvertiserIDCollectionEnabled", true);

            plist.AddApplicationQueriesSchemes("fbapi");
            plist.AddApplicationQueriesSchemes("fb-messenger-share-api");
            
            project.TryAddCapability(project.MainFramework, PBXCapabilityType.KeychainSharing);
            project.EnableSwiftCompile(project.UnityFramework);
        }

        [AndroidProjectModify]
        public static void OnAndroidProjectModify(BuildProcessorContext context, AndroidProjectManager projectManager)
        {
            var buildParams = context.BuildChannels.Get<BuildFacebookParams>("Facebook");
            if (!buildParams.Enable)
                return;
            
            var doc = projectManager.LauncherManifest;
            doc.SetMetaData("com.facebook.sdk.ApplicationId", $"fb{buildParams.AppId}");
            doc.SetMetaData("com.facebook.sdk.ClientToken", $"{buildParams.ClientToken}");
            if (buildParams.EnableAutoLogAppEvents)
                doc.SetMetaData("com.facebook.sdk.AutoLogAppEventsEnabled", "true");
            if (buildParams.EnableAdvertiserIDCollection)
                doc.SetMetaData("com.facebook.sdk.AdvertiserIDCollectionEnabled", "true");
            
            if (projectManager.LauncherGradle.TargetSdkVersion >= 28)
            {
                doc.addQueries("com.facebook.katana");
            }

            var configChanges = "fontScale|keyboard|keyboardHidden|locale|mnc|mcc|navigation|orientation|screenLayout|screenSize|smallestScreenSize|uiMode|touchscreen";
            var theme = "@android:style/Theme.Translucent.NoTitleBar.Fullscreen";
            
            var activity = doc.FindOrAddActivity("com.facebook.unity.FBUnityLoginActivity");
            activity.SetAttribute("configChanges", AndroidConst.NS_URI, configChanges);
            activity.SetAttribute("theme", AndroidConst.NS_URI, theme);
            
            activity = doc.FindOrAddActivity("com.facebook.unity.FBUnityDialogsActivity");
            activity.SetAttribute("configChanges", AndroidConst.NS_URI, configChanges);
            activity.SetAttribute("theme", AndroidConst.NS_URI, theme);
            
            activity = doc.FindOrAddActivity("com.facebook.unity.FBUnityGamingServicesFriendFinderActivity");
            activity.SetAttribute("configChanges", AndroidConst.NS_URI, configChanges);
            activity.SetAttribute("theme", AndroidConst.NS_URI, theme);
            
            activity = doc.FindOrAddActivity("com.facebook.unity.FBUnityAppLinkActivity");
            activity.SetAttribute("exported", AndroidConst.NS_URI, "true");
            
            activity = doc.FindOrAddActivity("com.facebook.unity.FBUnityDeepLinkingActivity");
            activity.SetAttribute("exported", AndroidConst.NS_URI, "true");
            
            doc.FindOrAddActivity("com.facebook.unity.FBUnityGameRequestActivity");
            
            var provider = doc.FindOrAddProvider("com.facebook.FacebookContentProvider");
            provider.SetAttribute("authorities", AndroidConst.NS_URI, $"com.facebook.app.FacebookContentProvider{buildParams.AppId}");
            provider.SetAttribute("exported", AndroidConst.NS_URI, "true");

            var application = projectManager.LibraryManifest.findFirstElement(AndroidConst.META_DATA_PARENT);
            application.RemoveAttribute("label", AndroidConst.NS_URI);
            application.RemoveAttribute("icon", AndroidConst.NS_URI);
        }

    }
}