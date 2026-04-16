import { useState } from "react";
import { Delete, Edit } from "@mui/icons-material";
import {
  Alert,
  Box,
  Button,
  Chip,
  IconButton,
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
  useContactsByBoardingHouseQuery,
  useCreateContact,
  useDeleteContact,
  useUpdateContact,
} from "../queries";
import { ContactDialog } from "../dialogs/contact-dialog";
import type {
  ContactDto,
  CreateContactPayload,
  UpdateContactPayload,
} from "../types";

interface ContactsSectionProps {
  boardingHouseId: string;
}

export function ContactsSection({ boardingHouseId }: ContactsSectionProps) {
  const { data, isLoading, isError, error } =
    useContactsByBoardingHouseQuery(boardingHouseId);
  const createContact = useCreateContact();
  const updateContact = useUpdateContact();
  const deleteContact = useDeleteContact();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingContact, setEditingContact] = useState<ContactDto | null>(null);
  const [snackbar, setSnackbar] = useState<{
    open: boolean;
    message: string;
    severity: "success" | "error";
  }>({ open: false, message: "", severity: "success" });

  const handleSubmit = async (payload: CreateContactPayload | UpdateContactPayload) => {
    try {
      if (editingContact) {
        await updateContact.mutateAsync({
          id: editingContact.id,
          payload: payload as UpdateContactPayload,
        });
        setSnackbar({
          open: true,
          message: "Contact updated successfully.",
          severity: "success",
        });
      } else {
        await createContact.mutateAsync(payload as CreateContactPayload);
        setSnackbar({
          open: true,
          message: "Contact created successfully.",
          severity: "success",
        });
      }
      setDialogOpen(false);
      setEditingContact(null);
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      setSnackbar({
        open: true,
        message: axiosError.response?.data?.message ?? "Contact operation failed.",
        severity: "error",
      });
    }
  };

  const handleDelete = async (contact: ContactDto) => {
    if (!window.confirm("Are you sure you want to delete this contact?")) return;

    try {
      await deleteContact.mutateAsync({
        id: contact.id,
        boardingHouseId,
      });
      setSnackbar({
        open: true,
        message: "Contact deleted successfully.",
        severity: "success",
      });
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      setSnackbar({
        open: true,
        message: axiosError.response?.data?.message ?? "Failed to delete contact.",
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
          Contacts
        </Typography>
        <Button
          size="small"
          variant="outlined"
          onClick={() => {
            setEditingContact(null);
            setDialogOpen(true);
          }}
        >
          + Add Contact
        </Button>
      </Box>

      {isError && (
        <Alert severity="error" sx={{ mb: 1 }}>
          {(error as Error).message || "Failed to load contacts."}
        </Alert>
      )}

      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Phone</TableCell>
            <TableCell>Network</TableCell>
            <TableCell>Primary</TableCell>
            <TableCell>Updated At</TableCell>
            <TableCell align="right">Actions</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {!isLoading && (data ?? []).length === 0 && (
            <TableRow>
              <TableCell colSpan={5}>No contacts yet.</TableCell>
            </TableRow>
          )}
          {(data ?? []).map((contact) => (
            <TableRow key={contact.id}>
              <TableCell>{contact.phoneNumber}</TableCell>
              <TableCell>{contact.network.toUpperCase()}</TableCell>
              <TableCell>
                {contact.isPrimary ? (
                  <Chip size="small" color="success" label="Primary" />
                ) : (
                  "-"
                )}
              </TableCell>
              <TableCell>
                {contact.updatedAt ? formatDateTime(contact.updatedAt) : "-"}
              </TableCell>
              <TableCell align="right">
                <IconButton
                  size="small"
                  onClick={() => {
                    setEditingContact(contact);
                    setDialogOpen(true);
                  }}
                >
                  <Edit fontSize="small" />
                </IconButton>
                <IconButton
                  size="small"
                  color="error"
                  disabled={deleteContact.isPending}
                  onClick={() => handleDelete(contact)}
                >
                  <Delete fontSize="small" />
                </IconButton>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <ContactDialog
        open={dialogOpen}
        onClose={() => {
          setDialogOpen(false);
          setEditingContact(null);
        }}
        onSubmit={handleSubmit}
        boardingHouseId={boardingHouseId}
        initialData={
          editingContact
            ? {
                phoneNumber: editingContact.phoneNumber,
                network: editingContact.network,
                isPrimary: editingContact.isPrimary,
              }
            : null
        }
        isLoading={createContact.isPending || updateContact.isPending}
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
