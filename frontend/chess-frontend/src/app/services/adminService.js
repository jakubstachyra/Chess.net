import apiClient from "./apiClient";

export const fetchReport = async () => {
    try {
      const response = await apiClient.get(`/reports/active/first`);
  
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
            `/users/${userId}/ban`,
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
        const response = await apiClient.patch(`reports/${reportId}`);

        return response;
    } catch (error) {
        console.error("Error rejecting report:", error);
        throw error;
    }
};
export const fetchRequests = async () => {
    try {
        const response = await apiClient.get(`admin-requests`);

        return response.data;
    } catch (error) {
        console.error("Error fetching requests:", error);
        throw error;
    }
}
export const rejectRequest = async (requestId) => {
  try {
      console.log(requestId);
      const response = await apiClient.patch(`/admin-requests/${requestId}`);

      return response;
  } catch (error) {
      console.error("Error rejecting report:", error);
      throw error;
  }
};
export const verifyUser = async (userID, requestID) => {
  try {
      const response = await apiClient.patch(`/users/${userID}/role`,
      null,
      { params: { requestID: requestID } });

      return response;
  } catch (error) {
      console.error("Error rejecting report:", error);
      throw error;
  }
};
