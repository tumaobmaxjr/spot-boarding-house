namespace SpotBoardingHouse.BlazorUI.Models.Auth
{
    public class LoginResult
    {
        public bool Succeeded { get; set; }
        public string? Error { get; set; }
        public static LoginResult Success() => new() { Succeeded = true };
        public static LoginResult Fail(string error) => new() { Succeeded = false, Error = error };
    }
}
