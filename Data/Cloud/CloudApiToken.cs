using System;
using System.Collections.Concurrent;

namespace Progcube.Core.Data.Cloud
{
    public class CloudApiToken
    {
        public string Token { get; set; }

        /// <summary>
        /// The time at which the token is no longer valid.
        /// </summary>
        public DateTime ExpirationDateTime { get; set; }
    }
}