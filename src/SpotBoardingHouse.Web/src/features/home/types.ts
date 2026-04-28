export type PublicBoardingHouseDto = {
  id: string;
  name: string;
  description: string | null;
  allowedGender: string;
  address: string;
  latitude: number;
  longitude: number;
  facebookLink: string | null;
  ownerName?: string | null;
  primaryImageUrl?: string | null;
  minPrice?: number | null;
  amenities?: string[];
  roomCount?: number;
};

export type PublicBoardingHouseDetailDto = {
  id: string;
  name: string;
  description: string | null;
  allowedGender: string;
  address: string;
  latitude: number;
  longitude: number;
  facebookLink: string | null;
  ownerName: string | null;
  primaryImageUrl: string | null;
  mediaUrls: string[];
  minPrice: number | null;
  amenities: Array<{
    name: string;
    icon: string | null;
  }>;
  contacts: Array<{
    id: string;
    phoneNumber: string;
    network: string;
    isPrimary: boolean;
  }>;
  roomCount: number;
  rooms: Array<{
    id: string;
    roomNumber: string;
    baseCapacity: number;
    occupiedSlots: number;
    remainingSlots: number;
    occupancyStatus: string;
    additionalDescription?: string | null;
  }>;
};
