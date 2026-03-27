import { useNavigate } from "react-router-dom";
import { useAuth } from "./use-auth";
import { useState } from "react";
import { Box, Stack } from "@mui/system";
import { Alert, Button, Paper, TextField, Typography } from "@mui/material";
import type { AxiosError } from "axios";

export function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState("admin@spotboardinghouse.com");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      await login({ email, password });
      navigate("/admin/users", { replace: true });
    } catch (err) {
      const axiosError = err as AxiosError<{ message?: string }>;
      if (axiosError.response?.status === 401) {
        setError(
          axiosError.response.data?.message ?? "Invalid email or password.",
        );
      } else if (!axiosError.response) {
        setError("Cannot connect to the API.");
      } else {
        setError(axiosError.response.data?.message ?? "Login failed.");
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
          Use your admin account.
        </Typography>
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
          </Stack>
        </form>
      </Paper>
    </Box>
  );
}
