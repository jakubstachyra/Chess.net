'use client';

import Link from "next/link";
import { logout, login } from "../../store/userSlice";
import { useSelector, useDispatch } from "react-redux";
import { useEffect } from "react";

const NavBar = () => {
  const dispatch = useDispatch();
  const { user, token } = useSelector((state) => state.user);
  const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

  useEffect(() => {
    const checkLogin = async () => {
      if (token) return;

      try {
        const response = await fetch(`${API_BASE_URL}/Account/me`, { 
          method: 'GET', 
          credentials: 'include',
        });

        if (response.ok) {
          const data = await response.json();
          dispatch(login({ user: { email: data.email, username: data.username }, token: 'valid' }));
        } else {
          console.warn("Failed to download user data.");
        }
      } catch (error) {
        console.error('Error finding logged in:', error);
      }
    };

    checkLogin();
  }, [dispatch]);

  const handleLogout = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/Account/logout`, {
        method: 'POST',
        credentials: 'include',
      });
      if (response.ok) {
        dispatch(logout());
      } else {
        console.warn("Nie udało się wylogować użytkownika.");
      }
    } catch (error) {
      console.error('Błąd podczas wylogowania:', error);
    }
  };

  return (
    <nav style={navStyle}>
      <div style={navLeftStyle}>
        <Link href="/home">Chess.net</Link>
      </div>
      
      <div style={navOptionsStyle}>
        <ul style={ulStyle}>
          <li><Link href="/play">Play</Link></li>
          {token? <li><Link href="/history">History</Link></li> : null}
          <li><Link href="/about" style={linkStyle}>About</Link></li>
        </ul>
      </div>

      <div style={navRightStyle}>
        <ul style={ulStyle}>
          {token ? (
            <>
              <li>
                <span style={usernameStyle}>{user?.username }</span>
              </li>
              <li>
                <button onClick={handleLogout} style={buttonStyle}>
                  Logout
                </button>
              </li>

            </>
          ) : (
            <>
              <li><Link href="/sign-up">Sign up</Link></li>
              <li><Link href="/log-in">Login</Link></li>
            </>
          )}
        </ul>
      </div>
    </nav>
  );
};

export default NavBar;

// Style
const navStyle = {
  width: '100%',
  padding: '10px 20px',
  background: 'gradient-backgound',
  color: 'rgba(255, 255, 255, 1)',
  display: 'flex',
  justifyContent: 'space-between',
  alignItems: 'center',
  position: 'fixed',
  top: 0,
  left: 0,
  zIndex: 1000,
  boxShadow: '0 2px 6px rgba(0, 0, 0, 0.1)',
};

const navLeftStyle = {
  fontSize: '1.5rem',
};

const navOptionsStyle = {
  display: 'flex',
  gap: '20px',
  flexGrow: 1,
  padding: '0 30px',
  justifyContent: 'left',
};

const navRightStyle = {};

const ulStyle = {
  listStyle: 'none',
  display: 'flex',
  gap: '20px',
  margin: 0,
  padding: 0,
};

const linkStyle = {
  color: 'white',
  textDecoration: 'none',
};

const buttonStyle = {
  background: 'none',
  border: 'none',
  color: 'white',
  cursor: 'pointer',
};

const usernameStyle = {
  color: 'lightblue',
};
