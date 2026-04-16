import { useNavigate } from "react-router-dom";
import { useAuth } from "../use-auth";
import { useState } from "react";
import type { AxiosError } from "axios";
import {
  Alert,
  Box,
  Button,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";

export function RegisterPage() {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError(null);
    try {
      await register({ email, password, firstName, lastName });
      navigate("/", { replace: true });
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      if (!axiosError.response) {
        setError("Cannot connect to the API.");
      } else {
        setError(axiosError.response?.data?.message ?? "Registration failed.");
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Box sx={{ display: "grid", placeItems: "center", minHeight: "70vh" }}>
      <Paper sx={{ p: 4, width: "100%", maxWidth: 420 }} elevation={6}>
        <Typography variant="h5" fontWeight={700} gutterBottom>
          Create Account
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Register as a boarding house owner.
        </Typography>
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}
        <form onSubmit={onSubmit}>
          <Stack spacing={2}>
            <TextField
              label="First Name"
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              required
              fullWidth
              autoFocus
            />
            <TextField
              label="Last Name"
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              required
              fullWidth
            />
            <TextField
              label="Email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              fullWidth
            />
            <TextField
              label="Password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              fullWidth
              helperText="Password must be at least 12 characters long"
            />
            <Button type="submit" variant="contained" disabled={submitting}>
              {submitting ? "Creating account..." : "Create Account"}
            </Button>
            <Button variant="text" onClick={() => navigate("/login")}>
              Already have an account? Sign in
            </Button>
          </Stack>
        </form>
      </Paper>
    </Box>
  );
}
