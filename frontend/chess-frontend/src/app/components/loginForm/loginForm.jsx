'use client';

import React from 'react';
import { useSelector, useDispatch } from 'react-redux';
import { updateField, setErrors } from '../../store/authSlice/loginFormSlice';
import { login } from '../../store/userSlice'; // Import akcji login
import { useRouter } from 'next/navigation';
import {
  Container,
  Box,
  Typography,
  TextField,
  Button,
  Grid,
  Link,
  Alert,
  CircularProgress,
} from '@mui/material';

function LoginForm() {
  const dispatch = useDispatch();
  const router = useRouter();
  const formData = useSelector((state) => state.loginForm);
  const { email, password, errors, success, loading } = formData;

  const handleChange = (e) => {
    const { name, value } = e.target;
    dispatch(updateField({ name, value }));
  };

  const validate = () => {
    let tempErrors = {};
    if (!email) tempErrors.email = 'Email is required.';
    if (!/\S+@\S+\.\S+/.test(email)) tempErrors.email = 'E-mail is incorrect.';
    if (!password) tempErrors.password = 'Password is required.';
    dispatch(setErrors(tempErrors));
    return Object.keys(tempErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (validate()) {
      try {
        const { email, password } = formData;
        const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;
        const response = await fetch(`${API_BASE_URL}/Account/login`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
          },
          body: JSON.stringify({ email, password }),
          credentials: 'include', 
        });
        if (!response.ok) {
          throw new Error(`HTTP Error: ${response.status}`);
        }
        const userData = await response.json();
        dispatch(
          login({
            user: { email: userData.email, username: userData.username },
            token: 'valid',
          })
        );
        // Przekieruj do strony "Play"
        console.log('Login successful');
        router.push('/play');
      } catch (error) {
        console.error('Login failed:', error.message);
      }
    }
  };

  return (
    <Container component="main" maxWidth="xs">
      <div style={modalContentStyles}>
        <Typography
          variant="h5"
          style={{
            color: 'white',
            textShadow: '-1px 1px 10px rgba(0, 0, 0, 0.75)',
          }}
        >
          Log in
        </Typography>
        <Box component="form" onSubmit={handleSubmit} sx={{ mt: 3 }}>
          <TextField
            fullWidth
            required
            label="Email"
            name="email"
            type="email"
            value={email}
            onChange={handleChange}
            error={!!errors.email}
            helperText={errors.email}
            margin="normal"
          />
          <TextField
            fullWidth
            required
            label="Password"
            name="password"
            type="password"
            value={password}
            onChange={handleChange}
            error={!!errors.password}
            helperText={errors.password}
            margin="normal"
          />
          <Button
            type="submit"
            fullWidth
            variant="contained"
            color="primary"
            sx={{ mt: 2, mb: 2 }}
            disabled={loading}
          >
            {loading ? <CircularProgress size={24} /> : 'Log in'}
          </Button>
          {success && <Alert severity="success">Login successful!</Alert>}
          {errors.general && <Alert severity="error">{errors.general}</Alert>}
          <Grid container justifyContent="flex-end">
            <Grid item>
              <Link href="/register" variant="body2">
                Don’t have an account? Sign up
              </Link>
            </Grid>
          </Grid>
        </Box>
      </div>
    </Container>
  );
}

export default LoginForm;

const modalContentStyles = {
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  padding: '20px',
  backgroundColor: 'rgba(255, 255, 255, 0.1)',
  borderRadius: '15px',
  boxShadow: '0 4px 30px rgba(0, 0, 0, 0.5)',
  backdropFilter: 'blur(10px)',
  border: '1px solid rgba(255, 255, 255, 0.2)',
};