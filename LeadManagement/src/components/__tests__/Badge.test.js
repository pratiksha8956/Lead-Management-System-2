import React from "react";
import { render, screen } from "@testing-library/react";
import Badge from "../Badge";

describe("Badge", () => {
  it("renders status label", () => {
    render(<Badge type="status" value="Qualified" />);
    expect(screen.getByText("Qualified")).toBeInTheDocument();
  });
});
