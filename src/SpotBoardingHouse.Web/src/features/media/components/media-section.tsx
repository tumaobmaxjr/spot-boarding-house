import { useState } from "react";
import { Delete, Edit } from "@mui/icons-material";
import {
  Alert,
  Box,
  Button,
  Chip,
  IconButton,
  Link,
  Snackbar,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import type { AxiosError } from "axios";
import { formatDateTime } from "../../../lib/format-date";
import {
  useCreateMedia,
  useDeleteMedia,
  useMediaByBoardingHouseQuery,
  useUpdateMedia,
} from "../queries";
import { MediaDialog } from "../dialogs/media-dialog";
import type { CreateMediaPayload, MediaDto, UpdateMediaPayload } from "../types";

interface MediaSectionProps {
  boardingHouseId: string;
}

export function MediaSection({ boardingHouseId }: MediaSectionProps) {
  const { data, isLoading, isError, error } =
    useMediaByBoardingHouseQuery(boardingHouseId);
  const createMedia = useCreateMedia();
  const updateMedia = useUpdateMedia();
  const deleteMedia = useDeleteMedia();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingMedia, setEditingMedia] = useState<MediaDto | null>(null);
  const [snackbar, setSnackbar] = useState<{
    open: boolean;
    message: string;
    severity: "success" | "error";
  }>({ open: false, message: "", severity: "success" });

  const handleSubmit = async (payload: CreateMediaPayload | UpdateMediaPayload) => {
    try {
      if (editingMedia) {
        await updateMedia.mutateAsync({
          id: editingMedia.id,
          payload: payload as UpdateMediaPayload,
        });
        setSnackbar({
          open: true,
          message: "Media updated successfully.",
          severity: "success",
        });
      } else {
        await createMedia.mutateAsync(payload as CreateMediaPayload);
        setSnackbar({
          open: true,
          message: "Media created successfully.",
          severity: "success",
        });
      }
      setDialogOpen(false);
      setEditingMedia(null);
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      setSnackbar({
        open: true,
        message: axiosError.response?.data?.message ?? "Media operation failed.",
        severity: "error",
      });
    }
  };

  const handleDelete = async (media: MediaDto) => {
    if (!window.confirm("Are you sure you want to delete this media?")) return;

    try {
      await deleteMedia.mutateAsync({
        id: media.id,
        boardingHouseId,
      });
      setSnackbar({
        open: true,
        message: "Media deleted successfully.",
        severity: "success",
      });
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      setSnackbar({
        open: true,
        message: axiosError.response?.data?.message ?? "Failed to delete media.",
        severity: "error",
      });
    }
  };

  return (
    <Box>
      <Box
        sx={{
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          mb: 1,
        }}
      >
        <Typography variant="subtitle1" fontWeight={700}>
          Media
        </Typography>
        <Button
          size="small"
          variant="outlined"
          onClick={() => {
            setEditingMedia(null);
            setDialogOpen(true);
          }}
        >
          + Add Media
        </Button>
      </Box>

      {isError && (
        <Alert severity="error" sx={{ mb: 1 }}>
          {(error as Error).message || "Failed to load media."}
        </Alert>
      )}

      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>URL</TableCell>
            <TableCell>Primary</TableCell>
            <TableCell>Order</TableCell>
            <TableCell>Updated At</TableCell>
            <TableCell align="right">Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {!isLoading && (data ?? []).length === 0 && (
            <TableRow>
              <TableCell colSpan={5}>No media yet.</TableCell>
            </TableRow>
          )}
          {(data ?? []).map((media) => (
            <TableRow key={media.id}>
              <TableCell>
                <Link href={media.url} target="_blank" rel="noreferrer" underline="hover">
                  {media.url}
                </Link>
              </TableCell>
              <TableCell>
                {media.isPrimary ? (
                  <Chip size="small" color="success" label="Primary" />
                ) : (
                  "-"
                )}
              </TableCell>
              <TableCell>{media.displayOrder}</TableCell>
              <TableCell>{media.updatedAt ? formatDateTime(media.updatedAt) : "-"}</TableCell>
              <TableCell align="right">
                <IconButton
                  size="small"
                  onClick={() => {
                    setEditingMedia(media);
                    setDialogOpen(true);
                  }}
                >
                  <Edit fontSize="small" />
                </IconButton>
                <IconButton
                  size="small"
                  color="error"
                  disabled={deleteMedia.isPending}
                  onClick={() => handleDelete(media)}
                >
                  <Delete fontSize="small" />
                </IconButton>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <MediaDialog
        open={dialogOpen}
        onClose={() => {
          setDialogOpen(false);
          setEditingMedia(null);
        }}
        onSubmit={handleSubmit}
        boardingHouseId={boardingHouseId}
        initialData={
          editingMedia
            ? {
                url: editingMedia.url,
                isPrimary: editingMedia.isPrimary,
                displayOrder: editingMedia.displayOrder,
              }
            : null
        }
        isLoading={createMedia.isPending || updateMedia.isPending}
      />

      <Snackbar
        anchorOrigin={{ vertical: "bottom", horizontal: "right" }}
        open={snackbar.open}
        autoHideDuration={3000}
        onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
      >
        <Alert
          severity={snackbar.severity}
          onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
