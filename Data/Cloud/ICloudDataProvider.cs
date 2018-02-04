using System.Threading.Tasks;

namespace Progcube.Core.Data.Cloud
{
    public interface ICloudDataProvider
    {
        Task<string> GetFileUrl(string bucket, string path, string fileName);

        /// <summary>
        /// Removes the expired tokens from the token cache.
        /// </summary>
        void PurgeExpiredTokens();
    }
}