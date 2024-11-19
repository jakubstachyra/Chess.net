// store.js
import { configureStore } from '@reduxjs/toolkit';
import formReducer from './authSlice/registerFormSlice';

const store = configureStore({
  reducer: {
    form: formReducer,
  },
});

export default store;
