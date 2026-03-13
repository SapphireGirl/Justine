using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Justine.LambdaWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAmazonCognitoIdentityProvider _cognito;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthController> _logger;
        private readonly IWebHostEnvironment _env;

        public AuthController(
            IAmazonCognitoIdentityProvider cognito,
            IConfiguration config,
            ILogger<AuthController> logger,
            IWebHostEnvironment env)
        {
            _cognito = cognito;
            _config = config;
            _logger = logger;
            _env = env;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            try
            {
                if (req is null)
                    return BadRequest(new { message = "Request body is required" });

                if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                    return BadRequest(new { message = "Username and password are required" });

                var userPoolId = _config["Cognito:UserPoolId"] ?? throw new InvalidOperationException("Missing Cognito:UserPoolId configuration");
                var clientId = _config["Cognito:AppClientId"] ?? throw new InvalidOperationException("Missing Cognito:AppClientId configuration");

                var authParameters = new Dictionary<string, string>
                {
                    ["USERNAME"] = req.Username,
                    ["PASSWORD"] = req.Password
                };

                // If an App Client Secret is configured, compute the Cognito SECRET_HASH and add it.
                var clientSecret = _config["Cognito:AppClientSecret"];
                if (!string.IsNullOrEmpty(clientSecret))
                {
                    try
                    {
                        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(clientSecret));
                        var data = Encoding.UTF8.GetBytes(req.Username + clientId);
                        var hash = hmac.ComputeHash(data);
                        var secretHash = Convert.ToBase64String(hash);
                        authParameters["SECRET_HASH"] = secretHash;
                        _logger.LogDebug("Computed SECRET_HASH for user {Username}", req.Username);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to compute SECRET_HASH for user {Username}", req.Username);
                        if (_env.IsDevelopment())
                            return StatusCode(500, new { message = "Failed to compute SECRET_HASH", details = ex.ToString() });
                        return StatusCode(500, new { message = "Authentication configuration error" });
                    }
                }

                // Use USER_PASSWORD_AUTH via InitiateAuth when the App Client supports ALLOW_USER_PASSWORD_AUTH
                var initReq = new InitiateAuthRequest
                {
                    ClientId = clientId,
                    AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                    AuthParameters = authParameters
                };

                var resp = await _cognito.InitiateAuthAsync(initReq);
                var auth = resp.AuthenticationResult;
                if (auth == null) return BadRequest(new { message = "Authentication failed" });

                // Set access_token and refresh_token as HttpOnly cookies so the browser can send them automatically.
                // JwtBearer will be configured to read the access token from the cookie.
                if (!string.IsNullOrEmpty(auth.AccessToken))
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Path = "/",
                        Expires = DateTimeOffset.UtcNow.AddSeconds(auth.ExpiresIn ?? 3600)
                    };
                    Response.Cookies.Append("access_token", auth.AccessToken, cookieOptions);
                }

                if (!string.IsNullOrEmpty(auth.RefreshToken))
                {
                    var refreshOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.None,
                        Path = "/",
                        // refresh token lifetime typically longer; use a reasonable expiry or Cognito policy
                        Expires = DateTimeOffset.UtcNow.AddDays(30)
                    };
                    Response.Cookies.Append("refresh_token", auth.RefreshToken, refreshOptions);
                }

                // You may optionally return a small JSON payload (no tokens exposed here)
                return Ok(new { message = "ok" });
            }
            catch (Exception ex)
            {
                // Log server-side with full exception
                _logger.LogError(ex, "Login error for user {Username}", req?.Username);

                // In development, return the exception message and stack for debugging. Remove/limit this in production.
                if (_env.IsDevelopment())
                    return StatusCode(500, new { message = ex.Message, details = ex.ToString() });

                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // Dev-only endpoint to decode JWT claims for debugging.
        // Returns decoded header and payload when running in Development environment.
        [HttpGet("debug")]
        public IActionResult DebugToken()
        {
            if (!_env.IsDevelopment())
                return NotFound();

            // Prefer Authorization header, fallback to access_token cookie
            string token = null;
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }
            else if (Request.Cookies.TryGetValue("access_token", out var cookieToken))
            {
                token = cookieToken;
            }

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "No token provided in Authorization header or access_token cookie." });

            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2)
                    return BadRequest(new { message = "Token is not a JWT." });

                string DecodeBase64Url(string input)
                {
                    string s = input.Replace('-', '+').Replace('_', '/');
                    switch (s.Length % 4)
                    {
                        case 2: s += "=="; break;
                        case 3: s += "="; break;
                    }
                    var bytes = Convert.FromBase64String(s);
                    return Encoding.UTF8.GetString(bytes);
                }

                var headerJson = DecodeBase64Url(parts[0]);
                var payloadJson = DecodeBase64Url(parts[1]);

                using var headerDoc = JsonDocument.Parse(headerJson);
                using var payloadDoc = JsonDocument.Parse(payloadJson);

                return Ok(new
                {
                    header = headerDoc.RootElement,
                    payload = payloadDoc.RootElement
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decode token in debug endpoint");
                return BadRequest(new { message = "Failed to decode token.", error = ex.Message });
            }
        }

        public record LoginRequest(string Username, string Password);
    }
}