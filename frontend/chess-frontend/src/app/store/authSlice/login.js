    import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
    import axios from 'axios';
    
    const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
    
    export const loginUser = createAsyncThunk(
      'auth/loginUser',
      async (credentials, { rejectWithValue }) => {
        try {
          const response = await axios.post(`${API_BASE_URL}/login`, credentials);
          return response.data;
        } catch (error) {
          return rejectWithValue(error.response.data);
        }
      }
    );
    
    export const refreshToken = createAsyncThunk(
      'auth/refreshToken',
      async (refreshToken, { rejectWithValue }) => {
        try {
          const response = await axios.post(`${API_BASE_URL}/refresh`, { token: refreshToken });
          return response.data;
        } catch (error) {
          return rejectWithValue(error.response.data);
        }
      }
    );
    
    const authSlice = createSlice({
      name: 'auth',
      initialState: {
        user: null,
        accessToken: null,
        refreshToken: null,
        isAuthenticated: false,
        loading: false,
        error: null,
      },
      reducers: {
        logout: (state) => {
          state.user = null;
          state.accessToken = null;
          state.refreshToken = null;
          state.isAuthenticated = false;
        },
      },
      extraReducers: (builder) => {
        builder
          .addCase(loginUser.pending, (state) => {
            state.loading = true;
            state.error = null;
          })
          .addCase(loginUser.fulfilled, (state, action) => {
            state.loading = false;
            state.user = action.payload.user;
            state.accessToken = action.payload.accessToken;
            state.refreshToken = action.payload.refreshToken;
            state.isAuthenticated = true;
          })
          .addCase(loginUser.rejected, (state, action) => {
            state.loading = false;
            state.error = action.payload;
          })
          .addCase(refreshToken.fulfilled, (state, action) => {
            state.accessToken = action.payload.accessToken;
          })
          .addCase(refreshToken.rejected, (state, action) => {
            state.isAuthenticated = false;
            state.error = action.payload;
          });
      },
    });
    
    export const { logout } = authSlice.actions;
    export default authSlice.reducer;
    
