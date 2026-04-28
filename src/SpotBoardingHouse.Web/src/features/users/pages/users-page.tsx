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
  TableSortLabel,
  TablePagination,
  TableRow,
  Typography,
} from "@mui/material";
import {
  useCreateUser,
  useDeleteUser,
  useRolesQuery,
  useUpdateUser,
  useUsersQuery,
} from "../queries";
import { useState } from "react";
import { Delete, Edit } from "@mui/icons-material";
import { UserDialog } from "../dialogs/user-dialog";
import type { AxiosError } from "axios";
import { formatDateTime } from "../../../lib/format-date";

export function UsersPage() {
  const { data, isLoading, isError, error } = useUsersQuery();
  const { data: roles } = useRolesQuery();
  const createUser = useCreateUser();
  const updateUser = useUpdateUser();
  const deleteUser = useDeleteUser();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingUser, setEditingUser] = useState<{
    id: string;
    roleId: string;
    firstName: string;
    lastName: string;
    email: string;
  } | null>(null);

  const [dialogKey, setDialogKey] = useState(0);
  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);
  const [orderBy, setOrderBy] = useState<"email" | "firstName" | "lastName" | "roleName" | "createdAt">("createdAt");
  const [order, setOrder] = useState<"asc" | "desc">("desc");

  const handleSort = (field: "email" | "firstName" | "lastName" | "roleName" | "createdAt") => {
    if (orderBy === field) {
      setOrder((prev) => (prev === "asc" ? "desc" : "asc"));
      return;
    }
    setOrderBy(field);
    setOrder("asc");
  };

  const sortedData = [...(data ?? [])].sort((a, b) => {
    const direction = order === "asc" ? 1 : -1;
    const av = a[orderBy];
    const bv = b[orderBy];
    if (orderBy === "createdAt") {
      return (new Date(av).getTime() - new Date(bv).getTime()) * direction;
    }
    return String(av).localeCompare(String(bv)) * direction;
  });

  const [snackbar, setSnackbar] = useState<{
    open: boolean;
    message: string;
    severity: "success" | "error";
  }>({
    open: false,
    message: "",
    severity: "success",
  });

  if (isLoading) return <CircularProgress />;
  if (isError)
    return (
      <Alert severity="error">
        {(error as Error).message || "Failed to load users."}
      </Alert>
    );

  const handleCreate = () => {
    setDialogKey((key) => key + 1);
    setEditingUser(null);
    setDialogOpen(true);
  };

  const handleEdit = (user: {
    id: string;
    roleId: string;
    firstName: string;
    lastName: string;
    email: string;
  }) => {
    setEditingUser(user);
    setDialogOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this user?")) return;
    try {
      await deleteUser.mutateAsync(id);
      setSnackbar({
        open: true,
        message: "User deleted successfully.",
        severity: "success",
      });
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      setSnackbar({
        open: true,
        message: axiosError.response?.data?.message ?? "Failed to delete user.",
        severity: "error",
      });
    }
  };

  const handleSubmit = async (payload: {
    roleId: string;
    firstName: string;
    lastName: string;
    email: string;
    password?: string;
  }) => {
    try {
      if (editingUser) {
        await updateUser.mutateAsync({ id: editingUser.id, payload });
        setSnackbar({
          open: true,
          message: "User updated successfully.",
          severity: "success",
        });
      } else {
        await createUser.mutateAsync({
          roleId: payload.roleId,
          firstName: payload.firstName,
          lastName: payload.lastName,
          email: payload.email,
          password: payload.password ?? "",
        });
        setSnackbar({
          open: true,
          message: "User created successfully.",
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
        <Typography variant="h6" sx={{ mb: 2 }}>
          Users
        </Typography>
        <Button variant="contained" size="small" onClick={handleCreate}>
          + New User
        </Button>
      </Box>
      <Table size="small">
        <TableHead>
          <TableRow sx={{ bgcolor: "grey.100" }}>
            <TableCell sx={{ fontWeight: 700 }}>
              <TableSortLabel active={orderBy === "email"} direction={orderBy === "email" ? order : "asc"} onClick={() => handleSort("email")}>Email</TableSortLabel>
            </TableCell>
            <TableCell sx={{ fontWeight: 700 }}>
              <TableSortLabel active={orderBy === "firstName"} direction={orderBy === "firstName" ? order : "asc"} onClick={() => handleSort("firstName")}>First Name</TableSortLabel>
            </TableCell>
            <TableCell sx={{ fontWeight: 700 }}>
              <TableSortLabel active={orderBy === "lastName"} direction={orderBy === "lastName" ? order : "asc"} onClick={() => handleSort("lastName")}>Last Name</TableSortLabel>
            </TableCell>
            <TableCell sx={{ fontWeight: 700 }}>
              <TableSortLabel active={orderBy === "roleName"} direction={orderBy === "roleName" ? order : "asc"} onClick={() => handleSort("roleName")}>Role</TableSortLabel>
            </TableCell>
            <TableCell sx={{ fontWeight: 700 }}>
              <TableSortLabel active={orderBy === "createdAt"} direction={orderBy === "createdAt" ? order : "asc"} onClick={() => handleSort("createdAt")}>Created At</TableSortLabel>
            </TableCell>
            <TableCell sx={{ fontWeight: 700 }}>Updated At</TableCell>
            <TableCell align="right" sx={{ fontWeight: 700 }}>Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {sortedData
            .slice(page * rowsPerPage, page * rowsPerPage + rowsPerPage)
            .map((user) => (
            <TableRow key={user.id}>
              <TableCell>{user.email}</TableCell>
              <TableCell>{user.firstName}</TableCell>
              <TableCell>{user.lastName}</TableCell>
              <TableCell>{user.roleName}</TableCell>
              <TableCell>{formatDateTime(user.createdAt)}</TableCell>
              <TableCell>{formatDateTime(user.updatedAt)}</TableCell>
              <TableCell align="right">
                <IconButton size="small" onClick={() => handleEdit(user)}>
                  <Edit fontSize="small" />
                </IconButton>
                <IconButton
                  size="small"
                  color="error"
                  onClick={() => handleDelete(user.id)}
                  disabled={deleteUser.isPending}
                >
                  <Delete fontSize="small" />
                </IconButton>
              </TableCell>
            </TableRow>
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

      <UserDialog
        key={dialogKey}
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        onSubmit={handleSubmit}
        initialData={editingUser}
        roles={roles ?? []}
        isLoading={createUser.isPending || updateUser.isPending}
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
