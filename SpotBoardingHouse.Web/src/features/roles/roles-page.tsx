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
import {
  useCreateRole,
  useDeleteRole,
  useRolesQuery,
  useUpdateRole,
} from "./queries";
import { useState } from "react";
import { RoleDialog } from "./role-dialog";
import { Edit, Delete } from "@mui/icons-material";

function formatDateTime(value: string | null) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return "-";

  return new Intl.DateTimeFormat(undefined, {
    year: "numeric",
    month: "numeric",
    day: "numeric",
    hour: "numeric",
    minute: "numeric",
    second: "numeric",
  }).format(date);
}

export function RolesPage() {
  const { data, isLoading, isError, error } = useRolesQuery();
  const createRole = useCreateRole();
  const updateRole = useUpdateRole();
  const deleteRole = useDeleteRole();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingRole, setEditingRole] = useState<{
    id: string;
    name: string;
    description: string | null;
  } | null>(null);

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
        {(error as Error).message || "Failed to load roles."}
      </Alert>
    );

  const handleCreate = () => {
    setEditingRole(null);
    setDialogOpen(true);
  };

  const handleEdit = (role: {
    id: string;
    name: string;
    description: string | null;
  }) => {
    setEditingRole(role);
    setDialogOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm("Are you sure you want to delete this role?")) return;
    try {
      await deleteRole.mutateAsync(id);
      setSnackbar({
        open: true,
        message: "Role deleted successfully.",
        severity: "success",
      });
    } catch {
      setSnackbar({
        open: true,
        message: "Failed to delete role.",
        severity: "error",
      });
    }
  };

  const handleSubmit = async (payload: {
    name: string;
    description?: string;
  }) => {
    try {
      if (editingRole) {
        await updateRole.mutateAsync({
          id: editingRole.id,
          payload: {
            name: payload.name,
            description: payload.description,
          },
        });
        setSnackbar({
          open: true,
          message: "Role updated successfully.",
          severity: "success",
        });
      } else {
        await createRole.mutateAsync(payload);
        setSnackbar({
          open: true,
          message: "Role created successfully.",
          severity: "success",
        });
      }
      setDialogOpen(false);
    } catch {
      setSnackbar({
        open: true,
        message: "Operation failed.",
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
          Roles
        </Typography>
        <Button variant="contained" size="small" onClick={handleCreate}>
          + New Role
        </Button>
      </Box>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Name</TableCell>
            <TableCell>Description</TableCell>
            <TableCell>Created At</TableCell>
            <TableCell>Updated At</TableCell>
            <TableCell align="right">Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {(data ?? []).map((role) => (
            <TableRow key={role.id}>
              <TableCell>{role.name}</TableCell>
              <TableCell>{role.description ?? "-"}</TableCell>
              <TableCell>{formatDateTime(role.createdAt)}</TableCell>
              <TableCell>{formatDateTime(role.updatedAt)}</TableCell>
              <TableCell align="right">
                <IconButton size="small" onClick={() => handleEdit(role)}>
                  <Edit fontSize="small" />
                </IconButton>
                <IconButton
                  size="small"
                  color="error"
                  onClick={() => handleDelete(role.id)}
                  disabled={deleteRole.isPending}
                >
                  <Delete fontSize="small" />
                </IconButton>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <RoleDialog
        key={editingRole?.id ?? "new"}
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        onSubmit={handleSubmit}
        initialData={editingRole}
        isLoading={createRole.isPending || updateRole.isPending}
      />

      <Snackbar
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
