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
  TableSortLabel,
  TablePagination,
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
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(5);
  const [orderBy, setOrderBy] = useState<"roomNumber" | "createdAt">("createdAt");
  const [order, setOrder] = useState<"asc" | "desc">("desc");
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

  const sortedData = [...(data ?? [])].sort((a, b) => {
    const direction = order === "asc" ? 1 : -1;
    if (orderBy === "createdAt") {
      return (new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()) * direction;
    }
    return a.roomNumber.localeCompare(b.roomNumber) * direction;
  });
  const visibleData = sortedData.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage);

  const handleSort = (field: "roomNumber" | "createdAt") => {
    if (orderBy === field) {
      setOrder((prev) => (prev === "asc" ? "desc" : "asc"));
      return;
    }
    setOrderBy(field);
    setOrder("asc");
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
          <TableRow sx={{ bgcolor: "grey.100" }}>
            <TableCell width={56} />
            <TableCell sx={{ fontWeight: 700 }}>Boarding House</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>
              <TableSortLabel active={orderBy === "roomNumber"} direction={orderBy === "roomNumber" ? order : "asc"} onClick={() => handleSort("roomNumber")}>Room #</TableSortLabel>
            </TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Capacity</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Occupied</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Remaining</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Status</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Description</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>
              <TableSortLabel active={orderBy === "createdAt"} direction={orderBy === "createdAt" ? order : "asc"} onClick={() => handleSort("createdAt")}>Created At</TableSortLabel>
            </TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Updated At</TableCell>
            <TableCell align="right" sx={{ fontWeight: 700 }}>Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {visibleData.map((room) => (
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
      <TablePagination
        component="div"
        count={(data ?? []).length}
        page={page}
        onPageChange={(_e, newPage) => setPage(newPage)}
        rowsPerPage={rowsPerPage}
        onRowsPerPageChange={(e) => {
          setRowsPerPage(parseInt(e.target.value, 10));
          setPage(0);
        }}
        rowsPerPageOptions={[5, 10, 25]}
      />

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
          address: bh.address,
          ownerName: bh.ownerName,
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
