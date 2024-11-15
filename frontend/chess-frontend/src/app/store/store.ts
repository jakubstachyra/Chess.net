import { configureStore } from '@reduxjs/toolkit';
import gameModeReducer from './slices/gameModeSlice';

export const store = configureStore({
  reducer: {
    gameMode: gameModeReducer,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;
