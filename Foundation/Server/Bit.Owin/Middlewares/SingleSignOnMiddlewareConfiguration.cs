﻿using System;
using System.Net.Http;
using Bit.Core.Contracts;
using Bit.Core.Models;
using Bit.Owin.Contracts;
using IdentityServer3.AccessTokenValidation;
using IdentityServer3.Core.Models;
using Owin;

namespace Bit.Owin.Middlewares
{
    public class SingleSignOnMiddlewareConfiguration : IOwinMiddlewareConfiguration
    {
        private readonly IAppEnvironmentProvider _appEnvironmentProvider;
        private readonly ICertificateProvider _certificateProvider;

        protected SingleSignOnMiddlewareConfiguration()
        {
        }

        public SingleSignOnMiddlewareConfiguration(IAppEnvironmentProvider appEnvironmentProvider,
            ICertificateProvider certificateProvider)
        {
            if (appEnvironmentProvider == null)
                throw new ArgumentNullException(nameof(appEnvironmentProvider));

            if (certificateProvider == null)
                throw new ArgumentNullException(nameof(certificateProvider));

            _appEnvironmentProvider = appEnvironmentProvider;
            _certificateProvider = certificateProvider;
        }

        public virtual void Configure(IAppBuilder owinApp)
        {
            if (owinApp == null)
                throw new ArgumentNullException(nameof(owinApp));

            AppEnvironment activeAppEnvironment = _appEnvironmentProvider.GetActiveAppEnvironment();

            IdentityServerBearerTokenAuthenticationOptions authOptions = new IdentityServerBearerTokenAuthenticationOptions
            {
                ClientId = activeAppEnvironment.Security.ClientName,
                Authority = activeAppEnvironment.Security.SSOServerUrl,
                DelayLoadMetadata = true,
                RequiredScopes = activeAppEnvironment.Security.Scopes,
                ClientSecret = activeAppEnvironment.Security.ClientSecret.Sha512(),
                EnableValidationResultCache = true,
                ValidationResultCacheDuration = TimeSpan.FromMinutes(15),
                // ValidationMode = ValidationMode.ValidationEndpoint,
                ValidationMode = ValidationMode.Local,
                PreserveAccessToken = true,
                SigningCertificate = _certificateProvider.GetSingleSignOnCertificate(),
                BackchannelHttpHandler = GetHttpClientHandler(nameof(IdentityServerBearerTokenAuthenticationOptions.BackchannelHttpHandler)),
                IntrospectionHttpHandler = GetHttpClientHandler(nameof(IdentityServerBearerTokenAuthenticationOptions.IntrospectionHttpHandler)),
                IssuerName = activeAppEnvironment.Security.SSOServerUrl
            };

            owinApp.UseIdentityServerBearerTokenAuthentication(authOptions);
        }

        protected virtual HttpMessageHandler GetHttpClientHandler(string usage)
        {
            return new HttpClientHandler();
        }
    }
}