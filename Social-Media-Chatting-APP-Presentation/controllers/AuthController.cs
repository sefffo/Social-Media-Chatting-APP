using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s;

namespace Social_Media_Chatting_APP_Presentation.Controllers
{
    /// <summary>
    /// Handles all authentication operations:
    /// registration, login, token refresh, logout, and Google OAuth login.
    ///
    /// BASE ROUTE: /api/auth
    ///
    /// HOW THE CONTROLLER LAYER WORKS:
    ///   The controller is deliberately thin — it does NO business logic.
    ///   Its only responsibilities are:
    ///     1. Receive the HTTP request and deserialize the body into a DTO.
    ///     2. Call the appropriate service method.
    ///     3. Map the service Result to an HTTP response (200, 400, 401, etc.).
    ///
    ///   All real logic (token generation, hashing, DB writes) lives in AuthService.
    ///   This separation makes the service independently testable without HTTP.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        // ═══════════════════════════════════════════════════════════════════════
        // REGISTRATION
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Registers a new user account.
        ///
        /// WHAT IT DOES:
        ///   Creates a new AppUser via ASP.NET Identity.
        ///   Password is hashed internally by Identity — we never store it plain.
        ///   No token is issued on registration — the user must call /login after.
        ///
        /// REQUEST BODY: RegisterDto
        ///   - UserName    : unique display handle
        ///   - Email       : unique email — used as the login identifier
        ///   - DisplayName : shown in the UI (can be non-unique)
        ///   - Password    : plain text — Identity hashes it automatically
        ///
        /// RESPONSES:
        ///   201 Created         → account created successfully
        ///   400 Bad Request     → duplicate email, or Identity validation failed
        ///                         (weak password, invalid email format, etc.)
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await authService.RegisterAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(result.Errors);

            // 201 Created — standard for successful resource creation.
            // No body needed — user should now call /login.
            return StatusCode(StatusCodes.Status201Created);
        }


        // ═══════════════════════════════════════════════════════════════════════
        // LOGIN
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Authenticates a user with email and password.
        ///
        /// WHAT IT DOES:
        ///   Verifies credentials against the Identity store.
        ///   On success, issues a JWT access token (short-lived) and a
        ///   refresh token (long-lived, 7 days) as a pair.
        ///
        ///   The two tokens are linked by the jti claim — they must always
        ///   be used together. See AuthService for the full login flow.
        ///
        /// REQUEST BODY: LoginDto
        ///   - Email    : registered email
        ///   - Password : account password
        ///
        /// RESPONSES:
        ///   200 OK              → { accessToken, refreshToken }
        ///   401 Unauthorized    → wrong email or password
        ///
        /// CLIENT STORAGE ADVICE:
        ///   Store accessToken  in memory (JS variable) — never in localStorage.
        ///   Store refreshToken in an HttpOnly cookie — prevents XSS theft.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await authService.LoginAsync(dto);

            if (!result.IsSuccess)
                return Unauthorized(result.Errors);

            return Ok(result.Data);
        }


        // ═══════════════════════════════════════════════════════════════════════
        // TOKEN REFRESH
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Issues a new token pair using an expired access token + valid refresh token.
        ///
        /// WHAT IT DOES:
        ///   Called automatically by the client when the access token expires.
        ///   Validates the expired JWT (signature only — expiry is intentionally ignored).
        ///   Looks up the refresh token by its SHA-256 hash in the database.
        ///   Verifies the jti link between the two tokens.
        ///   Rotates: marks old refresh token as used + revoked, issues a brand new pair.
        ///
        /// TOKEN ROTATION:
        ///   Each refresh token is single-use. After this call, the old tokens
        ///   are dead. The client must replace both stored tokens with the new ones.
        ///
        /// REQUEST BODY: RefreshTokenDto
        ///   - AccessToken  : the EXPIRED JWT string
        ///   - RefreshToken : the raw (unhashed) refresh token from the last login/refresh
        ///
        /// RESPONSES:
        ///   200 OK              → { accessToken, refreshToken } (brand new pair)
        ///   401 Unauthorized    → invalid/tampered/expired/already-used tokens
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto)
        {
            var result = await authService.RefreshTokenAsync(dto);

            if (!result.IsSuccess)
                return Unauthorized(result.Errors);

            return Ok(result.Data);
        }


        // ═══════════════════════════════════════════════════════════════════════
        // LOGOUT / REVOKE
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Logs the user out by permanently revoking their refresh token.
        ///
        /// WHAT IT DOES:
        ///   Hashes the incoming raw refresh token, finds the DB row, and
        ///   sets RevokedAt = now. This makes IsActive = false permanently,
        ///   blocking any future refresh attempts with this token.
        ///
        ///   The DB row is NEVER deleted — it is kept as an audit record.
        ///   For multi-device logout, call this for each active refresh token.
        ///
        /// WHY [Authorize] HERE:
        ///   Only authenticated users can logout. The JWT must be valid
        ///   (not expired) to reach this endpoint.
        ///
        /// REQUEST BODY: string
        ///   - rawRefreshToken : the plain refresh token stored by the client
        ///
        /// RESPONSES:
        ///   200 OK              → token revoked, session terminated
        ///   400 Bad Request     → token not found or already revoked
        ///   401 Unauthorized    → no valid JWT in Authorization header
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] string rawRefreshToken)
        {
            var result = await authService.RevokeRefreshTokenAsync(rawRefreshToken);

            if (!result.IsSuccess)
                return BadRequest(result.Errors);

            return Ok("Logged out successfully.");
        }


        // ═══════════════════════════════════════════════════════════════════════
        // GOOGLE OAUTH LOGIN
        // ═══════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Authenticates or registers a user via Google OAuth.
        ///
        /// WHAT IT DOES:
        ///   The frontend receives a Google idToken after the user completes
        ///   Google's own login flow. That raw idToken is sent here.
        ///
        ///   The backend validates the idToken via Google's public keys
        ///   (GoogleJsonWebSignature.ValidateAsync) — this happens in the
        ///   controller before calling the service.
        ///
        ///   Then passes verified (email, name, googleId) to the service which:
        ///     - Finds existing user by googleId → issues tokens immediately
        ///     - Finds email exists as normal account → returns 400 conflict
        ///     - No user found → creates new account (JIT provisioning) → issues tokens
        ///
        /// NO PASSWORD, NO 2FA:
        ///   Google already verified the user's identity. No OTP flow needed.
        ///   A random unguessable internal password is set — the user never knows it.
        ///
        /// REQUEST BODY: GoogleLoginDto
        ///   - IdToken : the raw Google JWT returned by the Google Sign-In SDK
        ///
        /// RESPONSES:
        ///   200 OK              → { accessToken, refreshToken }
        ///   400 Bad Request     → email conflict (registered with password)
        ///   401 Unauthorized    → invalid or expired Google idToken
        /// </summary>
        [HttpPost("google-login")]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
        {
            // Validate Google's idToken and extract claims.
            // Done in the controller — not the service — because it is
            // an infrastructure concern (HTTP call to Google), not business logic.
            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);
            }
            catch
            {
                return Unauthorized("Invalid Google token.");
            }

            var result = await authService.HandleGoogleLoginAsync(
                email:    payload.Email,
                name:     payload.Name,
                googleId: payload.Subject  // "sub" claim — permanent Google user ID
            );

            if (!result.IsSuccess)
                return BadRequest(result.Errors);

            return Ok(result.Data);
        }
    }
}
