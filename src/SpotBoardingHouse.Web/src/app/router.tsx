import {
  Navigate,
  Outlet,
  Route,
  Routes,
  useLocation,
  useNavigate,
} from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import {
  Alert,
  AppBar,
  Avatar,
  Box,
  Container,
  CssBaseline,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Typography,
} from "@mui/material";
import MenuIcon from "@mui/icons-material/Menu";
import DashboardIcon from "@mui/icons-material/Dashboard";
import HomeWorkIcon from "@mui/icons-material/HomeWork";
import MeetingRoomIcon from "@mui/icons-material/MeetingRoom";
import PeopleIcon from "@mui/icons-material/People";
import AdminPanelSettingsIcon from "@mui/icons-material/AdminPanelSettings";
import LoginIcon from "@mui/icons-material/Login";
import PersonAddIcon from "@mui/icons-material/PersonAdd";
import LogoutIcon from "@mui/icons-material/Logout";
import { useAuth } from "../features/auth/use-auth";
import { UsersPage } from "../features/users/pages/users-page";
import { LoginPage } from "../features/auth/pages/login-page";
import { RegisterPage } from "../features/auth/pages/register-page";
import { RolesPage } from "../features/roles/pages/roles-page";
import { getToken, isTokenExpired } from "../lib/auth";
import { BoardingHousesPage } from "../features/boardinghouses/pages/boarding-house-page";
import { RoomsPage } from "../features/rooms/pages/rooms-page";

type NavItem = {
  label: string;
  path: string;
  icon: React.ReactNode;
  section: "general" | "management" | "account";
};

const dashboardTheme = {
  pageBg: "#f3f6fb",
  sidebarBg: "#101828",
  sidebarText: "#d1d5db",
  sidebarMuted: "#9ca3af",
  sidebarSelected: "#1f2937",
  sidebarHover: "#1a2232",
};

const routeMeta: Record<string, { title: string; crumb: string }> = {
  "/": { title: "Dashboard", crumb: "Overview" },
  "/admin/users": { title: "Users", crumb: "Admin / Users" },
  "/admin/roles": { title: "Roles", crumb: "Admin / Roles" },
  "/boarding-houses": {
    title: "Boarding Houses",
    crumb: "Management / Boarding Houses",
  },
  "/rooms": { title: "Rooms", crumb: "Management / Rooms" },
  "/login": { title: "Sign In", crumb: "Account / Sign In" },
  "/register": { title: "Register", crumb: "Account / Register" },
};

function Shell({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, role, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [mobileOpen, setMobileOpen] = useState(false);
  const drawerWidth = 280;
  const showDashboardShell = isAuthenticated && (role === "Owner" || role === "Admin");

  const navItems = useMemo(() => {
    const items: NavItem[] = [];

    items.push({
      label: "Dashboard",
      path: "/",
      icon: <DashboardIcon fontSize="small" />,
      section: "general",
    });

    if (isAuthenticated && role === "Admin") {
      items.push({
        label: "Users",
        path: "/admin/users",
        icon: <PeopleIcon fontSize="small" />,
        section: "management",
      });
      items.push({
        label: "Roles",
        path: "/admin/roles",
        icon: <AdminPanelSettingsIcon fontSize="small" />,
        section: "management",
      });
    }

    if (isAuthenticated && (role === "Owner" || role === "Admin")) {
      items.push({
        label: "Boarding Houses",
        path: "/boarding-houses",
        icon: <HomeWorkIcon fontSize="small" />,
        section: "management",
      });
      items.push({
        label: "Rooms",
        path: "/rooms",
        icon: <MeetingRoomIcon fontSize="small" />,
        section: "management",
      });
    }

    if (!isAuthenticated) {
      items.push({
        label: "Sign In",
        path: "/login",
        icon: <LoginIcon fontSize="small" />,
        section: "account",
      });
      items.push({
        label: "Register",
        path: "/register",
        icon: <PersonAddIcon fontSize="small" />,
        section: "account",
      });
    }

    return items;
  }, [isAuthenticated, role]);

  const navButtonSx = {
    borderRadius: 2,
    mb: 0.5,
    minHeight: 44,
    px: 1.5,
  };

  const currentMeta = routeMeta[location.pathname] ?? {
    title: "Dashboard",
    crumb: "Overview",
  };
  const generalItems = navItems.filter((i) => i.section === "general");
  const managementItems = navItems.filter((i) => i.section === "management");
  const accountItems = navItems.filter((i) => i.section === "account");

  const drawerContent = (
    <Box
      sx={{
        width: drawerWidth,
        height: "100%",
        display: "flex",
        flexDirection: "column",
        bgcolor: dashboardTheme.sidebarBg,
        color: "#e5e7eb",
      }}
    >
      <Box sx={{ px: 2.5, py: 2.5 }}>
        <Typography variant="h6" fontWeight={800}>
          SpotBoardingHouse
        </Typography>
        <Typography variant="body2" sx={{ color: dashboardTheme.sidebarMuted }}>
          CRUD Dashboard
        </Typography>
      </Box>
      <Divider sx={{ borderColor: "#1f2937" }} />
      <List sx={{ px: 1.25, py: 1.25, flex: 1 }}>
        <Typography variant="overline" sx={{ px: 1.5, color: dashboardTheme.sidebarMuted }}>
          General
        </Typography>
        {generalItems.map((item) => (
          <ListItemButton
            key={item.path}
            selected={location.pathname === item.path}
            onClick={() => {
              navigate(item.path);
              setMobileOpen(false);
            }}
            sx={{
              ...navButtonSx,
              color: dashboardTheme.sidebarText,
              "&.Mui-selected": {
                bgcolor: dashboardTheme.sidebarSelected,
                color: "#fff",
              },
              "&.Mui-selected:hover": {
                bgcolor: "#263244",
              },
              "&:hover": {
                bgcolor: dashboardTheme.sidebarHover,
              },
            }}
          >
            <ListItemIcon sx={{ minWidth: 34, color: "inherit" }}>{item.icon}</ListItemIcon>
            <ListItemText primary={item.label} />
          </ListItemButton>
        ))}

        {managementItems.length > 0 && (
          <>
            <Typography
              variant="overline"
              sx={{ px: 1.5, mt: 1.25, color: dashboardTheme.sidebarMuted }}
            >
              Management
            </Typography>
            {managementItems.map((item) => (
              <ListItemButton
                key={item.path}
                selected={location.pathname === item.path}
                onClick={() => {
                  navigate(item.path);
                  setMobileOpen(false);
                }}
                sx={{
                  ...navButtonSx,
                  color: dashboardTheme.sidebarText,
                  "&.Mui-selected": {
                    bgcolor: dashboardTheme.sidebarSelected,
                    color: "#fff",
                  },
                  "&.Mui-selected:hover": {
                    bgcolor: "#263244",
                  },
                  "&:hover": {
                    bgcolor: dashboardTheme.sidebarHover,
                  },
                }}
              >
                <ListItemIcon sx={{ minWidth: 34, color: "inherit" }}>{item.icon}</ListItemIcon>
                <ListItemText primary={item.label} />
              </ListItemButton>
            ))}
          </>
        )}

        {accountItems.length > 0 && (
          <>
            <Typography
              variant="overline"
              sx={{ px: 1.5, mt: 1.25, color: dashboardTheme.sidebarMuted }}
            >
              Account
            </Typography>
            {accountItems.map((item) => (
              <ListItemButton
                key={item.path}
                selected={location.pathname === item.path}
                onClick={() => {
                  navigate(item.path);
                  setMobileOpen(false);
                }}
                sx={{
                  ...navButtonSx,
                  color: dashboardTheme.sidebarText,
                  "&.Mui-selected": {
                    bgcolor: dashboardTheme.sidebarSelected,
                    color: "#fff",
                  },
                  "&.Mui-selected:hover": {
                    bgcolor: "#263244",
                  },
                  "&:hover": {
                    bgcolor: dashboardTheme.sidebarHover,
                  },
                }}
              >
                <ListItemIcon sx={{ minWidth: 34, color: "inherit" }}>{item.icon}</ListItemIcon>
                <ListItemText primary={item.label} />
              </ListItemButton>
            ))}
          </>
        )}
      </List>
      {isAuthenticated && (
        <Box sx={{ p: 1.25 }}>
          <ListItemButton
            sx={{
              ...navButtonSx,
              color: "#fecaca",
              "&:hover": { bgcolor: "#3b1f28" },
            }}
            onClick={() => {
              logout();
              navigate("/login", { replace: true });
              setMobileOpen(false);
            }}
          >
            <ListItemIcon sx={{ minWidth: 34, color: "inherit" }}>
              <LogoutIcon fontSize="small" />
            </ListItemIcon>
            <ListItemText primary="Logout" />
          </ListItemButton>
        </Box>
      )}
    </Box>
  );

  if (!showDashboardShell) {
    return (
      <Box sx={{ minHeight: "100vh", bgcolor: dashboardTheme.pageBg }}>
        <CssBaseline />
        <Container sx={{ py: 4 }}>{children}</Container>
      </Box>
    );
  }

  return (
    <Box sx={{ display: "flex", minHeight: "100vh", bgcolor: dashboardTheme.pageBg }}>
      <CssBaseline />
      <AppBar
        position="fixed"
        color="inherit"
        elevation={0}
        sx={{
          borderBottom: 1,
          borderColor: "divider",
          ml: { md: `${drawerWidth}px` },
          width: { md: `calc(100% - ${drawerWidth}px)` },
          backdropFilter: "blur(8px)",
          bgcolor: "rgba(255,255,255,0.9)",
        }}
      >
        <Toolbar sx={{ gap: 1.5 }}>
          <IconButton
            edge="start"
            color="inherit"
            aria-label="open drawer"
            onClick={() => setMobileOpen((v) => !v)}
            sx={{ mr: 1, display: { md: "none" } }}
          >
            <MenuIcon />
          </IconButton>
          <Box sx={{ flex: 1 }}>
            <Typography variant="h6" fontWeight={700}>
              {currentMeta.title}
            </Typography>
            <Typography variant="caption" color="text.secondary">
              {currentMeta.crumb}
            </Typography>
          </Box>
          <Avatar sx={{ width: 32, height: 32 }}>{role?.[0] ?? "G"}</Avatar>
          <Typography variant="body2" color="text.secondary" sx={{ display: { xs: "none", sm: "block" } }}>
            {isAuthenticated ? `Signed in as ${role}` : "Guest mode"}
          </Typography>
        </Toolbar>
      </AppBar>

      <Box component="nav" sx={{ width: { md: drawerWidth }, flexShrink: { md: 0 } }}>
        <Drawer
          variant="temporary"
          open={mobileOpen}
          onClose={() => setMobileOpen(false)}
          ModalProps={{ keepMounted: true }}
          sx={{
            display: { xs: "block", md: "none" },
            "& .MuiDrawer-paper": { width: drawerWidth, boxSizing: "border-box" },
          }}
        >
          {drawerContent}
        </Drawer>
        <Drawer
          variant="permanent"
          sx={{
            display: { xs: "none", md: "block" },
            "& .MuiDrawer-paper": {
              width: drawerWidth,
              boxSizing: "border-box",
              borderRight: 1,
              borderColor: "divider",
            },
          }}
          open
        >
          {drawerContent}
        </Drawer>
      </Box>

      <Box component="main" sx={{ flexGrow: 1, width: { md: `calc(100% - ${drawerWidth}px)` } }}>
        <Toolbar />
        <Container
          maxWidth={false}
          sx={{
            py: 3,
            px: { xs: 1.5, sm: 2, md: 3 },
          }}
        >
          {children}
        </Container>
      </Box>
    </Box>
  );
}

function GuestOnlyRoute() {
  const { isAuthenticated } = useAuth();

  if (isAuthenticated) return <Navigate to="/" replace />;
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

function ProtectedOwnerRoute() {
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
  if (role !== "Owner" && role !== "Admin")
    return <Navigate to="/forbidden" replace />;
  return <Outlet />;
}

function HomeRedirect() {
  const { role } = useAuth();
  if (role === "Owner") return <Navigate to="/boarding-houses" replace />;
  return <Navigate to="/admin/users" replace />;
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

        <Route element={<ProtectedOwnerRoute />}>
          <Route path="/boarding-houses" element={<BoardingHousesPage />} />
          <Route path="/rooms" element={<RoomsPage />} />
        </Route>

        <Route path="/forbidden" element={<ForbiddenPage />} />
        <Route path="/" element={<HomeRedirect />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Shell>
  );
}
