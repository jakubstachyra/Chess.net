import type { Metadata } from "next";
import NavBar from "./components/NavBar.jsx";
import "./globals.css";

export const metadata: Metadata = {
  title: "Chess.net",
  description: "Web-app for playing chess",
};

export default function RootLayout({children}: Readonly<{children: React.ReactNode;}>) {
  return (
    <html lang="en">
      <head></head>
      <body className = "gradient-background">
        <NavBar/>
        <div className="centered">
        {children}

        </div>
      </body>
    </html>
  );
}
