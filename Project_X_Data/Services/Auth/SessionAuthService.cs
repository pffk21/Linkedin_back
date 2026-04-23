using Project_X_Data.Data.Entities;
using System.Text.Json;

namespace Project_X_Data.Services.Auth
{
    public class SessionAuthService(
        IHttpContextAccessor httpContextAccessor) : IAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        const String sessionKey = "ISessionAuthService";

        public T? GetAuth<T>() where T : notnull
        {
            if (_httpContextAccessor.HttpContext?
                .Session.Keys.Contains(sessionKey) ?? false)
            {
                if (JsonSerializer.Deserialize<T>(
                        _httpContextAccessor.HttpContext!
                        .Session.GetString(sessionKey)!)
                    is T payload)
                {
                    return payload;
                }
            }
            return default;
        }

        public void RemoveAuth()
        {
            _httpContextAccessor.HttpContext?.Session.Remove(sessionKey);
        }

        public void SetAuth(object payload)
        {
            _httpContextAccessor.HttpContext?.Session.SetString(
                sessionKey,
                JsonSerializer.Serialize(payload)
            );
        }
    }
}
