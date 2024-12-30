import { createSlice } from '@reduxjs/toolkit';

const userSlice = createSlice({
  name: 'user',
  initialState: {
    user: null,
    token: null,
    isAdmin: false,
  },
  reducers: {
    login: (state, action) => {
      state.user = {
        id: action.payload.user.userID, 
        email: action.payload.user.email,
        username: action.payload.user.username,
      };
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
