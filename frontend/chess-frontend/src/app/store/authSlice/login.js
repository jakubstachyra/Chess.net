import {createSlice, createAsyncThunk} from '@reduxjs/toolkit';

export const loginUser = createAsyncThunk(
    'auth/login',
    async (credentials, {rejectWithValue}) => {
        try{
            const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
            const respones = await fetch('{API_BASE_URL}/login',{
                method: 'POST',
                headers:{
                    'Content-Type': 'application/json',
                },
                credentials: 'include', // potrzebne do przesyłania ciasteczek sesji 
                body: JSON.stringify(credentials),
            } );
        }
    }