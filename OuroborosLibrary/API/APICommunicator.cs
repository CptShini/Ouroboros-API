using System.Net;

namespace OuroborosLibrary.API
{
    public static class APICommunicator
    {
        private const string _baseUrl = "https://scoresaber.com/api/";
        private static readonly WebClient _webClient = new();

        /// <summary>
        /// Calls the ScoreSaber API at the given destination and returns the result.
        /// </summary>
        /// <param name="targetURL">The request url after the 'https://scoresaber.com/api/'. Example: player/76561198074878770/basic.</param>
        /// <returns>The API's response as a JSON string.</returns>
        public static string CallAPI(string targetURL)
        {
            string fullAPICallURL = _baseUrl + targetURL;
            string APIResponse = _webClient.DownloadString(fullAPICallURL);

            return APIResponse;
        }
    }
}