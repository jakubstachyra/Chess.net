// store.js

import { configureStore } from "@reduxjs/toolkit";
import loginFormReducer from "./authSlice/loginFormSlice";
import registerFormReducer from "./authSlice/registerFormSlice";
import userReducer from "./userSlice";

// Define the shape of the Redux store state
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
