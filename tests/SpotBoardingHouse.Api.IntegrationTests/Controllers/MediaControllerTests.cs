using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SpotBoardingHouse.Api.IntegrationTests.TestInfrastructure;
using SpotBoardingHouse.Application.DTOs.Auth;
using SpotBoardingHouse.Application.DTOs.BoardingHouses;
using SpotBoardingHouse.Application.DTOs.Media;

namespace SpotBoardingHouse.Api.IntegrationTests.Controllers
{
    public class MediaControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private const string Endpoint = "/api/v1/media";
        private readonly CustomWebApplicationFactory _factory;

        public MediaControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetById_ShouldReturn401_WhenNoTestAuthHeaders()
        {
            using var client = _factory.CreateClient();

            var response = await client.GetAsync($"{Endpoint}/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Owner_ShouldAccessOwnMedia()
        {
            var owner = await RegisterOwnerAsync(_factory.CreateClient(), "owner-self-media");
            using var ownerClient = CreateAuthorizedClient(owner.UserId, "Owner");
            var boardingHouseId = await CreateBoardingHouseAsync(ownerClient, "Owner Self Media BH");
            var mediaId = await CreateMediaAsync(ownerClient, boardingHouseId, "https://example.com/self.jpg");

            var response = await ownerClient.GetAsync($"{Endpoint}/{mediaId}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Owner_ShouldReturn403_WhenAccessingAnotherOwnersMedia()
        {
            var ownerA = await RegisterOwnerAsync(_factory.CreateClient(), "owner-a-media");
            using var ownerAClient = CreateAuthorizedClient(ownerA.UserId, "Owner");
            var boardingHouseId = await CreateBoardingHouseAsync(ownerAClient, "Owner A Media BH");
            var mediaId = await CreateMediaAsync(ownerAClient, boardingHouseId, "https://example.com/other.jpg");

            var ownerB = await RegisterOwnerAsync(_factory.CreateClient(), "owner-b-media");
            using var ownerBClient = CreateAuthorizedClient(ownerB.UserId, "Owner");

            var response = await ownerBClient.GetAsync($"{Endpoint}/{mediaId}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Admin_ShouldAccessAnyOwnersMedia()
        {
            var owner = await RegisterOwnerAsync(_factory.CreateClient(), "owner-admin-media");
            using var ownerClient = CreateAuthorizedClient(owner.UserId, "Owner");
            var boardingHouseId = await CreateBoardingHouseAsync(ownerClient, "Admin Media Target BH");
            var mediaId = await CreateMediaAsync(ownerClient, boardingHouseId, "https://example.com/admin.jpg");

            using var adminClient = CreateAuthorizedClient(Guid.NewGuid(), "Admin");
            var response = await adminClient.GetAsync($"{Endpoint}/{mediaId}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Endpoints_ShouldReturn404_WhenResourcesDoNotExist()
        {
            var owner = await RegisterOwnerAsync(_factory.CreateClient(), "owner-404-media");
            using var ownerClient = CreateAuthorizedClient(owner.UserId, "Owner");

            var byIdResponse = await ownerClient.GetAsync($"{Endpoint}/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
            var byBoardingHouseResponse = await ownerClient.GetAsync($"{Endpoint}/boarding-house/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

            byIdResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
            byBoardingHouseResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        private HttpClient CreateAuthorizedClient(Guid userId, string role)
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, userId.ToString());
            client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
            return client;
        }

        private static async Task<(Guid UserId, string Email)> RegisterOwnerAsync(HttpClient client, string tag)
        {
            var email = $"{tag}-{Guid.NewGuid():N}@example.com";
            var registerDto = new RegisterRequestDto
            {
                Email = email,
                Password = "StrongPassword123!",
                FirstName = "Test",
                LastName = "Owner"
            };

            var registerResponse = await client.PostAsJsonAsync(
                "/api/v1/auth/register",
                registerDto,
                TestContext.Current.CancellationToken);
            registerResponse.EnsureSuccessStatusCode();

            var registerResult = await registerResponse.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken: TestContext.Current.CancellationToken);
            registerResult.Should().NotBeNull();

            return (registerResult!.UserId, email);
        }

        private static async Task<Guid> CreateBoardingHouseAsync(HttpClient client, string name)
        {
            var dto = new CreateBoardingHouseDto
            {
                Name = $"{name}-{Guid.NewGuid():N}",
                AllowedGender = "Mixed",
                Address = "123 Integration St",
                Latitude = 14.5995,
                Longitude = 120.9842
            };

            var response = await client.PostAsJsonAsync("/api/v1/boardinghouses", dto, TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<BoardingHouseDto>(cancellationToken: TestContext.Current.CancellationToken);
            created.Should().NotBeNull();
            return created!.Id;
        }

        private static async Task<Guid> CreateMediaAsync(HttpClient client, Guid boardingHouseId, string url)
        {
            var dto = new CreateMediaDto
            {
                BoardingHouseId = boardingHouseId,
                Url = url,
                IsPrimary = true,
                DisplayOrder = 1
            };

            var response = await client.PostAsJsonAsync(Endpoint, dto, TestContext.Current.CancellationToken);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<MediaDto>(cancellationToken: TestContext.Current.CancellationToken);
            created.Should().NotBeNull();
            return created!.Id;
        }
    }
}
