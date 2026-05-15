using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Social_Media_Chatting_APP_ServiceAbstraction;
using Social_Media_Chatting_APP_SharedLibrary.Dto_s;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;
using Social_Media_Chatting_APP_Domain.Entities;

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
    public class AuthController(IAuthService authService,IOtpService otpService) : ApiBaseController
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
        [EnableRateLimiting("GenerousAuth")]
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
        [EnableRateLimiting("StrictAuth")]
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
        ///   200 OK           → { accessToken, refreshToken } (brand-new pair)
        ///   401 Unauthorized → tampered, expired, already-used, or mismatched tokens
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("GenerousAuth")]
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
        
        // ═══════════════════════════════════════════════════════════════════
        // VERIFY OTP
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Verifies the 6-digit OTP code submitted by the user.
        ///
        /// Used for TWO flows — the purpose is auto-detected from user state:
        ///   1. EmailVerification: called after registration (EmailConfirmed=false)
        ///   2. TwoFactorLogin: called after login when IsTwoFactorSetup=true
        ///
        /// On success → issues JWT + RefreshToken pair (user is fully authenticated)
        /// On failure → returns specific error with attempts remaining
        ///
        /// REQUEST BODY:
        ///   - UserId : returned from /register or /login (RequiresTwoFactor=true)
        ///   - Code   : 6-digit code from the email
        ///
        /// RESPONSES:
        ///   200 OK           → { accessToken, refreshToken }
        ///   400 Bad Request  → wrong code / too many attempts / no active code
        ///   404 Not Found    → userId doesn't exist
        /// </summary>


        [HttpPost("verify-otp")]
        [AllowAnonymous]
        [EnableRateLimiting("StrictAuth")]
        public async Task<IActionResult> VerifyOTP([FromBody]  VerifyOtpDto dto)
        {
            var result = await authService.VerifyOtpAsync(dto);
            return HandleResult(result);
        }
        
        
        
        private async Task<OtpPurpose> ResolvePurposeAsync(string userId)
        {
            var emailConfirmed = await authService.IsEmailConfirmedAsync(userId);
            return emailConfirmed
                ? OtpPurpose.TwoFactorLogin
                : OtpPurpose.EmailVerification;
        }
        
        // ═══════════════════════════════════════════════════════════════════
        // RESEND OTP
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Generates and sends a fresh OTP, invalidating any existing one.
        ///
        /// The purpose is inferred from user state — same logic as verify-otp.
        /// Call this when the user didn't receive the code or it expired.
        ///
        /// WHY IOtpService directly (not IAuthService):
        ///   Resending a code is pure OTP logic — no JWT, no session, no Identity.
        ///   AuthService doesn't own this operation.
        ///
        /// REQUEST BODY:
        ///   - UserId : the user to resend the code to
        ///
        /// RESPONSES:
        ///   200 OK        → "Verification code sent successfully."
        ///   404 Not Found → userId doesn't exist
        /// </summary>
        [HttpPost("resend-otp")]
        [AllowAnonymous]
        [EnableRateLimiting("GenerousAuth")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpDto dto)
        {
            // Determine purpose from user state — same rule as VerifyOtpAsync
            // WHY here: OtpService.ResendOtpAsync takes a purpose parameter,
            // but the controller doesn't know the user's state — it only has userId.
            // We need to look up the user to decide which Redis key to overwrite.
            // This is the one place where the controller does a tiny bit of routing logic.
            var purpose = await ResolvePurposeAsync(dto.UserId);
            var result = await otpService.ResendOtpAsync(dto.UserId, purpose);
            return HandleResult(result);
        }
        
        // ═══════════════════════════════════════════════════════════════════
        // FORGOT PASSWORD
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Initiates the password reset flow by sending a reset link to the email.
        ///
        /// SECURITY: Always returns the same success message regardless of whether
        /// the email is registered — prevents email enumeration attacks.
        /// An attacker cannot use this endpoint to discover which emails exist.
        ///
        /// REQUEST BODY:
        ///   - email : the email address to send the reset link to
        ///
        /// RESPONSES:
        ///   200 OK → "If this email is registered, a reset link has been sent."
        ///            (always 200 — never reveals email existence)
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting("GenerousAuth")]
        public async Task<IActionResult> ForgotPassword([FromBody]string email)
        {
            var result = await authService.ForgotPasswordAsync(email);
            return HandleResult(result);
        }
        
        // ═══════════════════════════════════════════════════════════════════
        // RESET PASSWORD
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Completes the password reset flow using the token from the email link.
        ///
        /// The token is a 64-byte cryptographically secure random string
        /// embedded in the reset URL: /reset-password?token=xxx
        /// The frontend extracts it from the URL and sends it here.
        ///
        /// Three-gate validation: token exists → not used → not expired
        /// After success the token is marked IsUsed=true (single-use).
        ///
        /// REQUEST BODY:
        ///   - Token       : the raw token from the reset URL
        ///   - NewPassword : the replacement password (Identity rules apply)
        ///
        /// RESPONSES:
        ///   200 OK        → "Password reset successful"
        ///   400 Bad Request → token already used or expired
        ///   404 Not Found   → token doesn't exist / user not found
        ///   422 Unprocessable → weak password (Identity validation)
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        [EnableRateLimiting("StrictAuth")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await authService.ResetPasswordAsync(dto);
            return HandleResult(result);
        }
        // ═══════════════════════════════════════════════════════════════════
        // 2FA — ENABLE
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Enables two-factor authentication for the authenticated user.
        ///
        /// [Authorize] required — only the account owner can enable their own 2FA.
        /// userId is extracted from JWT claims — never trusted from the request body.
        ///
        /// After enabling: every future login will require an OTP code before
        /// the JWT is issued. A security email is sent to alert the user.
        ///
        /// RESPONSES:
        ///   200 OK        → "Two-factor authentication enabled successfully."
        ///   400 Bad Request → already enabled
        ///   401 Unauthorized → no valid JWT
        ///   404 Not Found → user not found (shouldn't happen with valid JWT)
        /// </summary>
        [HttpPost("2fa/enable")]
        [Authorize]
        [EnableRateLimiting("GenerousAuth")]
        public async Task<IActionResult> EnableTwoFactor()
        {
            // Extract userId from JWT claims — never accept userId from the body
            // WHY: If we trusted the body, any authenticated user could enable
            // 2FA on someone else's account by passing a different userId
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await authService.EnableTwoFactorAsync(userId);
            return HandleResult(result);
        }
        // ═══════════════════════════════════════════════════════════════════
        // 2FA — DISABLE
        // ═══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Disables two-factor authentication for the authenticated user.
        ///
        /// [Authorize] required — only the account owner can disable their own 2FA.
        /// userId extracted from JWT claims — same security reasoning as enable.
        ///
        /// After disabling: login will return JWT directly without OTP step.
        ///
        /// RESPONSES:
        ///   200 OK        → "Two-factor authentication disabled successfully."
        ///   400 Bad Request → not enabled
        ///   401 Unauthorized → no valid JWT
        ///   404 Not Found → user not found
        /// </summary>
        [HttpPost("2fa/disable")]
        [Authorize]
        [EnableRateLimiting("GenerousAuth")]
        public async Task<IActionResult> DisableTwoFactor()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await authService.DisableTwoFactorAsync(userId);
            return HandleResult(result);
        }

        
        
    }
}
