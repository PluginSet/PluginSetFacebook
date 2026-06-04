#if ENABLE_FACEBOOK_LOGIN
using System;
using System.Collections.Generic;
using Facebook.MiniJSON;
using Facebook.Unity;
using PluginSet.Core;

namespace PluginSet.Facebook
{
    public partial class PluginFacebook : ILoginPlugin
    {
        private const string PublicProfilePermission = "public_profile";

        private readonly List<Action<Result>> _loginCallbacks = new List<Action<Result>>();

        public bool IsEnableLogin => _inited;

        public bool IsLoggedIn => _inited && (FB.IsLoggedIn || !string.IsNullOrEmpty(_loginData));

        private string _loginData;

        public void Login(Action<Result> callback = null, string _ = null)
        {
            if (!_inited)
            {
                InvokeLoginFail(callback, PluginConstants.InvalidCode, "Facebook SDK is not initialized.");
                return;
            }

            if (callback != null)
                _loginCallbacks.Add(callback);

#if UNITY_IOS
            var nonce = ResolveLoginNonce();
#if FACEBOOK_LOGIN_LIMITED
            FB.Mobile.LoginWithTrackingPreference(LoginTracking.LIMITED, _config.LoginPermissions, nonce, OnFacebookLoginResult);
#else
            FB.Mobile.LoginWithTrackingPreference(LoginTracking.ENABLED, _config.LoginPermissions, nonce, OnFacebookLoginResult);
#endif
#else
            FB.LogInWithReadPermissions(_config.LoginPermissions, OnFacebookLoginResult);
#endif
        }

        public void Logout(Action<Result> callback = null, string _ = null)
        {
            LogoutInternal();

            callback?.Invoke(new Result
            {
                Success = true,
                PluginName = Name,
                Code = PluginConstants.SuccessCode,
            });
        }

        public string GetUserLoginData()
        {
            return _loginData;
        }

        private static bool HasPublicProfilePermission(string[] permissions)
        {
            if (permissions == null || permissions.Length == 0)
                return false;

            foreach (var permission in permissions)
            {
                if (string.Equals(permission, PublicProfilePermission, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private bool ShouldFetchProfileName()
        {
            return HasPublicProfilePermission(_config?.LoginPermissions);
        }

        private string ResolveLoginNonce()
        {
            if (!string.IsNullOrEmpty(_config?.LoginNonce))
                return _config.LoginNonce;

            return Guid.NewGuid().ToString("N");
        }

        private void InvokeLoginFail(Action<Result> callback, int code, string error)
        {
            if (callback == null)
                return;

            try
            {
                callback.Invoke(new Result
                {
                    Success = false,
                    PluginName = Name,
                    Code = code,
                    Error = error,
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Login callback error:{e.Message}:{e}");
            }
        }

        private void OnLoginResult(ref Result result)
        {
            result.PluginName = Name;
            foreach (var callback in _loginCallbacks)
            {
                try
                {
                    callback.Invoke(result);
                }
                catch (Exception e)
                {
                    Logger.Error($"Login result callback error:{e}");
                }
            }

            _loginCallbacks.Clear();
        }

        private void OnLoginSuccess(string loginData)
        {
            _loginData = loginData;

            var result = new Result
            {
                Success = true,
                Code = PluginConstants.SuccessCode,
                Data = _loginData,
            };
            OnLoginResult(ref result);
        }

        private void OnLoginFail(int code, string error = "")
        {
            _loginData = null;
            var result = new Result
            {
                Success = false,
                Code = code,
                Error = error,
            };
            OnLoginResult(ref result);
        }

        private void OnFacebookLoginResult(ILoginResult result)
        {
            if (result == null)
            {
                OnLoginFail(PluginConstants.FailDefaultCode);
                return;
            }

            if (!string.IsNullOrEmpty(result.Error))
            {
                OnLoginFail(PluginConstants.FailDefaultCode, result.Error);
            }
            else if (result.Cancelled)
            {
                OnLoginFail(PluginConstants.CancelCode, result.RawResult);
            }
            else if (!string.IsNullOrEmpty(result.RawResult))
            {
                if (ShouldFetchProfileName())
                    FetchProfileNameAndCompleteLogin(result.RawResult);
                else
                    OnLoginSuccess(result.RawResult);
            }
            else
            {
                OnLoginFail(PluginConstants.FailDefaultCode, "No Response");
            }
        }

        private void FetchProfileNameAndCompleteLogin(string rawResult)
        {
            TryResolveProfileName(name =>
            {
                OnLoginSuccess(MergeNickNameIntoLoginData(rawResult, name));
            });
        }

        private void TryResolveProfileName(Action<string> onComplete)
        {
#if UNITY_IOS || UNITY_ANDROID
            var profile = FB.Mobile.CurrentProfile();
            if (profile != null && !string.IsNullOrEmpty(profile.Name))
            {
                onComplete?.Invoke(profile.Name);
                return;
            }
#endif
            if (!FB.IsLoggedIn)
            {
                onComplete?.Invoke(null);
                return;
            }

            FB.API("/me?fields=name", HttpMethod.GET, graphResult =>
            {
                string nickName = null;
                if (string.IsNullOrEmpty(graphResult?.Error)
                    && graphResult?.ResultDictionary != null
                    && graphResult.ResultDictionary.TryGetValue("name", out var nameObj)
                    && nameObj != null)
                {
                    nickName = nameObj.ToString();
                }
                else if (!string.IsNullOrEmpty(graphResult?.Error))
                {
                    Logger.Debug("Facebook profile name fetch failed: " + graphResult.Error);
                }

                onComplete?.Invoke(nickName);
            });
        }

        private static string MergeNickNameIntoLoginData(string rawResult, string nickName)
        {
            if (string.IsNullOrEmpty(nickName))
                return rawResult;

            try
            {
                var dict = Json.Deserialize(rawResult) as Dictionary<string, object>;
                if (dict == null)
                    return rawResult;

                dict["nickName"] = nickName;
                return Json.Serialize(dict);
            }
            catch (Exception e)
            {
                Logger.Warn("Failed to merge nickName into Facebook login data: " + e.Message);
                return rawResult;
            }
        }

        private void OnGameRestartLogout()
        {
            LogoutInternal();
        }

        private void LogoutInternal()
        {
            if (FB.IsLoggedIn)
                FB.LogOut();

            _loginData = null;
        }
    }
}
#endif
