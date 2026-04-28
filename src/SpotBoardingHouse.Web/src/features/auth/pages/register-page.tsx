import { useNavigate } from "react-router-dom";
import { useAuth } from "../use-auth";
import { useState } from "react";
import type { AxiosError } from "axios";
import {
  Alert,
  Box,
  Button,
  Chip,
  Divider,
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
    <Box
      sx={{
        minHeight: "72vh",
        display: "grid",
        gridTemplateColumns: { xs: "1fr", md: "1.1fr 1fr" },
        gap: 0,
        borderRadius: 3,
        overflow: "hidden",
        border: "1px solid",
        borderColor: "divider",
        bgcolor: "background.paper",
      }}
    >
      <Box
        sx={{
          display: { xs: "none", md: "flex" },
          flexDirection: "column",
          justifyContent: "space-between",
          p: 4,
          background:
            "linear-gradient(140deg, #0f172a 0%, #1e293b 45%, #0b3a4b 100%)",
          color: "#e2e8f0",
        }}
      >
        <Box>
          <Chip
            size="small"
            label="SpotBoardingHouse"
            sx={{
              bgcolor: "rgba(148,163,184,0.2)",
              color: "#e2e8f0",
              fontWeight: 600,
            }}
          />
          <Typography variant="h4" fontWeight={800} sx={{ mt: 2, lineHeight: 1.2 }}>
            Create your account.
          </Typography>
          <Typography sx={{ mt: 1.5, color: "#cbd5e1", maxWidth: 360 }}>
            Register to start managing boarding houses and rooms. Admins and Owners
            can both sign in from the same access page.
          </Typography>
        </Box>
        <Typography variant="body2" sx={{ color: "#94a3b8" }}>
          Owner registration with admin-ready sign-in flow.
        </Typography>
      </Box>

      <Paper
        sx={{
          p: { xs: 2.5, md: 4 },
          width: "100%",
          borderRadius: 0,
          border: 0,
          display: "flex",
          flexDirection: "column",
          justifyContent: "center",
        }}
        elevation={0}
      >
        <Typography variant="h5" fontWeight={700} gutterBottom>
          Create Account
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Register as a boarding house owner.
        </Typography>
        <Divider sx={{ mb: 2 }} />
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
            <Button variant="outlined" onClick={() => navigate("/login")}>
              Already have an account? Sign in
            </Button>
          </Stack>
        </form>
      </Paper>
    </Box>
  );
}
