// src/app/store/userSlice.ts

import { createSlice, PayloadAction } from "@reduxjs/toolkit";

interface User {
  userID: string;
  userEmail: string;
  username: string;
}

interface UserState {
  user: User | null;
  token: string;
  isAdmin: boolean;
}

const initialState: UserState = {
  user: null,
  token: "",
  isAdmin: false,
};

const userSlice = createSlice({
  name: "user",
  initialState,
  reducers: {
    login: (
      state,
      action: PayloadAction<{ user: User; token: string; isAdmin: boolean }>
    ) => {
      state.user = action.payload.user;
      state.token = action.payload.token;
      state.isAdmin = action.payload.isAdmin;
    },
    logout: (state) => {
      state.user = null;
      state.token = "";
      state.isAdmin = false;
    },
    // Inne akcje, jeśli potrzebne
  },
});

export const { login, logout } = userSlice.actions;
export default userSlice.reducer;
