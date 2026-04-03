export type BoardingHouseDto = {
  id: string;
  userId: string;
  ownerName?: string;
  name: string;
  description: string | null;
  allowedGender: string;
  address: string;
  latitude: number;
  longitude: number;
  facebookLink: string | null;
  createdAt: string;
  updatedAt: string | null;
};

export type OwnerDto = {
  id: string;
  firstName: string;
  lastName: string;
  email?: string;
};
