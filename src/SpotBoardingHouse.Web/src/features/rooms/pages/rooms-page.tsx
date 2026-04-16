import { Fragment, useState } from "react";
import { Delete, Edit } from "@mui/icons-material";
import {
  Alert,
  Box,
  Button,
  Chip,
  Collapse,
  CircularProgress,
  IconButton,
  Paper,
  Stack,
  Snackbar,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import KeyboardArrowDownIcon from "@mui/icons-material/KeyboardArrowDown";
import KeyboardArrowUpIcon from "@mui/icons-material/KeyboardArrowUp";
import type { AxiosError } from "axios";
import { formatDateTime } from "../../../lib/format-date";
import { useBoardingHousesQuery } from "../../boardinghouses/queries";
import {
  useCreateRoom,
  useDeleteRoom,
  useRoomsQuery,
  useUpdateRoom,
} from "../queries";
import { RoomDialog } from "../dialogs/room-dialog";
import type { CreateRoomPayload, RoomDto, UpdateRoomPayload } from "../types";
import { RoomMediaSection } from "../../media/components/room-media-section";

export function RoomsPage() {
  const { data, isLoading, isError, error } = useRoomsQuery();
  const { data: boardingHouses } = useBoardingHousesQuery();
  const createRoom = useCreateRoom();
  const updateRoom = useUpdateRoom();
  const deleteRoom = useDeleteRoom();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogKey, setDialogKey] = useState(0);
  const [editingRoom, setEditingRoom] = useState<RoomDto | null>(null);
  const [expandedRows, setExpandedRows] = useState<Record<string, boolean>>({});
  const [snackbar, setSnackbar] = useState<{
    open: boolean;
    message: string;
    severity: "success" | "error";
  }>({ open: false, message: "", severity: "success" });

  if (isLoading) return <CircularProgress />;
  if (isError) {
    return (
      <Alert severity="error">{(error as Error).message || "Failed to load rooms."}</Alert>
    );
  }

  const handleCreate = () => {
    setDialogKey((k) => k + 1);
    setEditingRoom(null);
    setDialogOpen(true);
  };

  const handleEdit = (room: RoomDto) => {
    setDialogKey((k) => k + 1);
    setEditingRoom(room);
    setDialogOpen(true);
  };

  const handleSubmit = async (payload: CreateRoomPayload | UpdateRoomPayload) => {
    try {
      if (editingRoom) {
        await updateRoom.mutateAsync({
          id: editingRoom.id,
          payload: payload as UpdateRoomPayload,
        });
        setSnackbar({
          open: true,
          message: "Room updated successfully.",
          severity: "success",
        });
      } else {
        await createRoom.mutateAsync(payload as CreateRoomPayload);
        setSnackbar({
          open: true,
          message: "Room created successfully.",
          severity: "success",
        });
      }
      setDialogOpen(false);
      setEditingRoom(null);
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      setSnackbar({
        open: true,
        message: axiosError.response?.data?.message ?? "Room operation failed.",
        severity: "error",
      });
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this room?")) return;

    try {
      await deleteRoom.mutateAsync(id);
      setSnackbar({
        open: true,
        message: "Room deleted successfully.",
        severity: "success",
      });
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      setSnackbar({
        open: true,
        message: axiosError.response?.data?.message ?? "Failed to delete room.",
        severity: "error",
      });
    }
  };

  const toggleExpanded = (id: string) => {
    setExpandedRows((prev) => ({
      ...prev,
      [id]: !prev[id],
    }));
  };

  return (
    <Paper sx={{ p: 2 }}>
      <Box
        sx={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          mb: 2,
        }}
      >
        <Typography variant="h6">Rooms</Typography>
        <Button variant="contained" size="small" onClick={handleCreate}>
          + New Room
        </Button>
      </Box>

      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell width={56} />
            <TableCell>Boarding House</TableCell>
            <TableCell>Room #</TableCell>
            <TableCell>Capacity</TableCell>
            <TableCell>Occupied</TableCell>
            <TableCell>Remaining</TableCell>
            <TableCell>Status</TableCell>
            <TableCell>Description</TableCell>
            <TableCell>Created At</TableCell>
            <TableCell>Updated At</TableCell>
            <TableCell align="right">Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(data ?? []).map((room) => (
            <Fragment key={room.id}>
              <TableRow>
                <TableCell>
                  <IconButton size="small" onClick={() => toggleExpanded(room.id)}>
                    {expandedRows[room.id] ? (
                      <KeyboardArrowUpIcon fontSize="small" />
                    ) : (
                      <KeyboardArrowDownIcon fontSize="small" />
                    )}
                  </IconButton>
                </TableCell>
                <TableCell>{room.boardingHouseName}</TableCell>
                <TableCell>{room.roomNumber}</TableCell>
                <TableCell>{room.baseCapacity}</TableCell>
                <TableCell>{room.occupiedSlots}</TableCell>
                <TableCell>{room.remainingSlots}</TableCell>
                <TableCell>
                  <Chip
                    size="small"
                    color={room.occupancyStatus === "Full" ? "warning" : "success"}
                    label={room.occupancyStatus}
                  />
                </TableCell>
                <TableCell>{room.additionalDescription || "-"}</TableCell>
                <TableCell>{formatDateTime(room.createdAt)}</TableCell>
                <TableCell>{formatDateTime(room.updatedAt)}</TableCell>
                <TableCell align="right">
                  <IconButton size="small" onClick={() => handleEdit(room)}>
                    <Edit fontSize="small" />
                  </IconButton>
                  <IconButton
                    size="small"
                    color="error"
                    onClick={() => handleDelete(room.id)}
                    disabled={deleteRoom.isPending}
                  >
                    <Delete fontSize="small" />
                  </IconButton>
                </TableCell>
              </TableRow>
              <TableRow>
                <TableCell colSpan={11} sx={{ py: 0, borderBottom: 0 }}>
                  <Collapse in={!!expandedRows[room.id]} timeout="auto" unmountOnExit>
                    <Box sx={{ p: 2 }}>
                      <Stack spacing={2}>
                        <RoomMediaSection roomId={room.id} />
                      </Stack>
                    </Box>
                  </Collapse>
                </TableCell>
              </TableRow>
            </Fragment>
          ))}
          {(data ?? []).length === 0 && (
            <TableRow>
              <TableCell colSpan={11}>No rooms yet.</TableCell>
            </TableRow>
          )}
        </TableBody>
      </Table>

      <RoomDialog
        key={dialogKey}
        open={dialogOpen}
        onClose={() => {
          setDialogOpen(false);
          setEditingRoom(null);
        }}
        onSubmit={handleSubmit}
        initialData={
          editingRoom
            ? {
                boardingHouseId: editingRoom.boardingHouseId,
                roomNumber: editingRoom.roomNumber,
                baseCapacity: editingRoom.baseCapacity,
                occupiedSlots: editingRoom.occupiedSlots,
                additionalDescription: editingRoom.additionalDescription,
              }
            : null
        }
        boardingHouses={(boardingHouses ?? []).map((bh) => ({
          id: bh.id,
          name: bh.name,
        }))}
        isLoading={createRoom.isPending || updateRoom.isPending}
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
    </Paper>
  );
}
