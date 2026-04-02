using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SpotBoardingHouse.Application.DTOs.BoardingHouses;

namespace SpotBoardingHouse.Api.IntegrationTests.Controllers
{
    public class BoardingHousesControllerTests : IClassFixture<WebApplicationFactory<SpotBoardingHouse.Api.Program>>
    {
        private const string Endpoint = "/api/v1/boardinghouses";
        private readonly HttpClient _client;

        public BoardingHousesControllerTests(WebApplicationFactory<SpotBoardingHouse.Api.Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetAll_ShouldReturn401_WhenNotAuthenticated()
        {
            var response = await _client.GetAsync(Endpoint, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_ShouldReturn401_WhenNotAuthenticated()
        {
            var dto = new CreateBoardingHouseDto
            {
                Name = "Test BH",
                AllowedGender = "Mixed",
                Address = "123 Test St",
                Latitude = 14.5,
                Longitude = 121.0
            };

            var response = await _client.PostAsJsonAsync(Endpoint, dto, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetById_ShouldReturn401_WhenNotAuthenticated()
        {
            var id = Guid.NewGuid();

            var response = await _client.GetAsync($"{Endpoint}/{id}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Update_ShouldReturn401_WhenNotAuthenticated()
        {
            var id = Guid.NewGuid();
            var dto = new UpdateBoardingHouseDto
            {
                Name = "Updated Name"
            };

            var response = await _client.PutAsJsonAsync($"{Endpoint}/{id}", dto, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Delete_ShouldReturn401_WhenNotAuthenticated()
        {
            var id = Guid.NewGuid();

            var response = await _client.DeleteAsync($"{Endpoint}/{id}", TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
