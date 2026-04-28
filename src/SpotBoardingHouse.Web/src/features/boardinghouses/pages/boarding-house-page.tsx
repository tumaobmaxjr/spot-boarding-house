import { Fragment, useState } from "react";
import { Edit, Delete } from "@mui/icons-material";
import {
  Alert,
  Box,
  Button,
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
import { useAuth } from "../../auth/use-auth";
import {
  useBoardingHousesQuery,
  useCreateBoardingHouse,
  useDeleteBoardingHouse,
  useUpdateBoardingHouse,
  useOwnersQuery,
} from "../queries";
import { BoardingHouseDialog } from "../dialogs/boarding-house-dialog";
import { ContactsSection } from "../../contacts/components/contacts-section";
import { MediaSection } from "../../media/components/media-section";

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
  const [expandedRows, setExpandedRows] = useState<Record<string, boolean>>({});
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(5);
  const [orderBy, setOrderBy] = useState<"name" | "createdAt">("createdAt");
  const [order, setOrder] = useState<"asc" | "desc">("desc");

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
    return a.name.localeCompare(b.name) * direction;
  });
  const visibleData = sortedData.slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage);

  const handleSort = (field: "name" | "createdAt") => {
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
        <Typography variant="h6">
          {isAdmin ? "All Boarding Houses" : "My Boarding Houses"}
        </Typography>
        <Button variant="contained" size="small" onClick={handleCreate}>
          + New Boarding House
        </Button>
      </Box>
      <Table size="small">
        <TableHead>
          <TableRow sx={{ bgcolor: "grey.100" }}>
            <TableCell width={56} />
            <TableCell sx={{ fontWeight: 700 }}>
              <TableSortLabel active={orderBy === "name"} direction={orderBy === "name" ? order : "asc"} onClick={() => handleSort("name")}>Name</TableSortLabel>
            </TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Address</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Gender</TableCell>
            {isAdmin && <TableCell sx={{ fontWeight: 700 }}>Owner</TableCell>}
            <TableCell sx={{ fontWeight: 700 }}>Location</TableCell>
            <TableCell sx={{ fontWeight: 700 }}>
              <TableSortLabel active={orderBy === "createdAt"} direction={orderBy === "createdAt" ? order : "asc"} onClick={() => handleSort("createdAt")}>Created At</TableSortLabel>
            </TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Updated At</TableCell>
            <TableCell align="right" sx={{ fontWeight: 700 }}>Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {visibleData.map((bh) => (
            <Fragment key={bh.id}>
              <TableRow>
                <TableCell>
                  <IconButton size="small" onClick={() => toggleExpanded(bh.id)}>
                    {expandedRows[bh.id] ? (
                      <KeyboardArrowUpIcon fontSize="small" />
                    ) : (
                      <KeyboardArrowDownIcon fontSize="small" />
                    )}
                  </IconButton>
                </TableCell>
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
              <TableRow>
                <TableCell colSpan={isAdmin ? 9 : 8} sx={{ py: 0, borderBottom: 0 }}>
                  <Collapse in={!!expandedRows[bh.id]} timeout="auto" unmountOnExit>
                    <Box sx={{ p: 2 }}>
                      <Stack spacing={2}>
                        <ContactsSection boardingHouseId={bh.id} />
                        <MediaSection boardingHouseId={bh.id} />
                      </Stack>
                    </Box>
                  </Collapse>
                </TableCell>
              </TableRow>
            </Fragment>
          ))}
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
