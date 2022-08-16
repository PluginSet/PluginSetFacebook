using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using PluginSet.Core;
using PluginSet.Core.Editor;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace PluginSet.Facebook.Editor
{
    [BuildTools]
    public static class BuildFacebookTools
    {
        [OnSyncEditorSetting]
        public static void OnSyncEditorSetting(BuildProcessorContext context)
        {
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
            
            if (buildParams.EnablePayment)
            {
                context.Symbols.Add("ENABLE_FACEBOOK_PAYMENT");
                context.AddLinkAssembly("Facebook.Unity.Canvas");
            }

            if (buildParams.EnableLogin)
            {
                context.Symbols.Add("ENABLE_FACEBOOK_LOGIN");
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
        }
        
        [iOSXCodeProjectModify]
        public static void OnIosXcodeProjectModify(BuildProcessorContext context, PBXProjectManager project)
        {
            var buildParams = context.BuildChannels.Get<BuildFacebookParams>("Facebook");
            if (!buildParams.Enable)
                return;

            var plist = project.PlistDocument;
            plist.SetPlistValue("FacebookAppID", buildParams.AppId);
            plist.SetPlistValue("FacebookDisplayName", PlayerSettings.productName);
            plist.SetPlistValue("FacebookAutoLogAppEventsEnabled", true);
            plist.SetPlistValue("FacebookAdvertiserIDCollectionEnabled", true);

            // ------ 添加fb的LSApplicationQueriesSchemes
            Dictionary<string, bool> plistElementArray = new Dictionary<string, bool>();
            plistElementArray.Add("fbapi", true);
            plistElementArray.Add("fbauth", true);
            plistElementArray.Add("fbauth2", true);
            plistElementArray.Add("fbshareextension", true);

            PlistElement lsApplicationQueriesSchemes;
            plist.root.values.TryGetValue("LSApplicationQueriesSchemes", out lsApplicationQueriesSchemes);

            //如果infoPlist里面有LSApplicationQueriesSchemes 就先取出来
            if (lsApplicationQueriesSchemes != null &&
                lsApplicationQueriesSchemes.GetType() == typeof(PlistElementArray))
            {
                var plistElementDictionaries = lsApplicationQueriesSchemes.AsArray().values
                    .Where(plistElement => plistElement.GetType() == typeof(PlistElementArray));
                foreach (var plistElement in plistElementDictionaries)
                {
                    PlistElement existingId;
                    var elementArray = plistElement.AsArray().values;
                    for (int i = 0; i < elementArray.Count; i++)
                    {
                        existingId = elementArray[i];
                        if (existingId == null || existingId.GetType() != typeof(PlistElementString) ||
                            string.IsNullOrEmpty(existingId.AsString())) continue;
                        //去重
                        if (!plistElementArray.ContainsKey((existingId.AsString())))
                            plistElementArray.Add(existingId.AsString(), true);
                    }
                }
            }
            else
            {
                //如果没有就创建一个新的
                lsApplicationQueriesSchemes = plist.root.CreateArray("LSApplicationQueriesSchemes");
            }

            foreach (string key in plistElementArray.Keys)
            {
                //增加LSApplicationQueriesSchemes的字段
                lsApplicationQueriesSchemes.AsArray().AddString(key);
            }
            //-------end

            //-------添加fb的CFBundleURLTypes
            List<PlistElementDict> _tempCf = new List<PlistElementDict>();
            PlistElementDict fbDict = new PlistElementDict();
            PlistElementArray fbId = fbDict.CreateArray("CFBundleURLSchemes");
            fbId.AddString($"fb{buildParams.AppId}");
            _tempCf.Add(fbDict);

            PlistElement cfBundleURLTypes;
            plist.root.values.TryGetValue("CFBundleURLTypes", out cfBundleURLTypes);
            //如果infoPlist里面有CFBundleURLTypes 就先取出来
            if (cfBundleURLTypes != null && cfBundleURLTypes.GetType() == typeof(PlistElementArray))
            {
                var plistElementDictionaries = cfBundleURLTypes.AsArray().values
                    .Where(plistElement => plistElement.GetType() == typeof(PlistElementArray));
                foreach (var plistElement in plistElementDictionaries)
                {
                    _tempCf.Add(plistElement.AsDict());
                }
            }
            else
            {
                //如果没有就创建一个新的
                cfBundleURLTypes = plist.root.CreateArray("CFBundleURLTypes");
            }

            for (int i = 0; i < _tempCf.Count; i++)
            {
                var _addDict = cfBundleURLTypes.AsArray().AddDict();
                if (_tempCf[i].AsDict().values.ContainsKey("CFBundleTypeRole"))
                {
                    _addDict.SetString("CFBundleTypeRole", _tempCf[i].AsDict().values["CFBundleTypeRole"].AsString());
                }

                if (_tempCf[i].AsDict().values.ContainsKey("CFBundleURLName"))
                {
                    _addDict.SetString("CFBundleURLName", _tempCf[i].AsDict().values["CFBundleURLName"].AsString());
                }

                if (_tempCf[i].AsDict().values.ContainsKey("CFBundleURLSchemes"))
                {
                    var _tempAr = _tempCf[i].AsDict().values["CFBundleURLSchemes"].AsArray();
                    var _newAr = _addDict.CreateArray("CFBundleURLSchemes");
                    for (int j = 0; j < _tempAr.values.Count; j++)
                    {
                        _newAr.AddString(_tempAr.values[j].AsString());
                    }
                }
            }
            //-------end


            var pbxProject = project.Project;
#if UNITY_2019_3_OR_NEWER
			string xcodeTarget = pbxProject.GetUnityFrameworkTargetGuid();
#else
			string xcodeTarget = pbxProject.TargetGuidByName("Unity-iPhone");
#endif

#if UNITY_IOS
            var projectPath = context.Get<string>("projectPath");
            Global.EnableSwiftCompile(pbxProject, projectPath, xcodeTarget);
#endif
        }

        [AndroidProjectModify]
        public static void OnAndroidProjectModify(BuildProcessorContext context, AndroidProjectManager projectManager)
        {
            var buildParams = context.BuildChannels.Get<BuildFacebookParams>("Facebook");
            if (!buildParams.Enable)
                return;

            var doc = projectManager.LauncherManifest;
            doc.SetMetaData("com.facebook.sdk.ApplicationId", $"fb{buildParams.AppId}");
            doc.SetMetaData("com.facebook.sdk.AutoLogAppEventsEnabled", "true");
            doc.SetMetaData("com.facebook.sdk.AdvertiserIDCollectionEnabled", "true");
        }

    }
}