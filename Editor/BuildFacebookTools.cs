using System;
using System.IO;
using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace PluginSet.Facebook.Editor
{
    [BuildTools]
    public static class BuildFacebookTools
    {
        private static void CopyFacebookDependency(string fileName)
        {
            Global.CopyDependenciesFileInLib("com.pluginset.facebook", fileName, "Dependencies");
        }

        [OnSyncEditorSetting]
        public static void OnSyncEditorSetting(BuildProcessorContext context)
        {
            if (context.BuildTarget != BuildTarget.Android && context.BuildTarget != BuildTarget.iOS
                && context.BuildTarget != BuildTarget.WebGL)
                return;

            var buildParams = context.BuildChannels.Get<BuildFacebookParams>();
            if (!buildParams.Enable)
                return;

            context.Symbols.Add("ENABLE_FACEBOOK");
            CopyFacebookDependency("FacebookCoreDependencies.xml");

            if (buildParams.ReportInstall)
                context.Symbols.Add("ENABLE_FACEBOOK_REPORT_INSTALL");

            if (buildParams.EnableLogin)
            {
                context.Symbols.Add("ENABLE_FACEBOOK_LOGIN");
                CopyFacebookDependency("FacebookLoginDependencies.xml");
                if (buildParams.UseLimitedLogin)
                    context.Symbols.Add("FACEBOOK_LOGIN_LIMITED");
            }

            if (buildParams.EnableShare)
            {
                context.Symbols.Add("ENABLE_FACEBOOK_SHARE");
                CopyFacebookDependency("FacebookShareDependencies.xml");
            }

            if (buildParams.EnableAnalytics)
                context.Symbols.Add("ENABLE_FACEBOOK_ANALYTICS");

            if (buildParams.EnablePayment)
            {
                context.Symbols.Add("ENABLE_FACEBOOK_PAYMENT");
                CopyFacebookDependency("FacebookGamingDependencies.xml");
                if (context.BuildTarget == BuildTarget.WebGL)
                    context.AddLinkAssembly("Facebook.Unity.Canvas");
            }

            context.AddLinkAssembly("Facebook.Unity");
            context.AddLinkAssembly("PluginSet.Facebook");
#if UNITY_ANDROID
            context.AddLinkAssembly("Facebook.Unity.Android");
#elif UNITY_IOS
            context.AddLinkAssembly("Facebook.Unity.IOS");
#endif

            var pluginConfig = context.Get<PluginSetConfig>("pluginsConfig");
            var config = pluginConfig.AddConfig<PluginFacebookConfig>("Facebook");
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
            config.LoginNonce = buildParams.LoginNonce;
        }

        [iOSXCodeProjectModify]
        public static void OnIosXcodeProjectModify(BuildProcessorContext context, PBXProjectManager project)
        {
            var buildParams = context.BuildChannels.Get<BuildFacebookParams>();
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
            if (buildParams.EnableShare)
                plist.AddApplicationQueriesSchemes("fb-messenger-share-api");

            project.TryAddCapability(project.MainFramework, PBXCapabilityType.KeychainSharing);
            project.EnableSwiftCompile(project.UnityFramework);
        }

        [AndroidProjectModify]
        public static void OnAndroidProjectModify(BuildProcessorContext context, AndroidProjectManager projectManager)
        {
            var buildParams = context.BuildChannels.Get<BuildFacebookParams>();
            if (!buildParams.Enable)
                return;

            var doc = projectManager.LauncherManifest;
            doc.SetMetaData("com.facebook.sdk.ApplicationId", $"fb{buildParams.AppId}");
            doc.SetMetaData("com.facebook.sdk.ClientToken", $"{buildParams.ClientToken}");
            if (buildParams.EnableAutoLogAppEvents)
                doc.SetMetaData("com.facebook.sdk.AutoLogAppEventsEnabled", "true");
            if (buildParams.EnableAdvertiserIDCollection)
                doc.SetMetaData("com.facebook.sdk.AdvertiserIDCollectionEnabled", "true");

            if (projectManager.LauncherGradle.TargetSdkVersion >= 28
                && (buildParams.EnableLogin || buildParams.EnableShare))
            {
                doc.addQueries("com.facebook.katana");
            }

            var configChanges =
                "fontScale|keyboard|keyboardHidden|locale|mnc|mcc|navigation|orientation|screenLayout|screenSize|smallestScreenSize|uiMode|touchscreen";
            var theme = "@android:style/Theme.Translucent.NoTitleBar.Fullscreen";

            if (buildParams.EnableLogin)
            {
                var activity = doc.FindOrAddActivity("com.facebook.unity.FBUnityLoginActivity");
                activity.SetAttribute("configChanges", AndroidConst.NS_URI, configChanges);
                activity.SetAttribute("theme", AndroidConst.NS_URI, theme);
            }

            if (buildParams.EnableShare)
            {
                var activity = doc.FindOrAddActivity("com.facebook.unity.FBUnityDialogsActivity");
                activity.SetAttribute("configChanges", AndroidConst.NS_URI, configChanges);
                activity.SetAttribute("theme", AndroidConst.NS_URI, theme);
            }

            if (buildParams.EnablePayment)
            {
                var activity = doc.FindOrAddActivity("com.facebook.unity.FBUnityGamingServicesFriendFinderActivity");
                activity.SetAttribute("configChanges", AndroidConst.NS_URI, configChanges);
                activity.SetAttribute("theme", AndroidConst.NS_URI, theme);

                doc.FindOrAddActivity("com.facebook.unity.FBUnityGameRequestActivity");
            }

            doc.FindOrAddActivity("com.facebook.unity.FBUnityAppLinkActivity")
                .SetAttribute("exported", AndroidConst.NS_URI, "true");

            doc.FindOrAddActivity("com.facebook.unity.FBUnityDeepLinkingActivity")
                .SetAttribute("exported", AndroidConst.NS_URI, "true");

            if (buildParams.EnableShare)
            {
                var provider = doc.FindOrAddProvider("com.facebook.FacebookContentProvider");
                provider.SetAttribute("authorities", AndroidConst.NS_URI,
                    $"com.facebook.app.FacebookContentProvider{buildParams.AppId}");
                provider.SetAttribute("exported", AndroidConst.NS_URI, "true");
            }

            var application = projectManager.LibraryManifest.findFirstElement(AndroidConst.META_DATA_PARENT);
            application.RemoveAttribute("label", AndroidConst.NS_URI);
            application.RemoveAttribute("icon", AndroidConst.NS_URI);
        }
    }
}
