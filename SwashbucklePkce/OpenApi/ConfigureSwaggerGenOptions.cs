using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace SwashbucklePkce.OpenApi
{
    public class ConfigureSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly AppSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;

        public ConfigureSwaggerGenOptions(
            IOptions<AppSettings> settings,
            IHttpClientFactory httpClientFactory)
        {
            _settings = settings.Value;
            _httpClientFactory = httpClientFactory;
        }

        public void Configure(SwaggerGenOptions options)
        {
            var discoveryDocument = GetDiscoveryDocument();

            options.OperationFilter<AuthorizeOperationFilter>();
            options.DescribeAllParametersInCamelCase();
            options.CustomSchemaIds(x => x.FullName);
            options.SwaggerDoc("v1", CreateOpenApiInfo());

            options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,

                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(discoveryDocument.AuthorizeEndpoint),
                        TokenUrl = new Uri(discoveryDocument.TokenEndpoint),
                        Scopes = new Dictionary<string, string>
                        {
                            { _settings.Security.Jwt.Audience , "Balea Server HTTP Api" }
                        },
                    }
                },
                Description = "Balea Server OpenId Security Scheme"
            });
        }

        private DiscoveryDocumentResponse GetDiscoveryDocument()
        {
            return _httpClientFactory
                .CreateClient()
                .GetDiscoveryDocumentAsync(_settings.Security.Jwt.Authority)
                .GetAwaiter()
                .GetResult();
        }

        private OpenApiInfo CreateOpenApiInfo()
        {
            return new OpenApiInfo()
            {
                Title = "My Awesome API",
                Version = "v1",
                Description = "My Awesome API",
                Contact = new OpenApiContact() { Name = "API", Url = new Uri("https://github.com/lurumad") },
                License = new OpenApiLicense()
            };
        }
    }
}
