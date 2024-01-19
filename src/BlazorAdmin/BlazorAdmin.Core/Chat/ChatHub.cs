using BlazorAdmin.Core.Helper;
using FluentCodeServer.Core;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BlazorAdmin.Core.Chat
{
    public class ChatHub : Hub<IChatClient>
    {
        private readonly JwtHelper _jwtHelper;

        public static readonly string ChatHubUrl = "/blazoradmin-chat";

        public static ConcurrentDictionary<int, string> OnlineUsers = new ConcurrentDictionary<int, string>();

        public ChatHub(JwtHelper jwtHelper)
        {
            _jwtHelper = jwtHelper;
        }

        public override async Task OnConnectedAsync()
        {
            var httpContextFeature = Context.Features.FirstOrDefault(f => f.Value is IHttpRequestFeature).Value as IHttpRequestFeature;
            var authToken = httpContextFeature?.Headers.Authorization.ToString().Split(" ").Last();
            var userPrincipal = _jwtHelper.ValidToken(authToken ?? string.Empty);

            if (userPrincipal == null)
            {
                throw new HubException("认证失败！拒绝连接！");
            }

            OnlineUsers.AddOrUpdate(userPrincipal.GetUserId(), Context.ConnectionId, (key, value) => Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var httpContextFeature = Context.Features.FirstOrDefault(f => f.Value is IHttpRequestFeature).Value as IHttpRequestFeature;
            var authToken = httpContextFeature?.Headers.Authorization.ToString().Split(" ").Last();
            var userPrincipal = _jwtHelper.ValidToken(authToken ?? string.Empty);
            OnlineUsers.Remove(userPrincipal.GetUserId(), out var connectionId);
            await base.OnDisconnectedAsync(exception);
        }

        public record OnlineUser
        {
            public string ConnectionId { get; set; } = null!;

            public int UserId { get; set; }
        }
    }
}
