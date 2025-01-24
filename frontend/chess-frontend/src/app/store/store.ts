// src/app/store/store.ts

import { configureStore } from "@reduxjs/toolkit";
import { combineReducers } from "redux";
import userReducer from "./userSlice";
import loginFormReducer from "./authSlice/loginFormSlice";
import registerFormReducer from "./authSlice/registerFormSlice"; // Jeśli istnieje
import { persistStore, persistReducer } from "redux-persist";
import storage from "redux-persist/lib/storage"; // Domyślnie używa localStorage

const rootReducer = combineReducers({
  registerForm: registerFormReducer,
  loginForm: loginFormReducer,
  user: userReducer,
  // Dodaj inne reducery tutaj
});

const persistConfig = {
  key: "root",
  storage,
  whitelist: ["user"], // Reducery, które mają być przechowywane
};

const persistedReducer = persistReducer(persistConfig, rootReducer);

export const store = configureStore({
  reducer: persistedReducer,
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware({
      serializableCheck: {
        ignoredActions: ["persist/PERSIST"],
      },
    }),
});

export const persistor = persistStore(store);

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
