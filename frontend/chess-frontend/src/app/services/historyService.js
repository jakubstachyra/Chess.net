import apiClient from "./apiClient";
export const fetchGameHistoryByID = async (gameId) => {
    try {
      const response = await apiClient.get(`/history/games/${gameID}`);
  
      const reportData = response.data;
      return reportData;
    } catch (error) {
      console.error("Error fetching report:", error);
      throw error;
    }
  };