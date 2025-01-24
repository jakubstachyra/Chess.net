// src/store/authSlice/loginFormSlice.ts

import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { login } from '../userSlice';
import { LoginFormState, User } from '../../../types/types';
import type { AppDispatch } from '../../store/store'; // Adjust the path as needed

interface DecodedToken {
  sub: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress': string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role': string[];
  // Add other fields present in the token if necessary
}

interface LoginCredentials {
  email: string;
  password: string;
}

export const loginUser = createAsyncThunk<
  void,
  LoginCredentials,
  { rejectValue: Record<string, string>; dispatch: AppDispatch }
>(
  'form/loginUser',
  async (userData, { rejectWithValue, dispatch }) => {
    try {
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

      if (!API_BASE_URL) {
        throw new Error('NEXT_PUBLIC_API_BASE_URL is not defined in .env');
      }

      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(userData),
        credentials: 'include',
      });

      if (!response.ok) {
        const errorData = await response.json();
        return rejectWithValue(errorData);
      }

      const data = await response.json();
      const token: string = data.token; // Assuming the server returns { token: "..." }

      const decodedToken: DecodedToken = JSON.parse(atob(token.split('.')[1]));
      
      console.log(decodedToken);

      const userID = decodedToken.sub;
      const userEmail = decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'];
      const username = decodedToken['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'Unknown';
      const roles = decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || [];
      const isAdmin = roles.includes('ADMIN');

      // Dispatch the login action from userSlice to update the Redux state
      const user: User = { userID, userEmail, username };
      dispatch(login({ user, token, isAdmin }));

      // Optionally: Save the token in localStorage or cookies if necessary
      // localStorage.setItem('authToken', token);
    } catch (error: unknown) {
      if (error instanceof Error) {
        return rejectWithValue({
          message: error.message,
        });
      }
      return rejectWithValue({
        message: 'An unexpected error occurred',
      });
    }
  }
);

const initialState: LoginFormState = {
  email: '',
  password: '',
  errors: {},
  success: false,
  loading: false,
};

const loginFormSlice = createSlice({
  name: 'loginForm',
  initialState,
  reducers: {
    updateField: (
      state,
      action: PayloadAction<{ name: keyof LoginFormState; value: string | boolean | Record<string, string> }>
    ) => {
      const { name, value } = action.payload;
      // @ts-expect-error it worked like that
      state[name] = value; 
    },
    setErrors: (state, action: PayloadAction<Record<string, string>>) => {
      state.errors = action.payload;
    },
    resetForm: () => initialState,
  },
  extraReducers: (builder) => {
    builder
      .addCase(loginUser.pending, (state) => {
        state.loading = true;
        state.errors = {};
        state.success = false;
      })
      .addCase(loginUser.fulfilled, (state) => {
        state.loading = false;
        state.success = true;
        state.password = '';
      })
      .addCase(loginUser.rejected, (state, action) => {
        state.loading = false;
        state.errors.general = action.payload?.message || 'Login failed';
      });
  },
});

export const { updateField, setErrors, resetForm } = loginFormSlice.actions;
export default loginFormSlice.reducer;


