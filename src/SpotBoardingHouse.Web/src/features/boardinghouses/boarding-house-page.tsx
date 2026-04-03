import { useState } from "react";
import { Edit, Delete } from "@mui/icons-material";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  IconButton,
  Paper,
  Snackbar,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
} from "@mui/material";
import type { AxiosError } from "axios";
import { formatDateTime } from "../../lib/format-date";
import { useAuth } from "../auth/use-auth";
import {
  useBoardingHousesQuery,
  useCreateBoardingHouse,
  useDeleteBoardingHouse,
  useUpdateBoardingHouse,
  useOwnersQuery,
} from "./queries";
import { BoardingHouseDialog } from "./boarding-house-dialog";

export function BoardingHousesPage() {
  const { role } = useAuth();
  const isAdmin = role === "Admin";
  const { data, isLoading, isError, error } = useBoardingHousesQuery();
  const { data: owners } = useOwnersQuery();
  const createBoardingHouse = useCreateBoardingHouse();
  const updateBoardingHouse = useUpdateBoardingHouse();
  const deleteBoardingHouse = useDeleteBoardingHouse();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogKey, setDialogKey] = useState(0);
  const [editingBoardingHouse, setEditingBoardingHouse] = useState<{
    id: string;
    name: string;
    description: string | null;
    allowedGender: string;
    address: string;
    latitude: number;
    longitude: number;
    facebookLink: string | null;
  } | null>(null);

  const [snackbar, setSnackbar] = useState<{
    open: boolean;
    message: string;
    severity: "success" | "error";
  }>({ open: false, message: "", severity: "success" });

  if (isLoading) return <CircularProgress />;
  if (isError)
    return (
      <Alert severity="error">
        {(error as Error).message || "Failed to load boarding houses."}
      </Alert>
    );

  const handleCreate = () => {
    setDialogKey((k: number) => k + 1);
    setEditingBoardingHouse(null);
    setDialogOpen(true);
  };

  const handleEdit = (bh: {
    id: string;
    name: string;
    description: string | null;
    allowedGender: string;
    address: string;
    latitude: number;
    longitude: number;
    facebookLink: string | null;
  }) => {
    setDialogKey((k: number) => k + 1);
    setEditingBoardingHouse(bh);
    setDialogOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this boarding house?"))
      return;
    try {
      await deleteBoardingHouse.mutateAsync(id);
      setSnackbar({
        open: true,
        message: "Boarding house deleted successfully.",
        severity: "success",
      });
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      setSnackbar({
        open: true,
        message:
          axiosError.response?.data?.message ??
          "Failed to delete boarding house.",
        severity: "error",
      });
    }
  };

  const handleSubmit = async (payload: {
    name: string;
    description?: string;
    allowedGender: string;
    address: string;
    latitude: number;
    longitude: number;
    facebookLink?: string;
    ownerId?: string;
  }) => {
    try {
      if (editingBoardingHouse) {
        await updateBoardingHouse.mutateAsync({
          id: editingBoardingHouse.id,
          payload,
        });
        setSnackbar({
          open: true,
          message: "Boarding house updated successfully.",
          severity: "success",
        });
      } else {
        await createBoardingHouse.mutateAsync(payload);
        setSnackbar({
          open: true,
          message: "Boarding house created successfully.",
          severity: "success",
        });
      }
      setDialogOpen(false);
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      setSnackbar({
        open: true,
        message: axiosError.response?.data?.message ?? "Operation failed.",
        severity: "error",
      });
    }
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
        <Typography variant="h6">
          {isAdmin ? "All Boarding Houses" : "My Boarding Houses"}
        </Typography>
        <Button variant="contained" size="small" onClick={handleCreate}>
          + New Boarding House
        </Button>
      </Box>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Name</TableCell>
            <TableCell>Address</TableCell>
            <TableCell>Gender</TableCell>
            {isAdmin && <TableCell>Owner</TableCell>}
            <TableCell>Location</TableCell>
            <TableCell>Created At</TableCell>
            <TableCell>Updated At</TableCell>
            <TableCell align="right">Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(data ?? []).map((bh) => (
            <TableRow key={bh.id}>
              <TableCell>{bh.name}</TableCell>
              <TableCell>{bh.address}</TableCell>
              <TableCell>{bh.allowedGender}</TableCell>
              {isAdmin && <TableCell>{bh.ownerName ?? "-"}</TableCell>}
              <TableCell>
                {bh.latitude.toFixed(4)}, {bh.longitude.toFixed(4)}
              </TableCell>
              <TableCell>{formatDateTime(bh.createdAt)}</TableCell>
              <TableCell>{formatDateTime(bh.updatedAt)}</TableCell>
              <TableCell align="right">
                <IconButton size="small" onClick={() => handleEdit(bh)}>
                  <Edit fontSize="small" />
                </IconButton>
                <IconButton
                  size="small"
                  color="error"
                  onClick={() => handleDelete(bh.id)}
                  disabled={deleteBoardingHouse.isPending}
                >
                  <Delete fontSize="small" />
                </IconButton>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <BoardingHouseDialog
        key={dialogKey}
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        onSubmit={handleSubmit}
        initialData={editingBoardingHouse}
        isAdmin={isAdmin}
        owners={owners ?? []}
        isLoading={
          createBoardingHouse.isPending || updateBoardingHouse.isPending
        }
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
