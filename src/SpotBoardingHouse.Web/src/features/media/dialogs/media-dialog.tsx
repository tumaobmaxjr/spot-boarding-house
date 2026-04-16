import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
} from "@mui/material";
import { useState } from "react";
import type { CreateMediaPayload, UpdateMediaPayload } from "../types";

interface MediaDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (payload: CreateMediaPayload | UpdateMediaPayload) => void;
  boardingHouseId?: string;
  roomId?: string;
  initialData?: {
    url: string;
    isPrimary: boolean;
    displayOrder: number;
  } | null;
  isLoading?: boolean;
}

export function MediaDialog({
  open,
  onClose,
  onSubmit,
  boardingHouseId,
  roomId,
  initialData,
  isLoading,
}: MediaDialogProps) {
  const isEdit = !!initialData;
  const [url, setUrl] = useState(initialData?.url ?? "");
  const [isPrimary, setIsPrimary] = useState(initialData?.isPrimary ?? false);
  const [displayOrder, setDisplayOrder] = useState(initialData?.displayOrder ?? 0);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = () => {
    const nextErrors: Record<string, string> = {};
    if (!url.trim()) nextErrors.url = "URL is required";
    if (url.trim().length > 500) nextErrors.url = "URL must be 500 characters or less";
    if (!Number.isFinite(displayOrder)) nextErrors.displayOrder = "Display order is required";
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    if (isEdit) {
      onSubmit({
        url: url.trim(),
        isPrimary,
        displayOrder,
      });
      return;
    }

    onSubmit({
      boardingHouseId,
      roomId,
      url: url.trim(),
      isPrimary,
      displayOrder,
    });
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>{isEdit ? "Edit Media" : "Add Media"}</DialogTitle>
        <DialogContent
          sx={{
            display: "flex",
            flexDirection: "column",
            gap: 2,
            pt: 2,
            overflow: "visible",
          }}
        >
          <TextField
            autoFocus
            label="Media URL"
            value={url}
            onChange={(e) => setUrl(e.target.value)}
            error={!!errors.url}
            helperText={errors.url}
            required
            fullWidth
          />

          <TextField
            label="Display Order"
            type="number"
            value={displayOrder}
            onChange={(e) => setDisplayOrder(parseInt(e.target.value || "0", 10))}
            error={!!errors.displayOrder}
            helperText={errors.displayOrder}
            required
            fullWidth
          />

          <FormControl fullWidth>
            <InputLabel id="media-primary-label">Primary</InputLabel>
            <Select
              labelId="media-primary-label"
              label="Primary"
              value={isPrimary ? "true" : "false"}
              onChange={(e) => setIsPrimary(e.target.value === "true")}
            >
              <MenuItem value="true">Yes</MenuItem>
              <MenuItem value="false">No</MenuItem>
            </Select>
          </FormControl>
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
