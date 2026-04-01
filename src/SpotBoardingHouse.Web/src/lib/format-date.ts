export function formatDateTime(value: string | null) {
  if (!value) return "-";

  const dateStr = value.endsWith("Z") ? value : value + "Z";
  const date = new Date(dateStr);

  if (Number.isNaN(date.getTime())) return "-";

  return new Intl.DateTimeFormat(undefined, {
    year: "numeric",
    month: "numeric",
    day: "numeric",
    hour: "numeric",
    minute: "numeric",
    second: "numeric",
    timeZone: "Asia/Manila",
  }).format(date);
}
