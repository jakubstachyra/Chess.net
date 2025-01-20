'use client';

import Link from "next/link";
import { logout, login } from "../../store/userSlice";
import { useSelector, useDispatch } from "react-redux";
import { useEffect } from "react";

const NavBar = () => {
  const dispatch = useDispatch();
  const { user, token, isAdmin } = useSelector((state) => state.user); 
  const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL;

  useEffect(() => {
    const fetchUserData = async () => {
      if (token || user) return;
  
      try {
        const response = await fetch(`${API_BASE_URL}/auth/me`, {
          method: 'GET',
          credentials: 'include',
        });
  
        if (response.ok) {
          const data = await response.json();
  
          dispatch(login({ 
            user: { 
              userID: data.userID, 
              email: data.email, 
              username: data.username,
            }, 
            token: 'valid', 
            isAdmin: data.isAdmin, 
          }));
        } else {
          console.warn("Failed to fetch user data.");
        }
      } catch (error) {
        console.error("Error fetching user data:", error);
      }
    };
  
    fetchUserData();
  }, [dispatch, token, user, API_BASE_URL]);
  

  const handleLogout = async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/auth/logout`, {
        method: 'POST',
        credentials: 'include',
      });

      if (response.ok) {
        dispatch(logout()); // Czyszczenie stanu w Redux
      } else {
        console.warn("Nie udało się wylogować użytkownika.");
      }
    } catch (error) {
      console.error("Błąd podczas wylogowania:", error);
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
          {token ? <li><Link href="/history">History</Link></li> : null}
          {isAdmin ? <li><Link href="/admin">Admin</Link></li> : null} 
          <li><Link href="/about" style={linkStyle}>About</Link></li>
        </ul>
      </div>

      <div style={navRightStyle}>
        <ul style={ulStyle}>
          {token ? (
            <>
              <li>
                <Link style={usernameStyle} href="/my-profile">{user?.username}</Link>
              </li>
              <li>
                <Link href="/play" onClick={handleLogout} style={buttonStyle}>
                  Logout
                </Link>
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
