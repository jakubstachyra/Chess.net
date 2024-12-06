import { configureStore } from '@reduxjs/toolkit';
import loginFormReducer from './authSlice/loginFormSlice';
import registerFormReducer from './authSlice/registerFormSlice';
import userReducer from './userSlice';

// Brak wstępnego odczytu tokena z localStorage
const preloadedState = {
  user: { user: null, token: null },
};

const store = configureStore({
  reducer: {
    registerForm: registerFormReducer,
    loginForm: loginFormReducer,
    user: userReducer,
  },
  preloadedState,
});

export default store;
