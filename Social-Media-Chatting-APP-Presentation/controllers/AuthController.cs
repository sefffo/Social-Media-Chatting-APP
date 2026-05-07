using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s;
using System.Security.Claims;

namespace Social_Media_Chatting_APP_Presentation.Controllers
{
    /// <summary>
    /// Handles all authentication operations for the Social Media App.
    /// Only endpoints whose service implementation is complete are exposed here.
    ///
    /// Inherits from ApiBaseController which provides HandleResult() —
    /// a unified method that maps Result/Result&lt;T&gt; to the correct HTTP status code.
    /// This keeps every action method clean and consistent.
    ///
    /// BASE ROUTE: /api/auth
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ApiBaseController
    {
        // ═══════════════════════════════════════════════════════════════════
        // REGISTER
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Registers a new user account.
        ///
        /// Creates an AppUser via ASP.NET Identity. The password is hashed
        /// automatically by Identity — we never store it in plain text.
        /// No token is returned here; the user must call /login after registering.
        ///
        /// REQUEST BODY:
        ///   - UserName    : unique handle used across the app
        ///   - Email       : unique email — this is the login identifier
        ///   - DisplayName : visible name shown in the UI (not required to be unique)
        ///   - Password    : plain text — Identity hashes it internally
        ///
        /// RESPONSES:
        ///   204 No Content      → account created successfully
        ///   400 Bad Request     → duplicate email
        ///   422 Unprocessable   → Identity validation failed (weak password, etc.)
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await authService.RegisterAsync(dto);
            return HandleResult(result);
        }


        // ═══════════════════════════════════════════════════════════════════
        // LOGIN
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Authenticates a user with email and password.
        ///
        /// Verifies credentials against the Identity store.
        /// On success, returns a JWT access token (short-lived) paired with a
        /// refresh token (7 days). The two tokens are linked via the jti claim —
        /// they must always be used as a pair.
        ///
        /// REQUEST BODY:
        ///   - Email    : the registered email address
        ///   - Password : the account password
        ///
        /// RESPONSES:
        ///   200 OK           → { accessToken, refreshToken }
        ///   401 Unauthorized → wrong email or password
        ///
        /// CLIENT STORAGE:
        ///   Keep accessToken in memory only (not localStorage — XSS risk).
        ///   Keep refreshToken in an HttpOnly cookie.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await authService.LoginAsync(dto);
            return HandleResult(result);
        }


        // ═══════════════════════════════════════════════════════════════════
        // REFRESH TOKEN
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Issues a brand-new token pair using an expired access token + a valid refresh token.
        ///
        /// The client calls this automatically when its access token expires,
        /// so the user never has to log in again while their session is active.
        ///
        /// What happens internally:
        ///   1. The expired JWT signature and claims are validated (expiry is ignored intentionally).
        ///   2. The refresh token is SHA-256 hashed and looked up in the database.
        ///   3. The jti claim ties the two tokens together — a mismatch = rejected.
        ///   4. The old refresh token row is stamped UsedAt + RevokedAt (single-use).
        ///   5. A new JWT + new refresh token are issued and persisted.
        ///
        /// REQUEST BODY:
        ///   - AccessToken  : the EXPIRED JWT string (not a new one)
        ///   - RefreshToken : the raw (unhashed) refresh token from the last login
        ///
        /// RESPONSES:
        ///   200 OK           → { accessToken, refreshToken } (brand new pair)
        ///   401 Unauthorized → tampered, expired, already-used, or mismatched tokens
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            var result = await authService.RefreshTokenAsync(dto);
            return HandleResult(result);
        }


        // ═══════════════════════════════════════════════════════════════════
        // LOGOUT
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Logs the user out by permanently revoking their current refresh token.
        ///
        /// The refresh token row in the database is stamped with RevokedAt = now,
        /// which makes IsActive = false. Any future refresh attempt with this token
        /// will be rejected. The row is kept (never deleted) for audit purposes.
        ///
        /// [Authorize] is required — only a user with a valid (non-expired) JWT
        /// can hit this endpoint. Send the JWT in: Authorization: Bearer &lt;token&gt;
        ///
        /// For full multi-device logout, call this once per active refresh token.
        ///
        /// REQUEST BODY:
        ///   - rawRefreshToken : the plain refresh token string the client has stored
        ///
        /// RESPONSES:
        ///   204 No Content   → session terminated successfully
        ///   400 Bad Request  → token not found or already revoked
        ///   401 Unauthorized → missing or invalid JWT in Authorization header
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] string rawRefreshToken)
        {
            var result = await authService.RevokeRefreshTokenAsync(rawRefreshToken);
            return HandleResult(result);
        }


        // ═══════════════════════════════════════════════════════════════════
        // GOOGLE OAUTH — STEP 1: REDIRECT
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Initiates the Google OAuth2 login flow by redirecting the user to Google's consent screen.
        ///
        /// This is a browser-redirect flow (not a JSON API call).
        /// The client navigates to this URL, Google handles authentication,
        /// then Google redirects back to /google-callback automatically.
        ///
        /// No request body needed — just a GET request from the browser.
        ///
        /// RESPONSES:
        ///   302 Redirect → browser is sent to Google's login page
        /// </summary>
        [HttpGet("google-login")]
        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            // Build the URL Google should redirect back to after the user consents.
            var redirectUri = Url.Action(nameof(GoogleCallback), "Auth", null, Request.Scheme);

            // AuthenticationProperties carries the redirect URI into the Google middleware.
            var properties = new AuthenticationProperties { RedirectUri = redirectUri };

            // Challenge() triggers the Google middleware to redirect the browser to Google.
            // GoogleDefaults.AuthenticationScheme = "Google"
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }


        // ═══════════════════════════════════════════════════════════════════
        // GOOGLE OAUTH — STEP 2: CALLBACK
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Handles Google's redirect back after the user consents on Google's login page.
        ///
        /// Google redirects here with an authorization code. ASP.NET's Google middleware
        /// exchanges it for an access token, reads the user's profile, and stores
        /// the claims in a temporary Cookie. This action then reads those claims.
        ///
        /// What the service does with (email, name, googleId):
        ///   - User exists by googleId        → issues tokens immediately
        ///   - Email exists as normal account  → returns 400 conflict error
        ///   - No user found                   → creates new account (JIT) → issues tokens
        ///
        /// This endpoint is called by Google's servers, not directly by your client.
        ///
        /// RESPONSES:
        ///   200 OK           → { accessToken, refreshToken }
        ///   400 Bad Request  → email already registered with password
        ///   401 Unauthorized → Google authentication failed
        /// </summary>
        [HttpGet("google-callback")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleCallback()
        {
            // Read the cookie that Google middleware populated after the OAuth exchange.
            var authResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authResult.Succeeded)
                return Unauthorized(new { message = "Google authentication failed." });

            // Extract the three pieces of identity from Google's claims.
            var email    = authResult.Principal!.FindFirstValue(ClaimTypes.Email)!;
            var name     = authResult.Principal!.FindFirstValue(ClaimTypes.Name)!;
            var googleId = authResult.Principal!.FindFirstValue(ClaimTypes.NameIdentifier)!; // "sub" claim

            var result = await authService.HandleGoogleLoginAsync(email, name, googleId);
            return HandleResult(result);
        }
    }
}
