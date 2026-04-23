using Project_X_Data.Data.Entities;
using Project_X_Data.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text.Json;

namespace Project_X_Data.Middleware.Auth
{
    public class JwtAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            string authHeader = context.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                try
                {
                    string jwt = authHeader["Bearer ".Length..];
                    string[] parts = jwt.Split('.');

                    if (parts.Length == 3)
                    {
                        string tokenBody = parts[0] + "." + parts[1];
                        string secret = configuration["Jwt:Secret"]
                            ?? throw new KeyNotFoundException("Jwt:Secret is missing in configuration");

                        // Вычисляем подпись
                        string calculatedSignature = Base64UrlTextEncoder.Encode(
                            System.Security.Cryptography.HMACSHA256.HashData(
                                System.Text.Encoding.UTF8.GetBytes(secret),
                                System.Text.Encoding.UTF8.GetBytes(tokenBody)
                            ));

                        if (calculatedSignature == parts[2])
                        {
                            string payloadJson = System.Text.Encoding.UTF8.GetString(
                                Base64UrlTextEncoder.Decode(parts[1]));

                            using var doc = JsonDocument.Parse(payloadJson);
                            var data = doc.RootElement;

                            string getClaim(string prop) =>
                                data.TryGetProperty(prop, out var val) ? val.GetString() ?? "" : "";

                            var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Sid, getClaim("sub")),
                        new Claim(ClaimTypes.Name, getClaim("name")),
                        new Claim(ClaimTypes.Role, getClaim("role")),
                        new Claim(ClaimTypes.Email, getClaim("email"))
                    };

                            if (!string.IsNullOrEmpty(getClaim("sub")) || !string.IsNullOrEmpty(getClaim("email")))
                            {
                                context.User = new ClaimsPrincipal(
                                    new ClaimsIdentity(claims, nameof(JwtAuthMiddleware))
                                );
                            }
                        }
                        else
                        {
                            context.Response.Headers.Append("Authentication-Control", "Invalid JWT signature");
                        }
                    }
                    else
                    {
                        context.Response.Headers.Append("Authentication-Control", "Invalid JWT structure");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"JWT Middleware Error: {ex.Message}");
                    context.Response.Headers.Append("Authentication-Control", "Error parsing token");
                }
            }

            await _next(context);
        }
        //public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        //{
        //    String authHeader = context.Request.Headers.Authorization.ToString();
        //    if (!String.IsNullOrEmpty(authHeader))
        //    {
        //        String scheme = "Bearer ";
        //        if (authHeader.StartsWith(scheme))
        //        {
        //            String? errorMessage = null;
        //            String jwt = authHeader[scheme.Length..];
        //            String[] parts = jwt.Split('.');
        //            if (parts.Length == 3)
        //            {

        //                String tokenBody = parts[0] + '.' + parts[1];
        //                String secret = configuration.GetSection("Jwt").GetSection("Secret").Value
        //                ?? throw new KeyNotFoundException("Not found configuration 'Jwt.Secret'");
        //                String signature = Base64UrlTextEncoder.Encode(
        //                    System.Security.Cryptography.HMACSHA256.HashData(
        //                        System.Text.Encoding.UTF8.GetBytes(secret),
        //                        System.Text.Encoding.UTF8.GetBytes(tokenBody)
        //                ));
        //                if (signature == parts[2])
        //                {
        //                    String payload = System.Text.Encoding.UTF8.GetString(
        //                        Base64UrlTextEncoder.Decode(parts[1]));
        //                    var data = JsonSerializer.Deserialize<JsonElement>(payload)!;
        //                    context.User = new ClaimsPrincipal(
        //                        new ClaimsIdentity(
        //                            [
        //                                new Claim(ClaimTypes.Sid,  data.GetString("sub")! ),
        //                                new Claim(ClaimTypes.Name, data.GetString("name")!),
        //                                new Claim(ClaimTypes.Role, data.GetString("role")! ),
        //                                new Claim(ClaimTypes.Email, data.TryGetProperty("email", out var emailProp) ? emailProp.GetString()! : "")
        //                            ],
        //                            nameof(JwtAuthMiddleware)
        //                        )
        //                    );
        //                }
        //                else
        //                {
        //                    errorMessage = "Invalid JWT signature";
        //                }
        //            }
        //            else
        //            {
        //                errorMessage = "Invalid JWT structure";
        //            }
        //            if (errorMessage != null)
        //            {
        //                context.Response.Headers.Append(
        //                    "Authentication-Control",
        //                    errorMessage
        //                );
        //            }
        //        }
        //    }

        //    await _next(context);
        //}
    }


    public static class JwtAuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseJwtAuth(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JwtAuthMiddleware>();
        }
    }

}
/* Д.З. Включити у склад авторизаційних даних відомості про E-mail
 * користувача. Додати до навігаційної панелі Layout кнопку-посилання
 * "надіслати лист" з переходом на "mailto:..."
 */
/* Д.З. Реалізувати авторизацію засобами JWT у власному 
 * курсовому проєкті. Прикласти посилання на репозиторії
 * проєктів (бек, фронт)
 */
