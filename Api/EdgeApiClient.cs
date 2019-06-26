using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Api
{
    public class EdgeApiClient
    {
        private const string AuthorizationHeaderName = "Authorization";
        private const string PreferHeaderName = "prefer";
        private const string GrantType = "client_credentials";
        private const string Scope = "all";        
        private readonly Uri _baseUri;
        private readonly string _clientId;
        private readonly string _clientSecret;

        private readonly Dictionary<string, string> _dictionary;

        private bool _initialized;
        private string _accessToken;

        private HttpClient _client;
        private string _requestBody;
        
        public EdgeApiClient(Uri baseUri, string clientId, string clientSecret)
        {
            _baseUri = baseUri;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _dictionary = new Dictionary<string, string>();
            BuildRequestParameters();
        }

        public async Task<string> GetStringAsync(Uri relativeUri, int? maxPageSize = null)
        {
            await InitializeAsync();

            var request = new HttpRequestMessage
            {
                RequestUri = relativeUri,
                Method = HttpMethod.Get
            };

            var headers = new NameValueCollection
            {
                {AuthorizationHeaderName, GetAccessTokenValue()}

            };

            if (maxPageSize.HasValue)
            {
                headers.Add(PreferHeaderName, $"odata.maxpagesize={maxPageSize}");
            }

            AddHeadersToRequest(headers, request);

            var response = await _client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return content;
            }

            throw new Exception($"Response by URI '{relativeUri}' is '{(int)response.StatusCode}' '{response.ReasonPhrase}' with content '{content}'");
        }

        public async Task<EdgeApiODataPayload> GetODataPayloadAsync(Uri uri, int? maxPageSize = null)
        {
            var stringContent = await GetStringAsync(uri, maxPageSize);

            var serializerSettings = new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.None
            };

            // Alternative of Regex and Json closing is implementation of incremental parser using f.e. JsonTextReader...
            EdgeApiErrorValue errorValue = null;
            var errorMatch = Regex.Match(stringContent, ",?({\"error\":{.*}})");
            if (errorMatch.Success)
            {
                errorValue = JsonConvert.DeserializeObject<EdgeApiErrorValue>(errorMatch.Groups[1].Value, serializerSettings);
                stringContent = stringContent.Remove(errorMatch.Index) + "]}";
            }

            var payload = JsonConvert.DeserializeObject<EdgeApiODataPayload>(stringContent, serializerSettings);
            payload.ErrorValue = errorValue;
            return payload;
        }

        private async Task InitializeAsync()
        {
            if (_initialized)
            {
                return;
            }

            _client = new HttpClient
            {
                BaseAddress = _baseUri,
                // Set timeout to infinite because Edge API service has its own timeout.
                Timeout = Timeout.InfiniteTimeSpan,
            };

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3
                                                   | SecurityProtocolType.Tls
                                                   | SecurityProtocolType.Tls11
                                                   | SecurityProtocolType.Tls12;
            var stsUri = new Uri($@"/services/api/oauth2/token", UriKind.Relative);

            BuildRequestBodyJSON(_dictionary);

            var request = new HttpRequestMessage
            {
                RequestUri = stsUri,
                Method = HttpMethod.Post,
                Content = new StringContent(_requestBody,Encoding.UTF8, "application/json")
            };

            var headers = new NameValueCollection
            {
                {"cache-control", "no-cache"}
            };

            AddHeadersToRequest(headers, request);

            var response = await _client.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                _accessToken = ((JObject)JsonConvert.DeserializeObject<dynamic>(content))["access_token"].ToString();
                _initialized = true;
                return;
            }

            throw new Exception($"Initialization failed with '{(int)response.StatusCode}' '{response.ReasonPhrase}' '{response.Content.ReadAsStringAsync().Result}'.");
        }

        private void AddHeadersToRequest(NameValueCollection headers, HttpRequestMessage request)
        {
            foreach (var key in headers.AllKeys)
            {
                request.Headers.Add(key, headers[key]);
            }
        }
        private void BuildRequestParameters()
        {
            try
            {
                _dictionary.Add("clientId", _clientId);
                _dictionary.Add("clientSecret", _clientSecret);
                _dictionary.Add("grantType", GrantType);
                _dictionary.Add("scope", Scope);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void BuildRequestBodyJSON(Dictionary<string, string> dictionary)
        {
            StringBuilder _sbParameters = new StringBuilder();
            char doublequotes = '"';
            int max = 0;
            _sbParameters.AppendLine("{");
            foreach (string param in dictionary.Keys)
            {
                _sbParameters.Append(doublequotes + param + doublequotes);//key => parameter name 
                _sbParameters.Append(':');
                _sbParameters.Append(doublequotes + dictionary[param] + doublequotes);//key value                
                if (max < dictionary.Keys.Count - 1)
                {
                    _sbParameters.Append(",");
                }
                _sbParameters.AppendLine("");
                max++;
            }
            _requestBody = _sbParameters.AppendLine("}").ToString();
        }
        private string GetAccessTokenValue()
        {
            return "Bearer " + _accessToken;
        }

    }
}
