  'use client';
  import React from 'react';
  import { useSelector, useDispatch } from 'react-redux';
  import { updateField, setErrors, resetForm, registerUser } from '../../store/authSlice/registerFormSlice';
  import {
    Container,
    Box,
    Typography,
    TextField,
    Button,
    Checkbox,
    FormControlLabel,
    Grid,
    Link,
    Alert,
    CircularProgress,
  } from '@mui/material';

  function RegisterForm() {
    const dispatch = useDispatch();
    const formData = useSelector((state) => state.form);
    const { username, email, password, confirmPassword, acceptTerms, errors, success, loading } = formData;

    const handleChange = (e) => {
      const { name, value, type, checked } = e.target;
      dispatch(updateField({ name, value: type === 'checkbox' ? checked : value }));
    };

    const validate = () => {
      let tempErrors = {};
      if (!username) tempErrors.username = 'Username is required.';
      if (!email) tempErrors.email = 'Email is required.';
      if (!/\S+@\S+\.\S+/.test(email)) tempErrors.email = 'E-mail is incorrect.';
      if (!password) tempErrors.password = 'Password is required.';
      if (password.length < 6) tempErrors.password = 'Password must be at least 6 characters.';
      if (password !== confirmPassword) tempErrors.confirmPassword = 'Passwords do not match.';
      if (!acceptTerms) tempErrors.acceptTerms = 'You must accept the terms.';
      dispatch(setErrors(tempErrors));
      return Object.keys(tempErrors).length === 0;
    };

    const handleSubmit = async (e) => {
      e.preventDefault();
      if (validate()) {
        try {
          const { email, password } = formData;
          await dispatch(registerUser({ email, password })).unwrap();
          console.log('Registration successful!');
        } catch (error) {
          console.error('Registration failed:', error);
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
            Sign up
          </Typography>
          <Box component="form" onSubmit={handleSubmit} sx={{ mt: 3 }}>
            <TextField
              fullWidth
              required
              label="Username"
              name="username"
              value={username}
              onChange={handleChange}
              error={!!errors.username}
              helperText={errors.username}
              margin="normal"
            />
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
            <TextField
              fullWidth
              required
              label="Confirm password"
              name="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={handleChange}
              error={!!errors.confirmPassword}
              helperText={errors.confirmPassword}
              margin="normal"
            />
            <FormControlLabel
              control={
                <Checkbox
                  color="primary"
                  name="acceptTerms"
                  checked={acceptTerms}
                  onChange={handleChange}
                />
              }
              label="Accept terms"
            />
            {errors.acceptTerms && (
              <Typography color="error" variant="body2">
                {errors.acceptTerms}
              </Typography>
            )}
            <Button
              type="submit"
              fullWidth
              variant="contained"
              color="primary"
              sx={{ mt: 2, mb: 2 }}
              disabled={loading}
            >
              {loading ? <CircularProgress size={24} /> : 'Sign up'}
            </Button>
            {success && <Alert severity="success">Registration successful!</Alert>}
            {errors.general && <Alert severity="error">{errors.general}</Alert>}
            <Grid container justifyContent="flex-end">
              <Grid item>
                <Link href="/login" variant="body2">
                  Already have an account? Log in
                </Link>
              </Grid>
            </Grid>
          </Box>
        </div>
      </Container>
    );
  }

  export default RegisterForm;

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
