export type ContactDto = {
  id: string;
  boardingHouseId: string;
  phoneNumber: string;
  network: string;
  isPrimary: boolean;
  createdAt: string;
  updatedAt: string | null;
};

export type CreateContactPayload = {
  boardingHouseId: string;
  phoneNumber: string;
  network: string;
  isPrimary: boolean;
};

export type UpdateContactPayload = {
  phoneNumber?: string;
  network?: string;
  isPrimary?: boolean;
};
