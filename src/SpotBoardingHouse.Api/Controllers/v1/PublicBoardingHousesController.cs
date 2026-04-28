using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotBoardingHouse.Application.Interfaces.Repositories;
using SpotBoardingHouse.Application.Interfaces.Services;

namespace SpotBoardingHouse.Api.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [AllowAnonymous]
    [Route("api/v{version:apiVersion}/public/boarding-houses")]
    public class PublicBoardingHousesController : ControllerBase
    {
        private readonly IBoardingHouseService _boardingHouseService;
        private readonly IRoomService _roomService;
        private readonly IMediaRepository _mediaRepository;
        private readonly IBoardingHouseAmenityRepository _boardingHouseAmenityRepository;
        private readonly IRoomOfferRepository _roomOfferRepository;
        private readonly IContactRepository _contactRepository;

        public PublicBoardingHousesController(
            IBoardingHouseService boardingHouseService,
            IRoomService roomService,
            IMediaRepository mediaRepository,
            IBoardingHouseAmenityRepository boardingHouseAmenityRepository,
            IRoomOfferRepository roomOfferRepository,
            IContactRepository contactRepository)
        {
            _boardingHouseService = boardingHouseService;
            _roomService = roomService;
            _mediaRepository = mediaRepository;
            _boardingHouseAmenityRepository = boardingHouseAmenityRepository;
            _roomOfferRepository = roomOfferRepository;
            _contactRepository = contactRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var boardingHouses = await _boardingHouseService.GetAllAsync();
            var result = new List<object>();

            foreach (var b in boardingHouses)
            {
                var media = await _mediaRepository.GetAllByBoardingHouseIdAsync(b.Id);
                var amenities = await _boardingHouseAmenityRepository.GetAmenitiesByBoardingHouseIdAsync(b.Id);
                var rooms = await _roomService.GetAllByBoardingHouseIdAsync(b.Id);
                var minPrice = await GetMinimumPriceAsync(rooms.Select(r => r.Id));

                result.Add(new
                {
                    b.Id,
                    b.Name,
                    b.Description,
                    b.AllowedGender,
                    b.Address,
                    b.Latitude,
                    b.Longitude,
                    b.FacebookLink,
                    b.OwnerName,
                    PrimaryImageUrl = media.OrderByDescending(m => m.IsPrimary).ThenBy(m => m.DisplayOrder).FirstOrDefault()?.Url,
                    MinPrice = minPrice,
                    Amenities = amenities.Select(a => a.Name).Take(6).ToArray(),
                    RoomCount = rooms.Count(),
                });
            }

            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var bh = await _boardingHouseService.GetByIdAsync(id);
            if (bh == null) return NotFound();

            var rooms = await _roomService.GetAllByBoardingHouseIdAsync(id);
            var media = await _mediaRepository.GetAllByBoardingHouseIdAsync(id);
            var amenities = await _boardingHouseAmenityRepository.GetAmenitiesByBoardingHouseIdAsync(id);
            var contacts = await _contactRepository.GetAllByBoardingHouseIdAsync(id);
            var minPrice = await GetMinimumPriceAsync(rooms.Select(r => r.Id));

            return Ok(new
            {
                bh.Id,
                bh.Name,
                bh.Description,
                bh.AllowedGender,
                bh.Address,
                bh.Latitude,
                bh.Longitude,
                bh.FacebookLink,
                bh.OwnerName,
                PrimaryImageUrl = media.OrderByDescending(m => m.IsPrimary).ThenBy(m => m.DisplayOrder).FirstOrDefault()?.Url,
                MediaUrls = media.OrderByDescending(m => m.IsPrimary).ThenBy(m => m.DisplayOrder).Select(m => m.Url).ToArray(),
                MinPrice = minPrice,
                Amenities = amenities.Select(a => new { a.Name, a.Icon }).ToArray(),
                Contacts = contacts.Select(c => new
                {
                    c.Id,
                    c.PhoneNumber,
                    c.Network,
                    c.IsPrimary,
                }).ToArray(),
                RoomCount = rooms.Count(),
                Rooms = rooms.Select(r => new
                {
                    r.Id,
                    r.RoomNumber,
                    r.BaseCapacity,
                    r.OccupiedSlots,
                    r.RemainingSlots,
                    r.OccupancyStatus,
                    r.AdditionalDescription,
                })
            });
        }

        private async Task<decimal?> GetMinimumPriceAsync(IEnumerable<Guid> roomIds)
        {
            decimal? minPrice = null;

            foreach (var roomId in roomIds)
            {
                var offers = await _roomOfferRepository.GetAllByRoomIdAsync(roomId);
                var roomMin = offers
                    .Where(o => o.IsAvailable)
                    .Select(o => (decimal?)o.MinPrice)
                    .Min();

                if (roomMin.HasValue && (!minPrice.HasValue || roomMin.Value < minPrice.Value))
                {
                    minPrice = roomMin.Value;
                }
            }

            return minPrice;
        }
    }
}
