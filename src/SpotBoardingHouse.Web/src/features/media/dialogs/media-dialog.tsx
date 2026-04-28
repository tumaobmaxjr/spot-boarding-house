import {
  Alert,
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
import { useUploadImage } from "../../uploads/queries";
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
  const uploadImage = useUploadImage();
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [existingUrl] = useState(initialData?.url ?? "");
  const [isPrimary, setIsPrimary] = useState(initialData?.isPrimary ?? false);
  const [displayOrder, setDisplayOrder] = useState(initialData?.displayOrder ?? 0);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [uploadError, setUploadError] = useState<string | null>(null);

  const validate = () => {
    const nextErrors: Record<string, string> = {};
    if (!isEdit && !selectedFile) nextErrors.file = "Image file is required";
    if (!Number.isFinite(displayOrder)) nextErrors.displayOrder = "Display order is required";
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    setUploadError(null);

    let uploadedUrl: string | undefined;

    if (selectedFile) {
      try {
        const uploaded = await uploadImage.mutateAsync(selectedFile);
        uploadedUrl = uploaded.url;
      } catch (err) {
        setUploadError(err instanceof Error ? err.message : "Image upload failed.");
        return;
      }
    }

    if (isEdit) {
      onSubmit({
        ...(uploadedUrl ? { url: uploadedUrl } : {}),
        isPrimary,
        displayOrder,
      });
      return;
    }

    onSubmit({
      boardingHouseId,
      roomId,
      url: uploadedUrl ?? "",
      isPrimary,
      displayOrder,
    });
  };

  const isSubmitting = Boolean(isLoading || uploadImage.isPending);

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
          {!isEdit && (
            <TextField
              autoFocus
              type="file"
              inputProps={{ accept: "image/*" }}
              onChange={(e) => {
                const input = e.target as HTMLInputElement;
                setSelectedFile(input.files?.[0] ?? null);
              }}
              error={!!errors.file}
              helperText={errors.file || "Upload an image file (jpg, png, webp, gif, avif)."}
              required
              fullWidth
            />
          )}

          {isEdit && (
            <>
              <TextField
                type="file"
                inputProps={{ accept: "image/*" }}
                onChange={(e) => {
                  const input = e.target as HTMLInputElement;
                  setSelectedFile(input.files?.[0] ?? null);
                }}
                helperText="Optional: select a new image to replace existing file."
                fullWidth
              />
              {existingUrl && (
                <Alert severity="info" variant="outlined">
                  Current image: {existingUrl}
                </Alert>
              )}
            </>
          )}

          {uploadError && <Alert severity="error">{uploadError}</Alert>}

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
          <Button onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" variant="contained" disabled={isSubmitting}>
            {isSubmitting ? "Saving..." : isEdit ? "Update" : "Create"}
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
