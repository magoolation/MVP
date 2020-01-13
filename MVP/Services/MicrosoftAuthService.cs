﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using MVP.Services.Interfaces;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace MVP.Services
{
    public class MicrosoftAuthService : IMicrosoftAuthService
    {
        private const string Scope = "wl.emails wl.signin";
        private const string ClientID = "c18f496a-94e7-4307-87f4-2a255314bb4c";
        private const string RedirectUri = "com.microsoft.mvp://callback";
        private const string AccessTokenUrl = "https://login.live.com/oauth20_token.srf";

        private readonly Uri _signInUri = new Uri($"https://login.live.com/oauth20_authorize.srf?client_id={ClientID}&redirect_uri={HttpUtility.UrlEncode(RedirectUri)}&response_type=code&scope={HttpUtility.UrlEncode(Scope)}");
        private readonly Uri _signOutUri = new Uri($"https://login.live.com/oauth20_logout.srf?client_id={ClientID}&redirect_uri={HttpUtility.UrlEncode(RedirectUri)}");

        /// <summary>
        /// Signin with your Microsoft account.
        /// </summary>
        public async Task SignInAsync(bool isRefresh = false)
        {
            try
            {
                var returnValue = await DependencyService.Get<INativeBrowser>().LaunchBrowserAsync(_signInUri.AbsoluteUri);

                if (!returnValue.IsError)
                {
                    var code = HttpUtility.ParseQueryString(new Uri(returnValue.Response).Query).Get("code");

                    using (var client = new HttpClient())
                    {
                        // Construct the Form content, this is where we add the OAuth token (could be access token or refresh token)
                        var postContent = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("client_id", ClientID),
                        new KeyValuePair<string, string>("grant_type", isRefresh ? "refresh_token" : "authorization_code"),
                        new KeyValuePair<string, string>(isRefresh ? "refresh_token" : "code", code),
                        new KeyValuePair<string, string>("redirect_uri", RedirectUri)
                    });

                        // Variable to hold the response data
                        var responseTxt = "";

                        // post the Form data
                        using (var response = await client.PostAsync(new Uri(AccessTokenUrl), postContent))
                        {
                            // Read the response
                            responseTxt = await response.Content.ReadAsStringAsync();
                        }

                        // Deserialize the parameters from the response
                        var tokenData = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseTxt);

                        if (tokenData.ContainsKey("access_token"))
                        {
                            //TODO: STORE tokenData["access_token"]);
                            //TODO: STORE tokenData["refresh_token"]);

                            var tokenType = tokenData["token_type"];
                            var cleanedAccessToken = tokenData["access_token"].Split('&')[0];

                            // set public property that is "returned"
                            // TODO: STORE $"{tokenType} {cleanedAccessToken}";
                        }
                    }
                }
                else
                {
                    // TODO: What to do here?
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Sign out with your Microsoft account.
        /// </summary>
        public async Task SignOutAsync()
        {
            try
            {
                var returnValue = await DependencyService.Get<INativeBrowser>().LaunchBrowserAsync(_signOutUri.AbsoluteUri);

                if (!returnValue.IsError)
                {
                    var code = HttpUtility.ParseQueryString(new Uri(returnValue.Response).Query).Get("lc");

                    if (!string.IsNullOrEmpty(code))
                    {
                        // Logout the user.
                    }
                }
                else
                {
                    // TODO: What?
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}