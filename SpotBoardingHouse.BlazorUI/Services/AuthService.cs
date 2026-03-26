using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Authorization;
using SpotBoardingHouse.BlazorUI.Models.Auth;

namespace SpotBoardingHouse.BlazorUI.Services
{
    public class AuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenStorageService _tokenStorage;
        private readonly AuthStateProvider _authStateProvider;
        public AuthService(
            IHttpClientFactory httpClientFactory,
            TokenStorageService tokenStorage,
            AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClientFactory = httpClientFactory;
            _tokenStorage = tokenStorage;
            _authStateProvider = (AuthStateProvider)authenticationStateProvider;
        }

        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            var client = _httpClientFactory.CreateClient("Api.Public");
            var response = await client.PostAsJsonAsync("api/v1/Auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                return LoginResult.Fail("Invalid email or password.");
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (result == null || string.IsNullOrWhiteSpace(result.AccessToken))
            {
                return LoginResult.Fail("Invalid login response.");
            }

            await _tokenStorage.SetTokenAsync(result.AccessToken);
            _authStateProvider.NotifyUserAuthentication(result.AccessToken);

            return LoginResult.Success();
        }

        public async Task LogoutAsync()
        {
            await _tokenStorage.RemoveTokenAsync();
            _authStateProvider.NotifyUserLogout();
        }
    }
}
