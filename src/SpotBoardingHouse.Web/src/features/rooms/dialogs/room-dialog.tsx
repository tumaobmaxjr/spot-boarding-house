import {
  Autocomplete,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormHelperText,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import type { CreateRoomPayload, UpdateRoomPayload } from "../types";

interface RoomDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (payload: CreateRoomPayload | UpdateRoomPayload) => void;
  initialData?: {
    boardingHouseId: string;
    roomNumber: string;
    baseCapacity: number;
    occupiedSlots: number;
    additionalDescription: string | null;
  } | null;
  boardingHouses: {
    id: string;
    name: string;
    address?: string;
    ownerName?: string;
  }[];
  isLoading?: boolean;
}

export function RoomDialog({
  open,
  onClose,
  onSubmit,
  initialData,
  boardingHouses,
  isLoading,
}: RoomDialogProps) {
  const isEdit = !!initialData;
  const [boardingHouseId, setBoardingHouseId] = useState(
    initialData?.boardingHouseId ?? "",
  );
  const [roomNumber, setRoomNumber] = useState(initialData?.roomNumber ?? "");
  const [baseCapacity, setBaseCapacity] = useState(initialData?.baseCapacity ?? 1);
  const [occupiedSlots, setOccupiedSlots] = useState(initialData?.occupiedSlots ?? 0);
  const [additionalDescription, setAdditionalDescription] = useState(
    initialData?.additionalDescription ?? "",
  );
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = () => {
    const nextErrors: Record<string, string> = {};

    if (!isEdit && !boardingHouseId) {
      nextErrors.boardingHouseId = "Boarding house is required";
    }

    if (!roomNumber.trim()) {
      nextErrors.roomNumber = "Room number is required";
    }

    if (roomNumber.trim().length > 50) {
      nextErrors.roomNumber = "Room number must be 50 characters or less";
    }

    if (!Number.isFinite(baseCapacity) || baseCapacity < 1) {
      nextErrors.baseCapacity = "Base capacity must be at least 1";
    }

    if (!Number.isFinite(occupiedSlots) || occupiedSlots < 0) {
      nextErrors.occupiedSlots = "Occupied slots cannot be negative";
    }

    if (occupiedSlots > baseCapacity) {
      nextErrors.occupiedSlots = "Occupied slots cannot exceed base capacity";
    }

    if (additionalDescription.trim().length > 500) {
      nextErrors.additionalDescription =
        "Additional description must be 500 characters or less";
    }

    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    const payload = {
      roomNumber: roomNumber.trim(),
      baseCapacity,
      occupiedSlots,
      additionalDescription: additionalDescription.trim() || undefined,
    };

    if (isEdit) {
      onSubmit(payload);
      return;
    }

    onSubmit({
      boardingHouseId,
      ...payload,
    });
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>{isEdit ? "Edit Room" : "Add Room"}</DialogTitle>
        <DialogContent
          sx={{
            display: "flex",
            flexDirection: "column",
            gap: 2,
            pt: 2,
            overflow: "visible",
          }}
        >
          {!isEdit && (
            <FormControl error={!!errors.boardingHouseId} fullWidth>
              <Autocomplete
                options={boardingHouses}
                value={boardingHouses.find((bh) => bh.id === boardingHouseId) ?? null}
                onChange={(_event, value) => setBoardingHouseId(value?.id ?? "")}
                getOptionLabel={(option) => option.name}
                isOptionEqualToValue={(option, value) => option.id === value.id}
                renderOption={(props, option) => (
                  <li {...props} key={option.id}>
                    <div>
                      <Typography variant="body2" fontWeight={600}>
                        {option.name}
                      </Typography>
                      <Typography variant="caption" color="text.secondary">
                        {option.address ?? "No address"}
                        {option.ownerName ? ` - Owner: ${option.ownerName}` : ""}
                      </Typography>
                    </div>
                  </li>
                )}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Boarding House"
                    placeholder="Search boarding house"
                    error={!!errors.boardingHouseId}
                  />
                )}
              />
              {errors.boardingHouseId && <FormHelperText>{errors.boardingHouseId}</FormHelperText>}
            </FormControl>
          )}

          <TextField
            autoFocus
            label="Room Number"
            value={roomNumber}
            onChange={(e) => setRoomNumber(e.target.value)}
            error={!!errors.roomNumber}
            helperText={errors.roomNumber}
            required
            fullWidth
          />

          <TextField
            label="Base Capacity"
            type="number"
            value={baseCapacity}
            onChange={(e) => setBaseCapacity(parseInt(e.target.value || "0", 10))}
            error={!!errors.baseCapacity}
            helperText={errors.baseCapacity}
            required
            fullWidth
          />

          <TextField
            label="Occupied Slots"
            type="number"
            value={occupiedSlots}
            onChange={(e) => setOccupiedSlots(parseInt(e.target.value || "0", 10))}
            error={!!errors.occupiedSlots}
            helperText={errors.occupiedSlots}
            required
            fullWidth
          />

          <TextField
            label="Additional Description"
            value={additionalDescription}
            onChange={(e) => setAdditionalDescription(e.target.value)}
            error={!!errors.additionalDescription}
            helperText={errors.additionalDescription}
            multiline
            rows={3}
            fullWidth
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose} disabled={isLoading}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={isLoading}>
            {isLoading ? "Saving..." : isEdit ? "Update" : "Create"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
