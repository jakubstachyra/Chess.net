import apiClient from "./apiClient";
import { GameHistory } from "../../types/types";
import { AxiosResponse } from "axios";

export const fetchGameHistoryByID = async (gameID: string): Promise<GameHistory | null> => {
  try {
    const response: AxiosResponse<GameHistory> = await apiClient.get(`/history/games/${gameID}`);
    return response.data;
  } catch (error: unknown) {
    console.error("Error fetching game history by ID:", error);
    return null;
  }
};
