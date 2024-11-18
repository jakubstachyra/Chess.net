import {createSlice, createAsyncThunk} from '@reduxjs/toolkit';
import build from 'next/dist/build';

export const registerUser = createAsyncThunk(
    'user/register',
    async(userData, {rejectWithValue}) => {
        try{
            const response = await fetch('${apiBaseUrl}/register', {
            method: 'POST',
            headers:{
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(userData),
            });
            if(!response.ok){
                throw new Error('Registration failed');
            }
            const data = await response.json();
            return data;
        }catch(error)
        {
            return rejectWithValue(error.message);
        }
    });
const userSlice = createSlice({
    name:  'user', 
    initialState: {
        user: null,
        loading: false,
        error: null,
    },
    reducers: {},
    extraReducers: (builder) => {
    builder.addCase(registerUser.pending, (state) => {
        state.loading = true;
        state.error = null;
    })
    .addCase(registerUser.fulfilled, (state, action) => {
        state.loading = false;
        state.userInfo = action.payload;
    })
    .addCase(registerUser.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload;
    });
    }

});

export default userSlice.reducer;
