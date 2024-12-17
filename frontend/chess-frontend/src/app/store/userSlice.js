import { createSlice } from '@reduxjs/toolkit';

// const initialState = {
//   user: {
//     email: null, 
//     username: null, 
//   },
//   token: null, 
// };

const userSlice = createSlice({
  name: 'user',
  initialState: {
    user: null,
    token: null,
  },
  reducers: {
    login: (state, action) => {
      console.log('Payload for login:', action.payload);
      state.user = action.payload.user;
      state.token = action.payload.token;
    },
    logout: (state) => {
      state.user = null;
      state.token = null;
    },
  },
});


export const { login, logout } = userSlice.actions;
export default userSlice.reducer;
