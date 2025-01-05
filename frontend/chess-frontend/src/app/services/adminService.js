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

export const banUserWithReport = async (userId, reportId) => {
    try {
        console.log("Sending request to ban user:", { userId, reportId });

        const response = await apiClient.patch(
            `/ban/${userId}`,
            null,
            { params: { reportID: reportId } } // Przekazanie reportId jako parametru zapytania
        );

        return response;
    } catch (error) {
        console.error("Error banning user with report:", error);
        throw error;
    }
};

  
