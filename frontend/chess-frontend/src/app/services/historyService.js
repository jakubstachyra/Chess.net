import apiClient from "./apiClient";
export const fetchGameHistoryByID = async (gameId) => {
    try {
      const response = await apiClient.get(`/getGameHistoryByID/${gameId}`);
  
      const reportData = response.data;
      return reportData;
    } catch (error) {
      console.error("Error fetching report:", error);
      throw error;
    }
  };