#if ENABLE_FACEBOOK_LOGIN
using System;
using System.Collections.Generic;
using Facebook.Unity;
using PluginSet.Core;
using LoginResult = PluginSet.Core.LoginResult;

namespace PluginSet.Facebook
{
    public partial class PluginFacebook: ILoginPlugin
    {
        private readonly List<string> LoginPermissions = new List<string>
        {
            "publish_actions"
        };
        
        public bool IsEnableLogin => _inited;

        private readonly List<Action<LoginResult>> _loginCallbacks = new List<Action<LoginResult>>();

        public void Login(Action<LoginResult> callback = null)
        {
            if (callback != null)
                _loginCallbacks.Add(callback);
            
            FB.LogInWithPublishPermissions(LoginPermissions, OnFacebookLoginResult);
        }

        private void OnLoginResult(ref LoginResult result)
        {
            result.PluginName = Name;
            foreach (var callback in _loginCallbacks)
            {
                callback.Invoke(result);
            }
            _loginCallbacks.Clear();
        }

        private void OnLoginSuccess(string userId, string token, string extra = "")
        {
            var result = new LoginResult
            {
                Success = true,
                Token = token,
                Extra = extra,
                UserInfo = new UserInfo
                {
                    UserId = userId
                }
            };
            OnLoginResult(ref result);
        }

        private void OnLoginFail(string error = "")
        {
            var result = new LoginResult
            {
                Success = false,
                Error = error
            };
            OnLoginResult(ref result);
        }

        private void OnFacebookLoginResult(ILoginResult result)
        {
            if (result == null)
            {
                OnLoginFail();
                return;
            }
            
            // Some platforms return the empty string instead of null.
            if (!string.IsNullOrEmpty(result.Error))
            {
                OnLoginFail(result.Error);
            }
            else if (result.Cancelled)
            {
                OnLoginFail(result.RawResult);
            }
            else if (!string.IsNullOrEmpty(result.RawResult))
            {
                var token = result.AccessToken;
                OnLoginSuccess(token.UserId, token.TokenString, result.AuthenticationToken.TokenString);
            }
            else
            {
                OnLoginFail("No Response");
            }
        }
        
        private void OnGameRestartLogout()
        {
            if (FB.IsLoggedIn)
                FB.LogOut();
        }
    }
}
#endif