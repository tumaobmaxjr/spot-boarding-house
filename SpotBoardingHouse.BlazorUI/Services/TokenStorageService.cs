using Microsoft.JSInterop;

namespace SpotBoardingHouse.BlazorUI.Services
{
    public class TokenStorageService
    {
        private const string TokenKey = "sbh_access_token";
        private readonly IJSRuntime _jsRuntime;
        public TokenStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }
        public ValueTask SetTokenAsync(string token)
            => _jsRuntime.InvokeVoidAsync("sbhStorage.setItem", TokenKey, token);
        public ValueTask<string?> GetTokenAsync()
            => _jsRuntime.InvokeAsync<string?>("sbhStorage.getItem", TokenKey);
        public ValueTask RemoveTokenAsync()
            => _jsRuntime.InvokeVoidAsync("sbhStorage.removeItem", TokenKey);
    }
}
