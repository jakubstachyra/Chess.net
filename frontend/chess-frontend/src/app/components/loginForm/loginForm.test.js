/**
 * @jest-environment jsdom
 */
import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import LoginForm from './loginForm';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import loginFormReducer from '../../store/authSlice/loginFormSlice';
import userReducer, { login } from '../../store/userSlice';
import { useRouter } from 'next/navigation';

// Mocking next/navigation
jest.mock('next/navigation', () => ({
  useRouter: jest.fn(),
}));

// Mocking useSelector and useDispatch
import { useDispatch, useSelector } from 'react-redux';
jest.mock('react-redux', () => {
  const actual = jest.requireActual('react-redux');
  return {
    ...actual,
    useDispatch: jest.fn(),
    useSelector: jest.fn(),
  };
});

describe('LoginForm', () => {
  let dispatchMock;
  let pushMock;

  beforeEach(() => {
    dispatchMock = jest.fn();
    useDispatch.mockReturnValue(dispatchMock);
    pushMock = jest.fn();
    useRouter.mockReturnValue({ push: pushMock });

    useSelector.mockImplementation(() => ({
      email: '',
      password: '',
      errors: {},
      success: false,
      loading: false,
    }));

    // Mock fetch
    global.fetch = jest.fn((url, options) => {
      if (options.body.includes('test@example.com')) {
        return Promise.resolve({
          ok: true,
          json: () =>
            Promise.resolve({
              email: 'test@example.com',
              username: 'testuser',
            }),
        });
      }
      return Promise.resolve({
        ok: false,
        json: () => Promise.resolve({ message: 'Invalid credentials' }),
      });
    });
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  it('should render email and password fields along with a login button', () => {
    render(
      <Provider store={configureStore({ reducer: { loginForm: loginFormReducer, user: userReducer } })}>
        <LoginForm />
      </Provider>
    );

    expect(screen.getByLabelText(/Email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Password/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /Log in/i })).toBeInTheDocument();
  });

  it('should call login action and navigate to /play upon valid login', async () => {
    useSelector.mockImplementation(() => ({
      email: 'test@example.com',
      password: 'Test@1234',
      errors: {},
      success: false,
      loading: false,
    }));

    render(
      <Provider store={configureStore({ reducer: { loginForm: loginFormReducer, user: userReducer } })}>
        <LoginForm />
      </Provider>
    );

    fireEvent.click(screen.getByRole('button', { name: /Log in/i }));

    await waitFor(() => {
      expect(dispatchMock).toHaveBeenCalledWith(
        login({
          user: { email: 'test@example.com', username: 'testuser' },
          token: 'valid',
        })
      );
      expect(pushMock).toHaveBeenCalledWith('/play');
    });
  });
});
