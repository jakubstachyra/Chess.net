// src/store/authSlice/registerFormSlice.ts

import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { RegisterFormState } from '../../types';

interface RegisterCredentials {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  acceptTerms: boolean;
}

export const registerUser = createAsyncThunk<
  void,
  RegisterCredentials,
  { rejectValue: Record<string, string> }
>(
  'form/registerUser',
  async (userData, { rejectWithValue }) => {
    try {
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

      if (!API_BASE_URL) {
        throw new Error('NEXT_PUBLIC_API_BASE_URL is not defined in .env');
      }

      const response = await fetch(`${API_BASE_URL}/auth/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(userData),
        credentials: 'include',
      });

      // Sprawdź, czy odpowiedź ma kod 204 lub puste ciało
      if (response.status === 204 || response.headers.get('content-length') === '0') {
        return; // Zwróć void dla pustych odpowiedzi
      }

      if (!response.ok) {
        const errorData = await response.json();
        return rejectWithValue(errorData);
      }

      // Zakładamy, że serwer zwraca JSON z danymi użytkownika
      const data = await response.json();
      return data;
    } catch (error: unknown) {
      if (error instanceof Error) {
        return rejectWithValue({ message: error.message });
      }
      return rejectWithValue({ message: 'An unexpected error occurred' });
    }
    
  }
);

const initialState: RegisterFormState = {
  username: '',
  email: '',
  password: '',
  confirmPassword: '',
  acceptTerms: false,
  errors: {},
  success: false,
  loading: false,
};

type RegisterFormFields = 'username' | 'email' | 'password' | 'confirmPassword' | 'acceptTerms';

const registerFormSlice = createSlice({
  name: 'registerForm',
  initialState,
  reducers: {
    updateField: (
      state,
      action: PayloadAction<{ name: RegisterFormFields; value: string | boolean }>
    ) => {
      const { name, value } = action.payload;
      state[name] = value;
    },
    setErrors: (state, action: PayloadAction<Record<string, string>>) => {
      state.errors = action.payload;
    },
    setSuccess: (state, action: PayloadAction<boolean>) => {
      state.success = action.payload;
    },
    resetForm: () => initialState,
  },
  extraReducers: (builder) => {
    builder
      .addCase(registerUser.pending, (state) => {
        state.loading = true;
        state.errors = {};
        state.success = false;
      })
      .addCase(registerUser.fulfilled, (state) => {
        state.loading = false;
        state.success = true;
      })
      .addCase(registerUser.rejected, (state, action) => {
        state.loading = false;
        state.errors = action.payload || { general: 'Registration failed' };
      });
  },
});

export const { updateField, setErrors, setSuccess, resetForm } = registerFormSlice.actions;
export default registerFormSlice.reducer;
