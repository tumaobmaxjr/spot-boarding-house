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
import { useEffect, useState } from "react";
import {
  MapContainer,
  Marker,
  TileLayer,
  useMap,
  useMapEvents,
} from "react-leaflet";
import L from "leaflet";
import "leaflet/dist/leaflet.css";
import markerIcon2x from "leaflet/dist/images/marker-icon-2x.png";
import markerIcon from "leaflet/dist/images/marker-icon.png";
import markerShadow from "leaflet/dist/images/marker-shadow.png";
import MyLocationIcon from "@mui/icons-material/MyLocation";
import type { OwnerDto } from "../types";

const defaultIcon = L.icon({
  iconUrl: markerIcon,
  iconRetinaUrl: markerIcon2x,
  shadowUrl: markerShadow,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
});

L.Marker.prototype.options.icon = defaultIcon;

interface BoardingHouseDialogProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (payload: {
    name: string;
    description?: string;
    allowedGender: string;
    address: string;
    latitude: number;
    longitude: number;
    facebookLink?: string;
    ownerId?: string;
  }) => void;
  initialData?: {
    id: string;
    name: string;
    description: string | null;
    allowedGender: string;
    address: string;
    latitude: number;
    longitude: number;
    facebookLink: string | null;
  } | null;
  isAdmin?: boolean;
  owners?: OwnerDto[];
  isLoading?: boolean;
}

function MapPicker({
  position,
  onPositionChange,
}: {
  position: [number, number];
  onPositionChange: (lat: number, lng: number) => void;
}) {
  useMapEvents({
    click(e) {
      onPositionChange(e.latlng.lat, e.latlng.lng);
    },
  });

  return (
    <Marker
      position={position}
      draggable
      eventHandlers={{
        dragend: (e) => {
          const marker = e.target;
          const pos = marker.getLatLng();
          onPositionChange(pos.lat, pos.lng);
        },
      }}
    />
  );
}

function MapController({ center }: { center: [number, number] }) {
  const map = useMap();
  useEffect(() => {
    map.setView(center, map.getZoom());
  }, [center, map]);
  return null;
}

export function BoardingHouseDialog({
  open,
  onClose,
  onSubmit,
  initialData,
  isAdmin,
  owners,
  isLoading,
}: BoardingHouseDialogProps) {
  const isEdit = !!initialData;
  const [name, setName] = useState(initialData?.name ?? "");
  const [description, setDescription] = useState(
    initialData?.description ?? "",
  );
  const [allowedGender, setAllowedGender] = useState(
    initialData?.allowedGender ?? "Mixed",
  );
  const [address, setAddress] = useState(initialData?.address ?? "");
  const [latitude, setLatitude] = useState(initialData?.latitude ?? 14.5995);
  const [longitude, setLongitude] = useState(
    initialData?.longitude ?? 120.9842,
  );
  const [facebookLink, setFacebookLink] = useState(
    initialData?.facebookLink ?? "",
  );
  const [ownerId, setOwnerId] = useState(owners?.[0]?.id ?? "");
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [locating, setLocating] = useState(false);

  const handleAutoLocate = () => {
    setLocating(true);
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (pos) => {
          setLatitude(pos.coords.latitude);
          setLongitude(pos.coords.longitude);
          setLocating(false);
        },
        () => {
          setLocating(false);
        },
      );
    } else {
      setLocating(false);
    }
  };

  const validate = (): boolean => {
    const errs: Record<string, string> = {};
    if (!name.trim()) errs.name = "Name is required";
    if (!address.trim()) errs.address = "Address is required";
    if (!allowedGender) errs.allowedGender = "Gender is required";
    if (isAdmin && !ownerId) errs.ownerId = "Owner is required";
    if (isNaN(latitude) || latitude < -90 || latitude > 90)
      errs.latitude = "Valid latitude required (-90 to 90)";
    if (isNaN(longitude) || longitude < -180 || longitude > 180)
      errs.longitude = "Valid longitude required (-180 to 180)";
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!validate()) return;
    onSubmit({
      name: name.trim(),
      description: description.trim() || undefined,
      allowedGender,
      address: address.trim(),
      latitude,
      longitude,
      facebookLink: facebookLink.trim() || undefined,
      ownerId: isAdmin ? ownerId : undefined,
    });
  };

  const handleMapPositionChange = (lat: number, lng: number) => {
    setLatitude(lat);
    setLongitude(lng);
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <form onSubmit={handleSubmit}>
        <DialogTitle>
          {isEdit ? "Edit Boarding House" : "Create Boarding House"}
        </DialogTitle>
        <DialogContent
          sx={{
            display: "flex",
            flexDirection: "column",
            gap: 2,
            pt: 2,
            overflow: "visible",
          }}
        >
          {isAdmin && !isEdit && (
            <FormControl error={!!errors.ownerId} fullWidth>
              <InputLabel>Owner</InputLabel>
              <Select
                value={ownerId}
                onChange={(e) => setOwnerId(e.target.value)}
                label="Owner"
              >
                {owners?.map((owner) => (
                  <MenuItem key={owner.id} value={owner.id}>
                    {owner.firstName} {owner.lastName}
                  </MenuItem>
                ))}
              </Select>
              {errors.ownerId && (
                <FormHelperText>{errors.ownerId}</FormHelperText>
              )}
            </FormControl>
          )}

          <TextField
            label="Name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            error={!!errors.name}
            helperText={errors.name}
            required
            fullWidth
            autoFocus
          />

          <TextField
            label="Description"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            multiline
            rows={2}
            fullWidth
          />

          <FormControl error={!!errors.allowedGender} fullWidth>
            <InputLabel>Allowed Gender</InputLabel>
            <Select
              value={allowedGender}
              onChange={(e) => setAllowedGender(e.target.value)}
              label="Allowed Gender"
            >
              <MenuItem value="Male">Male</MenuItem>
              <MenuItem value="Female">Female</MenuItem>
              <MenuItem value="Mixed">Mixed</MenuItem>
            </Select>
            {errors.allowedGender && (
              <FormHelperText>{errors.allowedGender}</FormHelperText>
            )}
          </FormControl>

          <TextField
            label="Address"
            value={address}
            onChange={(e) => setAddress(e.target.value)}
            error={!!errors.address}
            helperText={errors.address}
            required
            fullWidth
          />

          <div style={{ display: "flex", gap: 2, alignItems: "flex-start" }}>
            <TextField
              label="Latitude"
              type="number"
              value={latitude}
              onChange={(e) => setLatitude(parseFloat(e.target.value) || 0)}
              error={!!errors.latitude}
              helperText={errors.latitude}
              required
              fullWidth
              inputProps={{ step: "any" }}
            />
            <TextField
              label="Longitude"
              type="number"
              value={longitude}
              onChange={(e) => setLongitude(parseFloat(e.target.value) || 0)}
              error={!!errors.longitude}
              helperText={errors.longitude}
              required
              fullWidth
              inputProps={{ step: "any" }}
            />
            <Button
              variant="outlined"
              onClick={handleAutoLocate}
              disabled={locating}
              sx={{ minWidth: "auto", height: 56 }}
              title="Auto-detect location"
            >
              <MyLocationIcon />
            </Button>
          </div>

          <TextField
            label="Facebook Link (optional)"
            value={facebookLink}
            onChange={(e) => setFacebookLink(e.target.value)}
            fullWidth
          />

          <div
            style={{
              height: 300,
              width: "100%",
              borderRadius: 8,
              overflow: "hidden",
              position: "relative",
            }}
          >
            {locating && (
              <div
                style={{
                  position: "absolute",
                  top: 8,
                  right: 8,
                  background: "white",
                  padding: "4px 8px",
                  borderRadius: 4,
                  fontSize: 12,
                  zIndex: 1000,
                  boxShadow: "0 2px 4px rgba(0,0,0,0.2)",
                }}
              >
                Getting your location...
              </div>
            )}
            <MapContainer
              center={[latitude, longitude]}
              zoom={13}
              style={{ height: "100%", width: "100%" }}
            >
              <TileLayer
                url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                attribution="© OpenStreetMap contributors"
              />
              <MapPicker
                position={[latitude, longitude]}
                onPositionChange={handleMapPositionChange}
              />
              <MapController center={[latitude, longitude]} />
            </MapContainer>
          </div>
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
