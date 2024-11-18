// store.js
import { configureStore } from '@reduxjs/toolkit';
import formReducer from './registerSlice/registerFormSlice';

const store = configureStore({
  reducer: {
    form: formReducer,
  },
});

export default store;
