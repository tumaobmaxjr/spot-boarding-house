namespace SpotBoardingHouse.Application.DTOs.RoomOffers
{
    public class UpdateRoomOfferDto
    {
        public Guid? RoomTypeId { get; set; }
        public int? Capacity { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? IsAvailable { get; set; }
    }
}
