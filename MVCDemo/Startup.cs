using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using MVCDemo.Infrastructure;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Thinktecture.IdentityModel;
using Thinktecture.IdentityModel.Client;
using Thinktecture.IdentityServer.v3.AccessTokenValidation;

[assembly: OwinStartup(typeof(MVCDemo.Startup))]

namespace MVCDemo
{
    public class Startup
    {
        //*** prefix ast - "app start up" - help keep from having overlapping keys in appSettings
        internal static class StTags
        {
            internal const string ast_Authority = "ast:Authority";
            internal const string ast_TokenEndpoint = "ast:TokenEndpoint";
            internal const string ast_UserInfoEndpoint = "ast:UserInfoEndpoint";
        }

        internal static class Config
        {
            private static string _ast_Authority;
            internal static string Authority
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(_ast_Authority))
                    {
                        _ast_Authority = ConfigurationManager.AppSettings[StTags.ast_Authority];
                    }
                    return _ast_Authority;
                }
            }

            private static string _ast_TokenEndpoint;
            internal static string TokenEndpoint
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(_ast_TokenEndpoint))
                    {
                        _ast_TokenEndpoint = ConfigurationManager.AppSettings[StTags.ast_TokenEndpoint];
                    }
                    return _ast_TokenEndpoint;
                }
            }

            private static string _ast_UserInfoEndpoint;
            internal static string UserInfoEndpoint
            {
                get
                {
                    if (string.IsNullOrWhiteSpace(_ast_UserInfoEndpoint))
                    {
                        _ast_UserInfoEndpoint = ConfigurationManager.AppSettings[StTags.ast_UserInfoEndpoint];
                    }
                    return _ast_UserInfoEndpoint;
                }
            }

            internal const string ClientId = "demoISkatana";
            internal const string ClientSecret = "secret";

        }

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
            //const string clientId = "demoISkatana";
            //const string clientSecret = "secret";
            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
            {
                Authority = Config.Authority, //"https://localhost:44311/identity",
                ClientId = Config.ClientId,
                ResponseType = "code id_token token",
                Scope = "openid email profile offline_access authTestAPI",
                ClientSecret = Config.ClientSecret,
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
                                    new Uri(Config.UserInfoEndpoint), //"https://localhost:44311/identity/connect/userinfo"),
                                    n.ProtocolMessage.AccessToken);

                                var userInfo = await userInfoClient.GetAsync();
                                userInfo.Claims.ToList().ForEach(ui => claims.Add(new Claim(ui.Item1, ui.Item2)));

                                // get access and refresh token
                                var tokenClient = new OAuth2Client(
                                    new Uri(Config.TokenEndpoint), //"https://localhost:44311/identity/connect/token"),
                                    Config.ClientId,
                                    Config.ClientSecret);

                                var response = await tokenClient.RequestAuthorizationCodeAsync(n.Code, n.RedirectUri);
                                //if (response.AccessToken != null)
                                //{
                                //    claims.Add(new Claim("access_token", response.AccessToken));
                                //    claims.Add(new Claim("expires_at", DateTime.Now.AddSeconds(response.ExpiresIn).ToLocalTime().ToString()));
                                //}
                                if (response.RefreshToken != null)
                                    claims.Add(new Claim(Constants.LocalClaimTypes.RefreshToken, response.RefreshToken));
                                if (n.ProtocolMessage.IdToken != null)
                                    claims.Add(new Claim(Constants.LocalClaimTypes.IdToken, n.ProtocolMessage.IdToken));

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
                            var token = n.OwinContext.Authentication.User.FindFirst(Constants.LocalClaimTypes.IdToken);
                            if (token != null)
                            {
                                var idTokenHint = token.Value;
                                n.ProtocolMessage.IdTokenHint = idTokenHint;
                            }
                        }
                    }
                }

            });
            try
            {
                //*** The type of Access Token (Reference or Jwt) must match the type specified in the startup of the Identity Server
                //*** The Reference token is smaller, but then requires the client to talk to the Id Server to get details
                //*** The Jwt is larger, and doesn't require the client app to talk to the Id Server, but then the Jwt can't be
                //*** invalidated until it expires.
#if false
                app.UseIdentityServerReferenceToken(new ReferenceTokenValidationOptions{
                    Authority = Config.Authority //"https://localhost:44311/identity"
                    });
#else
                app.UseIdentityServerJwt(new JwtTokenValidationOptions
                {
                    Authority = Config.Authority //"https://localhost:44311/identity"
                });
#endif
            }
            catch (Exception ex)
            {
                string foo = ex.Message;
                if (ex.InnerException != null)
                {
                    foo = ex.InnerException.Message;
                }
            }
        }
    }
}