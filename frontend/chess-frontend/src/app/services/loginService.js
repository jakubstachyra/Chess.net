import apiClient from "./apiClient";

export const loginUser = async (email, password) => {
  const response = await apiClient.post(
    "/Account/login",
    { email, password },
    { withCredentials: true }
  );
  return response.data;
};
