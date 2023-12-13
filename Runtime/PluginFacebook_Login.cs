#if ENABLE_FACEBOOK_LOGIN
using System;
using System.Collections.Generic;
using Facebook.Unity;
using PluginSet.Core;

namespace PluginSet.Facebook
{
    public partial class PluginFacebook: ILoginPlugin
    {
        private readonly List<Action<Result>> _loginCallbacks = new List<Action<Result>>();

        public bool IsEnableLogin => _inited;

        public bool IsLoggedIn => !string.IsNullOrEmpty(_loginData);

        private string _loginData = null;

        [FacebookDisposeExecutable]
        private void OnPluginDispose(bool isAppQuit)
        {
            if (isAppQuit)
                return;
            
            LogoutInternal();
        }
        
        public void Login(Action<Result> callback = null, string _ = null)
        {
            if (callback != null)
                _loginCallbacks.Add(callback);

            
#if UNITY_IOS
#if FACEBOOK_LOGIN_LIMITED
            FB.Mobile.LoginWithTrackingPreference(LoginTracking.LIMITED, _config.LoginPermissions, "limited_nonce123", OnFacebookLoginResult);
#else
            FB.Mobile.LoginWithTrackingPreference(LoginTracking.ENABLED, _config.LoginPermissions, "classic_nonce123", OnFacebookLoginResult);
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
                    Logger.Error($"Login result callback error:{e.Message}:{e}");
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
                Data = _loginData
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
            
            // Some platforms return the empty string instead of null.
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
//                var token = result.AccessToken;
//                OnLoginSuccess(token.UserId, token.TokenString, result.AuthenticationToken.TokenString);
                OnLoginSuccess(result.RawResult);
            }
            else
            {
                OnLoginFail(PluginConstants.FailDefaultCode, "No Response");
            }
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