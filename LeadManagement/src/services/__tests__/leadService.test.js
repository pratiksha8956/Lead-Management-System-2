import api from "../api";
import { getLeadAnalytics, loginUser } from "../leadService";

jest.mock("../api");

describe("leadService", () => {
  beforeEach(() => jest.clearAllMocks());

  it("loginUser posts credentials", async () => {
    api.post.mockResolvedValue({
      data: { token: "t", user: { role: "Admin" } }
    });

    const result = await loginUser({ email: "a@b.com", password: "x" });
    expect(api.post).toHaveBeenCalledWith("/auth/login", { email: "a@b.com", password: "x" });
    expect(result.token).toBe("t");
  });

  it("getLeadAnalytics aggregates analytics endpoints", async () => {
    api.get
      .mockResolvedValueOnce({ data: { New: 2 } })
      .mockResolvedValueOnce({ data: { Website: 2 } })
      .mockResolvedValueOnce({ data: { conversionRate: 12.5 } })
      .mockResolvedValueOnce({ data: { Aman: 1 } });

    const result = await getLeadAnalytics();
    expect(api.get).toHaveBeenCalledTimes(4);
    expect(result.statusCounts.New).toBe(2);
    expect(result.sourceCounts.Website).toBe(2);
    expect(result.conversionRate).toBe(12.5);
    expect(result.salesRepCounts.Aman).toBe(1);
  });
});
