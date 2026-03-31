import {
  Navigate,
  Outlet,
  Route,
  Routes,
  useLocation,
  useNavigate,
} from "react-router-dom";
import { useEffect } from "react";
import {
  Alert,
  Box,
  Button,
  Container,
  Stack,
  Typography,
} from "@mui/material";
import { useAuth } from "../features/auth/use-auth";
import { UsersPage } from "../features/users/users-page";
import { LoginPage } from "../features/auth/login-page";
import { RegisterPage } from "../features/auth/register-page";
import { RolesPage } from "../features/roles/roles-page";
import { getToken, isTokenExpired } from "../lib/auth";

function Shell({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, role, logout } = useAuth();
  const navigate = useNavigate();
  return (
    <Box sx={{ minHeight: "100vh" }}>
      <Container sx={{ py: 4 }}>
        <Stack
          direction="row"
          justifyContent="space-between"
          alignItems="center"
          sx={{ mb: 3 }}
        >
          <Typography variant="h5" fontWeight={700}>
            SpotBoardingHouse Web
          </Typography>
          <Stack direction="row" spacing={1}>
            {isAuthenticated && role === "Admin" && (
              <>
                <Button onClick={() => navigate("/admin/users")}>Users</Button>
                <Button onClick={() => navigate("/admin/roles")}>Roles</Button>
              </>
            )}
            {isAuthenticated ? (
              <Button
                color="error"
                variant="outlined"
                onClick={() => {
                  logout();
                  navigate("/login", { replace: true });
                }}
              >
                Logout
              </Button>
            ) : (
              <Button variant="contained" onClick={() => navigate("/login")}>
                Sign in
              </Button>
            )}
          </Stack>
        </Stack>
        {children}
      </Container>
    </Box>
  );
}

function GuestOnlyRoute() {
  const { isAuthenticated } = useAuth();
  const location = useLocation();

  const from = (location.state as { from?: { pathname?: string } } | null)?.from
    ?.pathname;

  if (isAuthenticated) return <Navigate to={from || "/"} replace />;
  return <Outlet />;
}

function ProtectedAdminRoute() {
  const { isAuthenticated, role, logout } = useAuth();
  const location = useLocation();
  const token = getToken();
  const hasExpiredToken = !!token && isTokenExpired(token);

  useEffect(() => {
    if (hasExpiredToken) {
      logout(true);
    }
  }, [hasExpiredToken, logout]);

  if (hasExpiredToken) {
    const from = {
      pathname: location.pathname,
      search: location.search,
      hash: location.hash,
    };
    return (
      <Navigate
        to="/login?reason=session-expired"
        replace
        state={{ from, reason: "session-expired" }}
      />
    );
  }

  if (!isAuthenticated)
    return (
      <Navigate
        to="/login"
        replace
        state={{
          from: {
            pathname: location.pathname,
            search: location.search,
            hash: location.hash,
          },
        }}
      />
    );
  if (role !== "Admin") return <Navigate to="/forbidden" replace />;

  return <Outlet />;
}

function ForbiddenPage() {
  return <Alert severity="warning">Access denied.</Alert>;
}

export function AppRouter() {
  return (
    <Shell>
      <Routes>
        <Route element={<GuestOnlyRoute />}>
          <Route path="/login" element={<LoginPage />} />
          <Route path="/register" element={<RegisterPage />} />
        </Route>

        <Route element={<ProtectedAdminRoute />}>
          <Route path="/admin/users" element={<UsersPage />} />
          <Route path="/admin/roles" element={<RolesPage />} />
        </Route>

        <Route path="/forbidden" element={<ForbiddenPage />} />
        <Route path="/" element={<Navigate to="/admin/users" replace />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Shell>
  );
}
