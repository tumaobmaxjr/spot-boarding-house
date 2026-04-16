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

interface UserDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (payload: {
    roleId: string;
    firstName: string;
    lastName: string;
    email: string;
    password?: string;
  }) => void;
  initialData?: {
    roleId: string;
    firstName: string;
    lastName: string;
    email: string;
  } | null;
  roles: { id: string; name: string }[];
  isLoading?: boolean;
}

export function UserDialog({
  open,
  onClose,
  onSubmit,
  initialData,
  roles,
  isLoading,
}: UserDialogProps) {
  const isEdit = !!initialData;
  const [roleId, setRoleId] = useState(initialData?.roleId ?? "");
  const [firstName, setFirstName] = useState(initialData?.firstName ?? "");
  const [lastName, setLastName] = useState(initialData?.lastName ?? "");
  const [email, setEmail] = useState(initialData?.email ?? "");
  const [password, setPassword] = useState("");
  const [errors, setErrors] = useState<Record<string, string>>({});

  const validate = (): boolean => {
    const errs: Record<string, string> = {};
    if (!roleId) errs.roleId = "Role is required";
    if (!firstName.trim()) errs.firstName = "First name is required";
    if (!lastName.trim()) errs.lastName = "Last name is required";
    if (!email.trim()) errs.email = "Email is required";
    if (!isEdit && !password) errs.password = "Password is required";
    if (password && password.length < 12)
      errs.password = "Password must be at least 12 characters";
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;
    onSubmit({
      roleId,
      firstName: firstName.trim(),
      lastName: lastName.trim(),
      email: email.trim(),
      password: password || undefined,
    });
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>{isEdit ? "Edit User" : "Add User"}</DialogTitle>
        <DialogContent
          sx={{
            display: "flex",
            flexDirection: "column",
            gap: 2,
            pt: 2,
            overflow: "visible",
          }}
        >
          <FormControl error={!!errors.roleId} fullWidth>
            <InputLabel id="role-label">Role</InputLabel>
            <Select
              labelId="role-label"
              label="Role"
              value={roleId}
              onChange={(e) => setRoleId(e.target.value)}
            >
              {roles.map((role) => (
                <MenuItem key={role.id} value={role.id}>
                  {role.name}
                </MenuItem>
              ))}
            </Select>
            {errors.roleId && <FormHelperText>{errors.roleId}</FormHelperText>}
          </FormControl>

          <TextField
            label="First Name"
            value={firstName}
            onChange={(e) => setFirstName(e.target.value)}
            error={!!errors.firstName}
            helperText={errors.firstName}
            required
            fullWidth
            autoFocus
          />

          <TextField
            label="Email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            error={!!errors.email}
            helperText={errors.email}
            required
            fullWidth
          />

          <TextField
            label="Last Name"
            value={lastName}
            onChange={(e) => setLastName(e.target.value)}
            error={!!errors.lastName}
            helperText={errors.lastName}
            required
            fullWidth
          />

          <TextField
            label={
              isEdit ? "New Password (leave blank to keep current)" : "Password"
            }
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            error={!!errors.password}
            helperText={
              errors.password || (!isEdit ? "Minimum 12 characters" : "")
            }
            required={!isEdit}
            fullWidth
          />
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
