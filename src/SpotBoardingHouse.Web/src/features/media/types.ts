export type MediaDto = {
  id: string;
  boardingHouseId: string | null;
  roomId: string | null;
  url: string;
  isPrimary: boolean;
  displayOrder: number;
  createdAt: string;
  updatedAt: string | null;
};

export type CreateMediaPayload = {
  boardingHouseId?: string;
  roomId?: string;
  url: string;
  isPrimary: boolean;
  displayOrder: number;
};

export type UpdateMediaPayload = {
  url?: string;
  isPrimary?: boolean;
  displayOrder?: number;
};
