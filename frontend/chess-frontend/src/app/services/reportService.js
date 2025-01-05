import apiClient from "./apiClient";

export const fetchReport = async () => {
  try {
    const response = await apiClient.get(`/Reports/getFirstActiveReport`);

    const reportData = response.data;
    return reportData;
  } catch (error) {
    console.error("Error fetching report:", error);
    throw error;
  }
};

