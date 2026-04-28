import { useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../use-auth";
import { useEffect, useState } from "react";
import { Box, Stack } from "@mui/system";
import {
  Alert,
  Button,
  Chip,
  Divider,
  Paper,
  TextField,
  Typography,
} from "@mui/material";
import type { AxiosError } from "axios";
import { consumeSessionExpired } from "../../../lib/auth";

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const location = useLocation();
  const fromState = location.state as { reason?: string; from?: unknown } | null;
  const [showSessionExpired] = useState(() => {
    const reasonFromState = fromState?.reason === "session-expired";
    const reasonFromQuery = new URLSearchParams(location.search).get("reason");
    return (
      reasonFromState ||
      reasonFromQuery === "session-expired" ||
      consumeSessionExpired()
    );
  });

  useEffect(() => {
    const searchParams = new URLSearchParams(location.search);
    if (searchParams.get("reason") === "session-expired") {
      searchParams.delete("reason");
      const nextSearch = searchParams.toString();
      navigate(
        {
          pathname: location.pathname,
          search: nextSearch ? `?${nextSearch}` : "",
          hash: location.hash,
        },
        { replace: true, state: location.state },
      );
    }
  }, [
    location.hash,
    location.pathname,
    location.search,
    location.state,
    navigate,
  ]);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      await login({ email, password });
      navigate("/", { replace: true });
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      if (axiosError.response?.status === 401) {
        setError(
          axiosError.response.data?.message ?? "Invalid email or password.",
        );
      } else if (!axiosError.response) {
        setError("Cannot connect to the API.");
      } else {
        setError(axiosError.response.data?.message ?? "Sign in failed.");
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
            Welcome back.
          </Typography>
          <Typography sx={{ mt: 1.5, color: "#cbd5e1", maxWidth: 360 }}>
            Sign in to manage boarding houses, rooms, contacts, and media in one place.
          </Typography>
        </Box>
        <Typography variant="body2" sx={{ color: "#94a3b8" }}>
          Secure access for Owners and Admins.
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
          Sign in
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Sign in to your account.
        </Typography>
        <Divider sx={{ mb: 2 }} />
        {showSessionExpired && (
          <Alert severity="info" sx={{ mb: 2 }}>
            Session expired, please sign in again.
          </Alert>
        )}
        {error && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {error}
          </Alert>
        )}
        <form onSubmit={onSubmit}>
          <Stack spacing={2}>
            <TextField
              label="Email"
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
            <TextField
              label="Password"
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
            <Button type="submit" variant="contained" disabled={submitting}>
              {submitting ? "Signing in..." : "Sign in"}
            </Button>
            <Button variant="outlined" onClick={() => navigate("/register")}>
              Don't have an account? Register
            </Button>
          </Stack>
        </form>
      </Paper>
    </Box>
  );
}
