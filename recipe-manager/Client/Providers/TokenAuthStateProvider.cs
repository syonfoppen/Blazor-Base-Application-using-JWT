using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace recipe_manager.Client.Providers
{
    public class TokenAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _http;

        private IConfiguration Configuration { get; }

        public TokenAuthStateProvider(ILocalStorageService localStorage, HttpClient http)
        {
            this._localStorage = localStorage;
            this._http = http;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var savedToken = await _localStorage.GetItemAsync<string>("authToken");
            var anonymousState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));


            // Not authenticated
            if (string.IsNullOrWhiteSpace(savedToken))
            {
                _http.DefaultRequestHeaders.Authorization = null;
                NotifyAuthenticationStateChanged(Task.FromResult(anonymousState));
                return anonymousState;
            }

            var claims = ParseClaimsFromJwt(savedToken);
            // Checks the exp field of the token
            var expiry = claims.Where(claim => claim.Type.Equals("exp")).FirstOrDefault();
            if (expiry == null)
            {
                _http.DefaultRequestHeaders.Authorization = null;
                NotifyAuthenticationStateChanged(Task.FromResult(anonymousState));
                return anonymousState;
            }

            // The exp field is in Unix time
            var datetime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiry.Value));
            if (datetime.UtcDateTime <= DateTime.UtcNow)
            {
                _http.DefaultRequestHeaders.Authorization = null;
                await _localStorage.RemoveItemAsync("authToken");
                NotifyAuthenticationStateChanged(Task.FromResult(anonymousState));
                return anonymousState;
            }

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            NotifyAuthenticationStateChanged(Task.FromResult(state));

            return state;
        }

        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            return keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()));
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
