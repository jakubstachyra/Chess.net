/**
 * @jest-environment jsdom
 */
import React from 'react';
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom';
import { Provider } from 'react-redux';
import { configureStore } from '@reduxjs/toolkit';
import NavBar from './NavBar';
import userReducer, { login, logout } from '../../store/userSlice';

jest.mock('next/link', () => {
  return ({ children, href }) => <a href={href}>{children}</a>;
});

describe('NavBar', () => {
  let store;

  beforeEach(() => {
    store = configureStore({
      reducer: {
        user: userReducer,
      },
      preloadedState: {
        user: {
          user: null,
          token: null,
        },
      },
    });

    global.fetch = jest.fn();
    jest.spyOn(console, 'error').mockImplementation(() => {}); // Suppress console.error
    jest.spyOn(console, 'warn').mockImplementation(() => {}); // Suppress console.warn
  });

  afterEach(() => {
    jest.clearAllMocks();
  });

  it('should render the navigation links correctly', () => {
    render(
      <Provider store={store}>
        <NavBar />
      </Provider>
    );

    expect(screen.getByText('Chess.net')).toBeInTheDocument();
    expect(screen.getByText('Play')).toBeInTheDocument();
    expect(screen.getByText('About')).toBeInTheDocument();
    expect(screen.getByText('Sign up')).toBeInTheDocument();
    expect(screen.getByText('Login')).toBeInTheDocument();
  });

  it('should display user info and logout button when logged in', async () => {
    store.dispatch(
      login({
        user: { username: 'testuser', email: 'test@example.com' },
        token: 'valid',
      })
    );

    render(
      <Provider store={store}>
        <NavBar />
      </Provider>
    );

    expect(screen.getByText('History')).toBeInTheDocument();
    expect(screen.getByText('testuser')).toBeInTheDocument();
    expect(screen.getByText('Logout')).toBeInTheDocument();
  });

  it('should log out the user when clicking the logout button', async () => {
    store.dispatch(
      login({
        user: { username: 'testuser', email: 'test@example.com' },
        token: 'valid',
      })
    );

    render(
      <Provider store={store}>
        <NavBar />
      </Provider>
    );

    global.fetch.mockResolvedValueOnce({ ok: true });

    fireEvent.click(screen.getByText('Logout'));

    await waitFor(() => {
      expect(store.getState().user.token).toBeNull();
    });

    expect(global.fetch).toHaveBeenCalledWith(expect.stringContaining('/Account/logout'), {
      method: 'POST',
      credentials: 'include',
    });
  });

  it('should fetch user data if not logged in', async () => {
    global.fetch.mockResolvedValueOnce({
      ok: true,
      json: () =>
        Promise.resolve({
          email: 'fetched@example.com',
          username: 'fetcheduser',
        }),
    });

    render(
      <Provider store={store}>
        <NavBar />
      </Provider>
    );

    await waitFor(() => {
      expect(store.getState().user.user.username).toBe('fetcheduser');
    });

    expect(global.fetch).toHaveBeenCalledWith(expect.stringContaining('/Account/me'), {
      method: 'GET',
      credentials: 'include',
    });
  });

  it('should handle failed user data fetch gracefully', async () => {
    global.fetch.mockResolvedValueOnce({ ok: false });

    render(
      <Provider store={store}>
        <NavBar />
      </Provider>
    );

    await waitFor(() => {
      expect(store.getState().user.user).toBeNull();
    });

    expect(global.fetch).toHaveBeenCalledWith(expect.stringContaining('/Account/me'), {
      method: 'GET',
      credentials: 'include',
    });
  });
});
