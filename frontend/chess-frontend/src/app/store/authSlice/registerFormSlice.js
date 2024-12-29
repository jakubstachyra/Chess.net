import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';

export const registerUser = createAsyncThunk(
  'form/registerUser',
  async (userData, { rejectWithValue }) => {
    try {
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

      if (!API_BASE_URL) {
        throw new Error('NEXT_PUBLIC_API_BASE_URL is not defined in .env');
      }

      const response = await fetch(`${API_BASE_URL}/Account/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(userData),
        credentials: 'include',
      });

      // Sprawdź, czy odpowiedź ma kod 204 lub puste ciało
      if (response.status === 204 || response.headers.get('content-length') === '0') {
        return {}; // Zwróć pusty obiekt dla pustych odpowiedzi
      }

      if (!response.ok) {
        const errorData = await response.json();
        return rejectWithValue(errorData);
      }

      const data = await response.json();
      return data;
    } catch (error) {
      return rejectWithValue({ message: error.message || 'An error occurred' });
    }
  }
);

const initialState = {
  username: '',
  email: '',
  password: '',
  confirmPassword: '',
  acceptTerms: false,
  errors: {},
  success: false,
  loading: false,
};

const registerFormSlice = createSlice({
  name: 'form',
  initialState,
  reducers: {
    updateField: (state, action) => {
      const { name, value } = action.payload;
      state[name] = value;
    },
    setErrors: (state, action) => {
      state.errors = action.payload;
    },
    setSuccess: (state, action) => {
      state.success = action.payload;
    },
    resetForm: (state) => {
      Object.assign(state, initialState);
    },
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
