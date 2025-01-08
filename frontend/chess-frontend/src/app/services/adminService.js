import apiClient from "./apiClient";

export const fetchReport = async () => {
    try {
      const response = await apiClient.get(`/Reports/getFirstActiveReport`);
  
      if (response.status === 204) {
        console.log("No active reports available.");
        return null; 
      }
  
      return response.data; 
    } catch (error) {
      console.error("Error fetching report:", error);
      throw error;
    }
  };
  

export const banUserWithReport = async (userId, reportId) => {
    try {
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

export const rejectReport = async (reportId) => {
    try {
        const response = await apiClient.patch(`/Reports/makeReportResolved/${reportId}`);

        return response;
    } catch (error) {
        console.error("Error rejecting report:", error);
        throw error;
    }
};
  
