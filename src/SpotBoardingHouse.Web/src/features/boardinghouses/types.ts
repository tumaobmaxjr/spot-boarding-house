export type BoardingHouseDto = {
  id: string;
  userId: string;
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
