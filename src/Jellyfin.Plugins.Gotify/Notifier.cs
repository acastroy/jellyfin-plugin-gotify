using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugins.Gotify.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Notifications;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugins.Gotify
{
    public class Notifier : INotificationService
    {
        private readonly IHttpClient _httpClient;

        public Notifier(IHttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        
        public async Task SendNotification(UserNotification request, CancellationToken cancellationToken)
        {
            var options = GetOptions(request.User);

            var body = new Dictionary<string, string>();
            
            // message parameter is required. If it is sent as null 
            // put name as message

            if (string.IsNullOrEmpty(request.Description))
            {
                body.Add("message", request.Name);
            }
            else
            {
                if (!string.IsNullOrEmpty(request.Name))
                    body.Add("title", request.Name);
                body.Add("message", request.Description);
            }
            
            var requestOptions = new HttpRequestOptions
            {
                Url = options.Url.TrimEnd('/') + $"/message?token={options.Token}"
            };

            requestOptions.SetPostData(body);

            await _httpClient.Post(requestOptions).ConfigureAwait(false);
        }

        public bool IsEnabledForUser(User user)
        {
            var options = GetOptions(user);
            return options != null && IsValid(options) && options.Enabled;
        }
        
        public string Name => Plugin.Instance.Name;

        private static GotifyOptions GetOptions(BaseItem user)
        {
            return Plugin.Instance.Configuration.Options
                .FirstOrDefault(u => string.Equals(u.UserId, user.Id.ToString("N"),
                    StringComparison.OrdinalIgnoreCase));
        }
        
        private static bool IsValid(GotifyOptions options)
        {
            return !string.IsNullOrEmpty(options.Token)
                   && !string.IsNullOrEmpty(options.Url);
        }
    }
}