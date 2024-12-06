import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { login } from '../userSlice';

// Akcja asynchroniczna do logowania
export const loginUser = createAsyncThunk(
  'form/loginUser',
  async (userData, { rejectWithValue, dispatch }) => {
    try {
      const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

      if (!API_BASE_URL) {
        throw new Error('NEXT_PUBLIC_API_BASE_URL is not defined in .env');
      }

      const response = await fetch(`${API_BASE_URL}/Account/login`, {
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
      const token = data.token.result;
      const userEmail = JSON.parse(atob(token.split('.')[1])).sub; 

      // Zapisz token w localStorage
      if (token) {
        localStorage.setItem('authToken', token);
      }

      // Wywołaj akcję login z userSlice, aby zaktualizować stan użytkownika
      dispatch(login({ user: { email: userEmail }, token }));

      return { userEmail, token };
    } catch (error) {
      return rejectWithValue({ message: error.message || 'An error occurred' });
    }
  }
);
const loginFormSlice = createSlice({
  name: 'loginForm',
  initialState: {
    email: '',
    password: '',
    errors: {},
    success: false,
    loading: false,
  },
  reducers: {
    updateField: (state, action) => {
      state[action.payload.name] = action.payload.value;
    },
    setErrors: (state, action) => {
      state.errors = action.payload;
    },
    resetForm: (state) => {
      state.email = '';
      state.password = '';
      state.errors = {};
      state.success = false;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(loginUser.pending, (state) => {
        state.loading = true;
        state.errors = {};
      })
      .addCase(loginUser.fulfilled, (state) => {
        state.loading = false;
        state.success = true;
      })
      .addCase(loginUser.rejected, (state, action) => {
        state.loading = false;
        state.errors.general = action.payload.message || 'Login failed';
      });
  },
});

export const { updateField, setErrors, resetForm } = loginFormSlice.actions;
export default loginFormSlice.reducer;
