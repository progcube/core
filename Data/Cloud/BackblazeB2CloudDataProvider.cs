using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Progcube.Core.Data.Cloud
{
    public class BackblazeB2CloudDataProvider : ICloudDataProvider
    {
        /// <summary>
        /// The secret used to request an authorization token.
        /// </summary>
        private readonly string _authKey;

        /// <summary>
        /// The id of the bucket to use.
        /// </summary>
        private readonly string _bucketId;

        /// <summary>
        /// The authorization token to use to perform authenticated calls to the B2 API.
        /// </summary>
        /// <remarks>
        /// Get the token by using the b2_authorize_account API call.
        /// Valid for at most 24 hours, see AuthorizationTokenExpirationDateTime property.
        /// </remarks>
        private string AccountAuthorizationToken { get; set; }

        /// <summary>
        /// The base URL to use for downloading files.
        /// </summary>
        private string BaseUrl { get; set; }

        /// <summary>
        /// The base URL to use for API calls.
        /// </summary>
        private string ApiUrl { get; set; }

        public BackblazeB2CloudDataProvider(string authKey, string bucketId)
        {
            _authKey = authKey;
            _bucketId = bucketId;
        }

        /// <summary>
        /// The concurrent dictionary used to store tokens for bucket scopes.
        /// Key: bucket and file prefix to which the token grants access.
        /// </summary>
        private ConcurrentDictionary<string, CloudApiToken> _tokens = new ConcurrentDictionary<string, CloudApiToken>();

        public async Task<string> GetFileUrl(string bucket, string path, string fileName)
        {

            // TODO: call AuthorizeAccount() every start and every time we get a 401 (also have a retry count not to get stuck in a loop)

            if (!_tokens.ContainsKey(path))
            {
                // Get a token for this file path
                await AuthorizeAccount();
                await GetTokenForBucket(path);
            }

            CloudApiToken tokenInfo;
            var hasToken = _tokens.TryGetValue(path, out tokenInfo);
            if (!hasToken)
            {
                throw new InvalidOperationException("Error when trying to get token from concurrent dictionary. Dictionary should contain key, but key was not found.");
            }

            if (tokenInfo.ExpirationDateTime <= DateTime.UtcNow)
            {
                // Token expired, get new token

            }

            return $"{BaseUrl}/file/{bucket}/{path}/{fileName}?Authorization={tokenInfo.Token}";
        }

        public void PurgeExpiredTokens()
        {
            foreach (var entry in _tokens)
            {
                if (entry.Value.ExpirationDateTime > DateTime.UtcNow)
                    continue;

                CloudApiToken removedValue;
                bool removed = _tokens.TryRemove(entry.Key, out removedValue);
            }
        }

        private async Task AuthorizeAccount()
        {
            // b2_authorize_account: Used to log in to the B2 API. Returns an authorization token that can be used for account-level operations, and a URL that should be used as the base URL for subsequent API calls.

            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(_authKey));
            var webRequest = WebRequest.Create("https://api.backblazeb2.com/b2api/v1/b2_authorize_account");
            webRequest.Headers.Add("Authorization", $"Basic {credentials}");
            webRequest.ContentType = "application/json; charset=utf-8";
            
            var response = await webRequest.GetResponseAsync();

            string json;
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                json = await sr.ReadToEndAsync();
            }
            
            dynamic d = JObject.Parse(json);
            ApiUrl = d.apiUrl;
            BaseUrl = d.downloadUrl;
            AccountAuthorizationToken = d.authorizationToken;
        }

        private async Task GetTokenForBucket(string path)
        {
            // b2_get_download_authorization: Used to generate an authorization token that can be used to download files with the specified prefix (and other optional headers) from a private B2 bucket.

            int validDuration = 180; // The number of seconds the authorization is valid for

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create($"{ApiUrl}/b2api/v1/b2_get_download_authorization");
            var body = "{\"bucketId\":\"" + _bucketId + "\", \"fileNamePrefix\":\"" + path + "\", \"validDurationInSeconds\":" + validDuration + "}";
            var data = Encoding.UTF8.GetBytes(body);
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", AccountAuthorizationToken);
            webRequest.ContentType = "application/json; charset=utf-8";
            webRequest.ContentLength = data.Length;
            using (var stream = webRequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
                stream.Close();
            }

            // Time the request to deduct it from the token expiration date
            Stopwatch sw = Stopwatch.StartNew();
            WebResponse response = null;
            try
            {
                response = await webRequest.GetResponseAsync();
            }
            catch (WebException ex)
            {
                using (var stream = ex.Response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine(reader.ReadToEnd());
                }
            }
            sw.Stop();

            string json;
            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                json = await sr.ReadToEndAsync();
            }
            
            dynamic d = JObject.Parse(json);
            var tokenInfo = new CloudApiToken
            {
                Token = d.authorizationToken,
                ExpirationDateTime = DateTime.UtcNow.AddSeconds(validDuration - sw.Elapsed.TotalSeconds)
            };

            _tokens.AddOrUpdate(path, tokenInfo, (key, oldValue) => tokenInfo);
        }
    }
}