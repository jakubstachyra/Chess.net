import { createSlice } from '@reduxjs/toolkit';

const initialState = {
  isModalOpen: false,
  gameMode: null,
  timer: 'No Timer',
};

const gameModeSlice = createSlice({
  name: 'gameMode',
  initialState,
  reducers: {
    openModal: (state) => {
      state.isModalOpen = true;
    },
    closeModal: (state) => {
      state.isModalOpen = false;
    },
    setGameMode: (state, action) => {
      state.gameMode = action.payload;
    },
    setTimer: (state, action) => {
      state.timer = action.payload;
    },
  },
});

export const { openModal, closeModal, setGameMode, setTimer } = gameModeSlice.actions;

export default gameModeSlice.reducer;
