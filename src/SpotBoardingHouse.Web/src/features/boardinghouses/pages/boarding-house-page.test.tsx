import {
  fireEvent,
  render,
  screen,
  waitFor,
  within,
} from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { BoardingHousesPage } from "./boarding-house-page";

const mockUseAuth = vi.fn();
const mockUseBoardingHousesQuery = vi.fn();

vi.mock("../../auth/use-auth", () => ({
  useAuth: () => mockUseAuth(),
}));

vi.mock("../queries", () => ({
  useBoardingHousesQuery: () => mockUseBoardingHousesQuery(),
  useOwnersQuery: () => ({ data: [] }),
  useCreateBoardingHouse: () => ({ mutateAsync: vi.fn(), isPending: false }),
  useUpdateBoardingHouse: () => ({ mutateAsync: vi.fn(), isPending: false }),
  useDeleteBoardingHouse: () => ({ mutateAsync: vi.fn(), isPending: false }),
}));

vi.mock("../dialogs/boarding-house-dialog", () => ({
  BoardingHouseDialog: () => null,
}));

vi.mock("../../contacts/components/contacts-section", () => ({
  ContactsSection: ({ boardingHouseId }: { boardingHouseId: string }) => (
    <div data-testid={`contacts-${boardingHouseId}`}>Contacts</div>
  ),
}));

vi.mock("../../media/components/media-section", () => ({
  MediaSection: ({ boardingHouseId }: { boardingHouseId: string }) => (
    <div data-testid={`media-${boardingHouseId}`}>Media</div>
  ),
}));

describe("BoardingHousesPage", () => {
  beforeEach(() => {
    mockUseBoardingHousesQuery.mockReturnValue({
      data: [
        {
          id: "bh-1",
          userId: "u-1",
          ownerName: "Owner One",
          name: "BH One",
          description: null,
          allowedGender: "Mixed",
          address: "Address One",
          latitude: 14.12,
          longitude: 121.45,
          facebookLink: null,
          createdAt: "2026-01-01T00:00:00.000Z",
          updatedAt: null,
        },
      ],
      isLoading: false,
      isError: false,
      error: null,
    });
  });

  it("shows owner column for admin and toggles expanded contacts/media sections", async () => {
    mockUseAuth.mockReturnValue({ role: "Admin" });

    render(<BoardingHousesPage />);

    expect(screen.getByText("All Boarding Houses")).toBeInTheDocument();
    expect(screen.getByText("Owner")).toBeInTheDocument();
    expect(screen.queryByTestId("contacts-bh-1")).not.toBeInTheDocument();
    expect(screen.queryByTestId("media-bh-1")).not.toBeInTheDocument();

    const mainRow = screen.getByText("BH One").closest("tr");
    expect(mainRow).not.toBeNull();

    const expandButton = within(mainRow as HTMLTableRowElement).getAllByRole(
      "button",
    )[0];
    fireEvent.click(expandButton);

    expect(screen.getByTestId("contacts-bh-1")).toBeInTheDocument();
    expect(screen.getByTestId("media-bh-1")).toBeInTheDocument();

    fireEvent.click(expandButton);
    await waitFor(() => {
      expect(screen.queryByTestId("contacts-bh-1")).not.toBeInTheDocument();
      expect(screen.queryByTestId("media-bh-1")).not.toBeInTheDocument();
    });
  });

  it("hides owner column for owner role", () => {
    mockUseAuth.mockReturnValue({ role: "Owner" });

    render(<BoardingHousesPage />);

    expect(screen.getByText("My Boarding Houses")).toBeInTheDocument();
    expect(screen.queryByText("Owner")).not.toBeInTheDocument();
  });
});
