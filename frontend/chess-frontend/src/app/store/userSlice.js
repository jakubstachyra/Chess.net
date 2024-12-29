import { createSlice } from '@reduxjs/toolkit';

const userSlice = createSlice({
  name: 'user',
  initialState: {
    user: null,
    token: null,
    isAdmin: false, // Dodane pole
  },
  reducers: {
    login: (state, action) => {
      state.user = action.payload.user;
      state.token = action.payload.token;
      state.isAdmin = action.payload.isAdmin; 
    },
    logout: (state) => {
      state.user = null;
      state.token = null;
      state.isAdmin = false; 
    },
  },
});

export const { login, logout } = userSlice.actions;
export default userSlice.reducer;
