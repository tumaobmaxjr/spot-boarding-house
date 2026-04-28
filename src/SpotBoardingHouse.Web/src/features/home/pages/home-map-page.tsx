import { useEffect, useMemo, useState } from "react";
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  IconButton,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import NearMeIcon from "@mui/icons-material/NearMe";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import ChevronLeftIcon from "@mui/icons-material/ChevronLeft";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import WifiIcon from "@mui/icons-material/Wifi";
import AcUnitIcon from "@mui/icons-material/AcUnit";
import LocalLaundryServiceIcon from "@mui/icons-material/LocalLaundryService";
import SecurityIcon from "@mui/icons-material/Security";
import { useNavigate } from "react-router-dom";
import {
  MapContainer,
  Marker,
  Polyline,
  Popup,
  TileLayer,
  Tooltip,
  useMap,
} from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";
import markerIcon2x from "leaflet/dist/images/marker-icon-2x.png";
import markerIcon from "leaflet/dist/images/marker-icon.png";
import markerShadow from "leaflet/dist/images/marker-shadow.png";
import { useAuth } from "../../auth/use-auth";
import {
  usePublicBoardingHouseDetailQuery,
  usePublicBoardingHousesQuery,
} from "../queries";
import type { PublicBoardingHouseDto } from "../types";
import { resolveMediaUrl } from "../../media/url";

const fallbackCenter: [number, number] = [14.5995, 120.9842];

const defaultIcon = L.icon({
  iconUrl: markerIcon,
  iconRetinaUrl: markerIcon2x,
  shadowUrl: markerShadow,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
});

const userLocationIcon = L.divIcon({
  className: "user-location-marker",
  html: `<div style="width:18px;height:18px;border-radius:999px;background:#2563eb;border:3px solid #fff;box-shadow:0 0 0 4px rgba(37,99,235,0.25)"></div>`,
  iconSize: [18, 18],
  iconAnchor: [9, 9],
});

L.Marker.prototype.options.icon = defaultIcon;

function Recenter({ center }: { center: [number, number] }) {
  const map = useMap();
  useEffect(() => {
    map.setView(center);
  }, [center, map]);
  return null;
}

function getAmenityIcon(name: string) {
  const key = name.toLowerCase();
  if (key.includes("wifi")) return <WifiIcon fontSize="small" />;
  if (key.includes("air") || key.includes("ac")) return <AcUnitIcon fontSize="small" />;
  if (key.includes("laundry")) return <LocalLaundryServiceIcon fontSize="small" />;
  if (key.includes("security")) return <SecurityIcon fontSize="small" />;
  return <WifiIcon fontSize="small" />;
}

export function HomeMapPage() {
  const navigate = useNavigate();
  const { isAuthenticated, role, logout } = useAuth();
  const { data, isLoading, isError, error } = usePublicBoardingHousesQuery();

  const [userLocation, setUserLocation] = useState<[number, number] | null>(null);
  const [selected, setSelected] = useState<PublicBoardingHouseDto | null>(null);
  const [search, setSearch] = useState("");
  const [geoMessage, setGeoMessage] = useState<string | null>(null);
  const [routePoints, setRoutePoints] = useState<[number, number][]>([]);
  const [locating, setLocating] = useState(false);
  const [selectedImageIndex, setSelectedImageIndex] = useState(0);

  const detailQuery = usePublicBoardingHouseDetailQuery(selected?.id);

  const requestPreciseLocation = () => {
    if (!navigator.geolocation) {
      setGeoMessage("Geolocation is not supported by this browser.");
      return;
    }
    setLocating(true);
    navigator.geolocation.getCurrentPosition(
      (pos) => {
        setUserLocation([pos.coords.latitude, pos.coords.longitude]);
        setGeoMessage(null);
        setLocating(false);
      },
      () => {
        setGeoMessage("Location permission denied. Showing map without your location.");
        setLocating(false);
      },
      { enableHighAccuracy: true, timeout: 10000, maximumAge: 0 },
    );
  };

  useEffect(() => {
    requestPreciseLocation();
  }, []);

  useEffect(() => {
    setSelectedImageIndex(0);
  }, [selected?.id]);

  useEffect(() => {
    const run = async () => {
      if (!userLocation || !selected) {
        setRoutePoints([]);
        return;
      }
      try {
        const from = `${userLocation[1]},${userLocation[0]}`;
        const to = `${selected.longitude},${selected.latitude}`;
        const response = await fetch(
          `https://router.project-osrm.org/route/v1/driving/${from};${to}?overview=full&geometries=geojson`,
        );
        if (!response.ok) throw new Error("route");
        const json = await response.json();
        const route = json?.routes?.[0];
        const coords =
          route?.geometry?.coordinates?.map(
            (c: [number, number]) => [c[1], c[0]] as [number, number],
          ) ?? [];
        setRoutePoints(
          coords.length > 0 ? coords : [userLocation, [selected.latitude, selected.longitude]],
        );
      } catch {
        setRoutePoints([userLocation, [selected.latitude, selected.longitude]]);
      }
    };
    void run();
  }, [selected, userLocation]);

  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    return (data ?? []).filter((bh) => {
      return (
        !q ||
        bh.name.toLowerCase().includes(q) ||
        bh.address.toLowerCase().includes(q) ||
        (bh.description ?? "").toLowerCase().includes(q)
      );
    });
  }, [data, search]);

  const center = useMemo<[number, number]>(() => {
    if (selected) return [selected.latitude, selected.longitude];
    if (userLocation) return userLocation;
    return fallbackCenter;
  }, [selected, userLocation]);

  const mediaUrls = useMemo(() => {
    if (!detailQuery.data) return [];
    return detailQuery.data.mediaUrls.map((url) => resolveMediaUrl(url)).filter(Boolean);
  }, [detailQuery.data]);

  const heroImage =
    mediaUrls[selectedImageIndex] ??
    (detailQuery.data?.primaryImageUrl
      ? resolveMediaUrl(detailQuery.data.primaryImageUrl)
      : undefined);

  if (isLoading) {
    return (
      <Box sx={{ minHeight: "100dvh", display: "grid", placeItems: "center" }}>
        <CircularProgress />
      </Box>
    );
  }

  if (isError) {
    return <Alert severity="error">{(error as Error).message}</Alert>;
  }

  return (
    <Box sx={{ position: "relative", height: "100dvh", width: "100%", overflow: "hidden" }}>
      <MapContainer center={center} zoom={13} style={{ height: "100%", width: "100%" }}>
        <TileLayer
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
          attribution="&copy; OpenStreetMap contributors"
        />
        <Recenter center={center} />

        {userLocation && (
          <Marker position={userLocation} icon={userLocationIcon}>
            <Tooltip direction="top" offset={[0, -12]} permanent>
              Your location
            </Tooltip>
            <Popup>Your location</Popup>
          </Marker>
        )}

        {filtered.map((bh) => (
          <Marker
            key={bh.id}
            position={[bh.latitude, bh.longitude]}
            eventHandlers={{ click: () => setSelected(bh) }}
          >
            {selected?.id === bh.id && (
              <Tooltip direction="top" offset={[0, -24]} permanent>
                {bh.name}
              </Tooltip>
            )}
            <Popup>
              <Box sx={{ minWidth: 180 }}>
                <Typography fontWeight={700}>{bh.name}</Typography>
                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                  {bh.address}
                </Typography>
                <Button size="small" variant="contained" onClick={() => setSelected(bh)}>
                  View details
                </Button>
              </Box>
            </Popup>
          </Marker>
        ))}

        {routePoints.length > 1 && (
          <Polyline positions={routePoints} color="#0ea5e9" weight={5} />
        )}
      </MapContainer>

        <Paper
          sx={{
            position: "absolute",
            left: 12,
            top: 12,
            bottom: 12,
            width: { xs: "calc(100% - 24px)", md: 400 },
            bgcolor: "rgba(255,255,255,0.97)",
            borderRadius: 3,
            overflow: "hidden",
          zIndex: 1000,
          display: "flex",
          flexDirection: "column",
          boxShadow: "0 18px 48px rgba(15,23,42,0.16)",
        }}
      >
        <Box
          sx={{
            p: 1.5,
            borderBottom: "1px solid",
            borderColor: "divider",
            bgcolor: "rgba(255,255,255,0.98)",
            position: "sticky",
            top: 0,
            zIndex: 2,
          }}
        >
          {!selected ? (
            <TextField
              size="small"
              fullWidth
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              placeholder="Search boarding houses"
              InputProps={{ startAdornment: <SearchIcon fontSize="small" /> }}
            />
          ) : (
            <Stack spacing={0.5}>
              <Stack direction="row" spacing={1} alignItems="center">
              <IconButton onClick={() => setSelected(null)} size="small">
                <ArrowBackIcon />
              </IconButton>
              <Typography variant="h6" fontWeight={800}>
                {detailQuery.data?.name ?? selected.name}
              </Typography>
              </Stack>
              <Typography variant="caption" color="text.secondary" sx={{ pl: 5 }}>
                Boarding house details
              </Typography>
            </Stack>
          )}
        </Box>

        <Box sx={{ p: 1.5, overflow: "auto", flex: 1 }}>
          {!selected ? (
            <Stack spacing={1.25}>
              <Typography variant="body2" color="text.secondary">
                {filtered.length} result{filtered.length === 1 ? "" : "s"}
              </Typography>
              {filtered.map((bh) => (
                <Paper
                  key={bh.id}
                  variant="outlined"
                  sx={{ p: 1.25, borderRadius: 2, cursor: "pointer", transition: "0.18s ease", "&:hover": { borderColor: "primary.main", bgcolor: "rgba(15,118,110,0.03)" } }}
                  onClick={() => setSelected(bh)}
                >
                  <Typography variant="body1" fontWeight={700}>
                    {bh.name}
                  </Typography>
                  <Typography variant="body2" color="text.secondary">
                    {bh.address}
                  </Typography>
                  <Typography variant="caption" color="text.secondary">
                    {bh.allowedGender}
                  </Typography>
                </Paper>
              ))}
            </Stack>
          ) : detailQuery.isLoading || !detailQuery.data ? (
            <Box sx={{ minHeight: 320, display: "grid", placeItems: "center" }}>
              <CircularProgress />
            </Box>
          ) : (
            <Stack spacing={2}>
              {heroImage && (
                <Box sx={{ position: "relative" }}>
                  <Box
                    component="img"
                    src={heroImage}
                    alt={detailQuery.data.name}
                    sx={{
                      width: "100%",
                      height: 220,
                      objectFit: "cover",
                      borderRadius: 2.5,
                      border: "1px solid",
                      borderColor: "divider",
                    }}
                  />
                  {mediaUrls.length > 1 && (
                    <>
                      <IconButton
                        size="small"
                        onClick={() =>
                          setSelectedImageIndex((prev) =>
                            prev === 0 ? mediaUrls.length - 1 : prev - 1,
                          )
                        }
                        sx={{
                          position: "absolute",
                          left: 10,
                          top: "50%",
                          transform: "translateY(-50%)",
                          bgcolor: "rgba(15,23,42,0.45)",
                          color: "white",
                          "&:hover": { bgcolor: "rgba(15,23,42,0.62)" },
                        }}
                      >
                        <ChevronLeftIcon fontSize="small" />
                      </IconButton>
                      <IconButton
                        size="small"
                        onClick={() =>
                          setSelectedImageIndex((prev) =>
                            prev === mediaUrls.length - 1 ? 0 : prev + 1,
                          )
                        }
                        sx={{
                          position: "absolute",
                          right: 10,
                          top: "50%",
                          transform: "translateY(-50%)",
                          bgcolor: "rgba(15,23,42,0.45)",
                          color: "white",
                          "&:hover": { bgcolor: "rgba(15,23,42,0.62)" },
                        }}
                      >
                        <ChevronRightIcon fontSize="small" />
                      </IconButton>
                    </>
                  )}
                </Box>
              )}

              {mediaUrls.length > 1 && (
                <Stack direction="row" spacing={0.75} sx={{ overflowX: "auto", pb: 0.25 }}>
                  {mediaUrls.map((url, index) => (
                    <Box
                      key={url}
                      component="button"
                      type="button"
                      onClick={() => setSelectedImageIndex(index)}
                      sx={{
                        p: 0,
                        border: index === selectedImageIndex ? "2px solid #0f766e" : "1px solid #cbd5e1",
                        borderRadius: 1.75,
                        overflow: "hidden",
                        cursor: "pointer",
                        background: "transparent",
                        minWidth: 72,
                      }}
                    >
                      <Box component="img" src={url} alt="thumbnail" sx={{ width: 72, height: 48, objectFit: "cover", display: "block" }} />
                    </Box>
                  ))}
                </Stack>
              )}

              <Box>
                <Typography variant="h5" fontWeight={900} sx={{ lineHeight: 1.2 }}>
                  {detailQuery.data.name}
                </Typography>
                {detailQuery.data.minPrice != null && (
                  <Typography variant="h6" color="primary.main" fontWeight={800} sx={{ mt: 0.5 }}>
                    PHP {detailQuery.data.minPrice.toLocaleString()} / mo
                  </Typography>
                )}
                <Typography variant="body2" color="text.secondary" sx={{ mt: 0.75 }}>
                  {detailQuery.data.address}
                </Typography>
              </Box>

              <Box>
                <Typography variant="subtitle1" fontWeight={800} sx={{ mb: 0.75 }}>
                  Overview
                </Typography>
                <Stack spacing={0.75}>
                  <Typography variant="body2"><strong>Owner:</strong> {detailQuery.data.ownerName || "Not available"}</Typography>
                  <Typography variant="body2"><strong>Allowed Gender:</strong> {detailQuery.data.allowedGender}</Typography>
                  <Typography variant="body2"><strong>Rooms:</strong> {detailQuery.data.roomCount}</Typography>
                  <Typography variant="body2"><strong>Coordinates:</strong> {detailQuery.data.latitude.toFixed(4)}, {detailQuery.data.longitude.toFixed(4)}</Typography>
                </Stack>
              </Box>

              {detailQuery.data.description && (
                <Box>
                  <Typography variant="subtitle1" fontWeight={800} sx={{ mb: 0.75 }}>
                    Description
                  </Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ lineHeight: 1.7 }}>
                    {detailQuery.data.description}
                  </Typography>
                </Box>
              )}

              {(detailQuery.data.amenities?.length ?? 0) > 0 && (
                <Box>
                  <Typography variant="subtitle1" fontWeight={800} sx={{ mb: 0.75 }}>
                    Amenities
                  </Typography>
                  <Stack direction="row" spacing={1} useFlexGap flexWrap="wrap">
                    {detailQuery.data.amenities.slice(0, 8).map((amenity) => (
                      <Stack
                        key={amenity.name}
                        direction="row"
                        spacing={0.75}
                        alignItems="center"
                        sx={{
                          px: 1,
                          py: 0.65,
                          borderRadius: 2,
                          border: "1px solid",
                          borderColor: "divider",
                          bgcolor: "rgba(15,23,42,0.03)",
                        }}
                      >
                        {getAmenityIcon(amenity.name)}
                        <Typography variant="body2">{amenity.name}</Typography>
                      </Stack>
                    ))}
                  </Stack>
                </Box>
              )}

              {(detailQuery.data.contacts?.length ?? 0) > 0 && (
                <Box>
                  <Typography variant="subtitle1" fontWeight={800} sx={{ mb: 0.75 }}>
                    Contacts
                  </Typography>
                  <Stack spacing={0.5}>
                    {detailQuery.data.contacts.map((contact) => (
                      <Box key={contact.id} sx={{ py: 0.75, borderBottom: "1px solid", borderColor: "divider" }}>
                        <Typography variant="body2" fontWeight={700}>{contact.phoneNumber}</Typography>
                        <Typography variant="caption" color="text.secondary">
                          {contact.network}{contact.isPrimary ? " • Primary contact" : ""}
                        </Typography>
                      </Box>
                    ))}
                  </Stack>
                </Box>
              )}

              <Box>
                  <Typography variant="subtitle1" fontWeight={800} sx={{ mb: 0.75 }}>
                    Rooms
                  </Typography>
                <Stack spacing={0.5}>
                  {detailQuery.data.rooms.slice(0, 8).map((room) => (
                    <Box key={room.id} sx={{ py: 0.85, borderBottom: "1px solid", borderColor: "divider" }}>
                      <Stack direction="row" justifyContent="space-between" spacing={1}>
                        <Typography variant="body2" fontWeight={700}>Room {room.roomNumber}</Typography>
                        <Typography variant="caption" color={room.occupancyStatus === "Full" ? "error.main" : "success.main"}>
                          {room.occupancyStatus}
                        </Typography>
                      </Stack>
                      <Typography variant="caption" color="text.secondary" display="block">
                        {room.baseCapacity} pax • {room.remainingSlots} remaining
                      </Typography>
                    </Box>
                  ))}
                </Stack>
              </Box>
            </Stack>
          )}
        </Box>
      </Paper>

      <Paper
        sx={{
          position: "absolute",
          top: 12,
          right: 12,
          p: 1,
          display: "flex",
          gap: 1,
          borderRadius: 2,
          bgcolor: "rgba(255,255,255,0.96)",
          zIndex: 1000,
        }}
      >
        <Button size="small" variant="outlined" startIcon={<NearMeIcon />} onClick={requestPreciseLocation} disabled={locating}>
          {locating ? "Locating..." : "Locate Me"}
        </Button>
        {!isAuthenticated ? (
          <>
            <Button size="small" variant="outlined" onClick={() => navigate("/login")}>Sign in</Button>
            <Button size="small" variant="contained" onClick={() => navigate("/register")}>Register</Button>
          </>
        ) : (
          <>
            {(role === "Owner" || role === "Admin") && (
              <Button
                size="small"
                variant="outlined"
                onClick={() => navigate(role === "Admin" ? "/admin/users" : "/boarding-houses")}
              >
                Dashboard
              </Button>
            )}
            <Button
              size="small"
              color="error"
              variant="text"
              onClick={() => {
                logout();
                navigate("/login", { replace: true });
              }}
            >
              Logout
            </Button>
          </>
        )}
      </Paper>

      {geoMessage && (
        <Alert severity="info" sx={{ position: "absolute", top: 70, right: 12, zIndex: 1000 }}>
          {geoMessage}
        </Alert>
      )}
    </Box>
  );
}
