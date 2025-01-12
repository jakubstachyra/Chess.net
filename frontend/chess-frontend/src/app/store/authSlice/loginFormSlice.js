import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import { login } from '../userSlice';

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
      const token = data.token; // bo serwer zwraca { token: "eyJhbG..." }
      
      const decodedToken = JSON.parse(atob(token.split('.')[1]));
      console.log(decodedToken);
      // Zmieniamy klucze na te, które masz w tokenie
      const userID = decodedToken.sub; 
      const userEmail = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"];
      const username = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || 'Unknown';
      const roles = decodedToken["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || [];
      const isAdmin = roles.includes("ADMIN");
      
     // localStorage.setItem('authToken', token);

      // Wywołujemy akcję login z userSlice (by uzupełnić stan Redux)
      dispatch(
        login({
          user: {userID, userEmail, username },
          token,
          isAdmin,
        })
      );
      
      return { userID, userEmail, username, isAdmin, token };
      
    } catch (error) {
      return rejectWithValue({
        message: error.message || 'An error occurred',
      });
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
        state.errors.general =
          action.payload?.message || 'Login failed';
      });
  },
});

export const { updateField, setErrors, resetForm } = loginFormSlice.actions;
export default loginFormSlice.reducer;
