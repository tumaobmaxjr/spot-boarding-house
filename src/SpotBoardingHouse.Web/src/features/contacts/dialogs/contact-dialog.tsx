import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  FormControl,
  FormHelperText,
  InputLabel,
  MenuItem,
  Select,
  TextField,
} from "@mui/material";
import { useState } from "react";
import type { CreateContactPayload, UpdateContactPayload } from "../types";

interface ContactDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (payload: CreateContactPayload | UpdateContactPayload) => void;
  boardingHouseId: string;
  initialData?: {
    phoneNumber: string;
    network: string;
    isPrimary: boolean;
  } | null;
  isLoading?: boolean;
}

const NETWORKS = ["globe", "tm", "dito", "smart", "tnt"];

export function ContactDialog({
  open,
  onClose,
  onSubmit,
  boardingHouseId,
  initialData,
  isLoading,
}: ContactDialogProps) {
  const isEdit = !!initialData;
  const [phoneNumber, setPhoneNumber] = useState(initialData?.phoneNumber ?? "");
  const [network, setNetwork] = useState(initialData?.network ?? "globe");
  const [isPrimary, setIsPrimary] = useState(initialData?.isPrimary ?? false);
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = () => {
    const nextErrors: Record<string, string> = {};
    if (!phoneNumber.trim()) nextErrors.phoneNumber = "Phone number is required";
    if (phoneNumber.trim().length > 20) {
      nextErrors.phoneNumber = "Phone number must be 20 characters or less";
    }
    if (!network) nextErrors.network = "Network is required";
    setErrors(nextErrors);
    return Object.keys(nextErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;

    if (isEdit) {
      onSubmit({
        phoneNumber: phoneNumber.trim(),
        network,
        isPrimary,
      });
      return;
    }

    onSubmit({
      boardingHouseId,
      phoneNumber: phoneNumber.trim(),
      network,
      isPrimary,
    });
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>{isEdit ? "Edit Contact" : "Add Contact"}</DialogTitle>
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
            label="Phone Number"
            value={phoneNumber}
            onChange={(e) => setPhoneNumber(e.target.value)}
            error={!!errors.phoneNumber}
            helperText={errors.phoneNumber}
            required
            fullWidth
          />

          <FormControl error={!!errors.network} fullWidth>
            <InputLabel id="contact-network-label">Network</InputLabel>
            <Select
              labelId="contact-network-label"
              label="Network"
              value={network}
              onChange={(e) => setNetwork(e.target.value)}
            >
              {NETWORKS.map((value) => (
                <MenuItem key={value} value={value}>
                  {value.toUpperCase()}
                </MenuItem>
              ))}
            </Select>
            {errors.network && <FormHelperText>{errors.network}</FormHelperText>}
          </FormControl>

          <FormControl fullWidth>
            <InputLabel id="contact-primary-label">Primary</InputLabel>
            <Select
              labelId="contact-primary-label"
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
