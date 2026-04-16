export type RoomDto = {
  id: string;
  boardingHouseId: string;
  boardingHouseName: string;
  roomNumber: string;
  baseCapacity: number;
  occupiedSlots: number;
  remainingSlots: number;
  occupancyStatus: string;
  additionalDescription: string | null;
  createdAt: string;
  updatedAt: string | null;
};

export type CreateRoomPayload = {
  boardingHouseId: string;
  roomNumber: string;
  baseCapacity: number;
  occupiedSlots: number;
  additionalDescription?: string;
};

export type UpdateRoomPayload = {
  roomNumber?: string;
  baseCapacity?: number;
  occupiedSlots?: number;
  additionalDescription?: string;
};
