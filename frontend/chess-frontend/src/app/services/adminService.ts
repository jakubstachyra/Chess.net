// src/services/adminService.ts

import apiClient from "./apiClient";
import { Report, AdminRequest, SuccessResponse } from "../../types/types";
import { AxiosResponse } from "axios";

// Funkcja do pobierania pierwszego aktywnego raportu
export const fetchReport = async (): Promise<Report | null> => {
  try {
    const response: AxiosResponse<Report> = await apiClient.get(`/reports/active/first`);

    if (response.status === 204) {
      console.log("No active reports available.");
      return null;
    }

    return response.data;
  } catch (error: unknown) {
    console.error("Error fetching report:", error);
    throw error;
  }
};

// Funkcja do banowania użytkownika na podstawie raportu
export const banUserWithReport = async (
  userId: string,
  reportId: string
): Promise<SuccessResponse> => {
  try {
    const response: AxiosResponse<SuccessResponse> = await apiClient.patch(
      `/users/${userId}/ban`,
      null,
      { params: { reportID: reportId } } // Przekazanie reportId jako parametru zapytania
    );

    return response.data;
  } catch (error: unknown) {
    console.error("Error banning user with report:", error);
    throw error;
  }
};

// Funkcja do odrzucania raportu
export const rejectReport = async (reportId: string): Promise<SuccessResponse> => {
  try {
    const response: AxiosResponse<SuccessResponse> = await apiClient.patch(`reports/${reportId}`);

    return response.data;
  } catch (error: unknown) {
    console.error("Error rejecting report:", error);
    throw error;
  }
};

// Funkcja do pobierania żądań administracyjnych
export const fetchRequests = async (): Promise<AdminRequest[]> => {
  try {
    const response: AxiosResponse<AdminRequest[]> = await apiClient.get(`admin-requests`);

    return response.data;
  } catch (error: unknown) {
    console.error("Error fetching requests:", error);
    throw error;
  }
};

// Funkcja do odrzucania żądania administracyjnego
export const rejectRequest = async (requestId: string): Promise<SuccessResponse> => {
  try {
    console.log(requestId);
    const response: AxiosResponse<SuccessResponse> = await apiClient.patch(`/admin-requests/${requestId}`);

    return response.data;
  } catch (error: unknown) {
    console.error("Error rejecting request:", error);
    throw error;
  }
};

// Funkcja do weryfikacji użytkownika
export const verifyUser = async (
  userID: string,
  requestID: string
): Promise<SuccessResponse> => {
  try {
    const response: AxiosResponse<SuccessResponse> = await apiClient.patch(
      `/users/${userID}/role`,
      null,
      { params: { requestID: requestID } }
    );

    return response.data;
  } catch (error: unknown) {
    console.error("Error verifying user:", error);
    throw error;
  }
};
