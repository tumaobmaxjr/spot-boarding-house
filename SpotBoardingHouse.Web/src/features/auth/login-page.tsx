import { useLocation, useNavigate, type To } from "react-router-dom";
import { useAuth } from "./use-auth";
import { useEffect, useState } from "react";
import { Box, Stack } from "@mui/system";
import { Alert, Button, Paper, TextField, Typography } from "@mui/material";
import type { AxiosError } from "axios";
import { consumeSessionExpired } from "../../lib/auth";

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const location = useLocation();
  const fromState = location.state as { reason?: string; from?: To } | null;
  const [showSessionExpired] = useState(() => {
    const reasonFromState = fromState?.reason === "session-expired";
    const reasonFromQuery = new URLSearchParams(location.search).get("reason");
    return (
      reasonFromState ||
      reasonFromQuery === "session-expired" ||
      consumeSessionExpired()
    );
  });
  const from: To = fromState?.from ?? "/";

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
      navigate(from, { replace: true });
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
    <Box sx={{ display: "grid", placeItems: "center", minHeight: "70vh" }}>
      <Paper sx={{ p: 4, width: "100%", maxWidth: 420 }} elevation={6}>
        <Typography variant="h5" fontWeight={700} gutterBottom>
          Sign in
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Sign in to your account.
        </Typography>
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
            <Button variant="text" onClick={() => navigate("/register")}>
              Don't have an account? Register
            </Button>
          </Stack>
        </form>
      </Paper>
    </Box>
  );
}
