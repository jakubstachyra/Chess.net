// components/NavBar.js
import Link from "next/link";
import styles from "./NavBar.module.css";

const NavBar = () => {
  return (
    <nav
      style={{
        width: "100%",
        padding: "10px 20px",
        background: "gradient-backgound",
        color: "rgba(255, 255, 255, 1)",
        display: "flex",
        justifyContent: "space-between-nav",
        alignItems: "center",
        position: "fixed",
        top: 0,
        left: 0,
        zIndex: 1000,
        boxShadow: "0 2px 6px rgba(0, 0, 0, 0.1)",
      }}
    >
      <div className="nav-left" style={{ fontSize: "1.5rem" }}>
        <Link href={"/home"}>Chess.net </Link>
      </div>

      <div className="nav-options">
        <ul
          style={{
            listStyle: "none",
            display: "flex",
            gap: "20px",
            margin: 0,
            padding: 3,
          }}
        >
          <li>
            <Link href="/play">Play</Link>
          </li>
          <li>
            <Link href="/test">Test</Link>
          </li>
          <li>
            <Link
              href="/about"
              style={{ color: "white", textDecoration: "none" }}
            >
              About
            </Link>
          </li>
        </ul>
      </div>
      <div className="nav-right">
        <ul
          style={{
            textAlign: "right",
            listStyle: "none",
            display: "flex",
            gap: "20px",
            margin: 0,
            padding: 3,
          }}
        >
          <li>
            <Link href="/signup">Sign up </Link>
          </li>
          <li>
            <Link href="/login">Login </Link>
          </li>
        </ul>
      </div>
    </nav>
  );
};

export default NavBar;
