using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using MVCDemo.Infrastructure;
using Owin;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityModel.Client;

[assembly: OwinStartup(typeof(MVCDemo.Startup))]

namespace MVCDemo
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            //app.UseResourceAuthorization(new AuthorizationManager());

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Cookies",
                CookieName = "AuthCookie",
                CookiePath = "/demo/"
            });
            const string clientId = "demoISkatana";
            const string clientSecret = "secret";
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = "https://localhost:44311/identity",
                ClientId = clientId,
                ResponseType = "code id_token token",
                Scope = "openid email profile offline_access",
                ClientSecret = clientSecret,
                RedirectUri = "https://localhost:44314/demo/auth/authresponse/",
                PostLogoutRedirectUri = "https://localhost:44314/demo/auth/loggedout/",

                SignInAsAuthenticationType = "Cookies",
                UseTokenLifetime = false,

                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                        AuthorizationCodeReceived = async n =>
                            {
                                // filter "protocol" claims
                                var claims = new List<Claim>(from c in n.AuthenticationTicket.Identity.Claims
                                                             where c.Type != Constants.ClaimTypes.Issuer &&
                                                                   c.Type != Constants.ClaimTypes.Audience &&
                                                                   c.Type != Constants.ClaimTypes.NotBefore &&
                                                                   c.Type != Constants.ClaimTypes.Expiration &&
                                                                   c.Type != Constants.ClaimTypes.IssuedAt &&
                                                                   c.Type != Constants.ClaimTypes.Nonce &&
                                                                   c.Type != Constants.ClaimTypes.AuthorizationCodeHash &&
                                                                   c.Type != Constants.ClaimTypes.AccessTokenHash
                                                             select c);

                                // get userinfo data
                                var userInfoClient = new UserInfoClient(
                                    new Uri("https://localhost:44311/identity/connect/userinfo"),
                                    n.ProtocolMessage.AccessToken);

                                var userInfo = await userInfoClient.GetAsync();
                                userInfo.Claims.ToList().ForEach(ui => claims.Add(new Claim(ui.Item1, ui.Item2)));

                                // get access and refresh token
                                var tokenClient = new OAuth2Client(
                                    new Uri("https://localhost:44311/identity/connect/token"),
                                    clientId,
                                    clientSecret);

                                var response = await tokenClient.RequestAuthorizationCodeAsync(n.Code, n.RedirectUri);
                                //if (response.AccessToken != null)
                                //{
                                //    claims.Add(new Claim("access_token", response.AccessToken));
                                //    claims.Add(new Claim("expires_at", DateTime.Now.AddSeconds(response.ExpiresIn).ToLocalTime().ToString()));
                                //}
                                if (response.RefreshToken != null)
                                    claims.Add(new Claim("refresh_token", response.RefreshToken));
                                if (n.ProtocolMessage.IdToken != null)
                                    claims.Add(new Claim("id_token", n.ProtocolMessage.IdToken));

                                var claimsId = new ClaimsIdentity(claims.Distinct(new ClaimComparer()),
                                    n.AuthenticationTicket.Identity.AuthenticationType,
                                    Constants.ClaimTypes.Name,
                                    Constants.ClaimTypes.Role);
                                n.AuthenticationTicket = new AuthenticationTicket(
                                    claimsId, n.AuthenticationTicket.Properties);
                            },

                    RedirectToIdentityProvider = async n =>
                    {
                        if (n.ProtocolMessage.RequestType == OpenIdConnectRequestType.LogoutRequest)
                        {
                            var token = n.OwinContext.Authentication.User.FindFirst("id_token");
                            if (token != null)
                            {
                                var idTokenHint = token.Value;
                                n.ProtocolMessage.IdTokenHint = idTokenHint;
                            }
                        }
                    }
                }

            });
        }
    }
}