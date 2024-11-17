import type { Metadata } from "next";
<<<<<<< HEAD
import NavBar from "./components/NavBar.jsx";
import "./globals.css";

=======

import NavBar from "./components/navBar/navBar";
import "./globals.css";
import ReduxProvider from "./store/ReduxProvider"; 

>>>>>>> main
export const metadata: Metadata = {
  title: "Chess.net",
  description: "Web-app for playing chess",
};

<<<<<<< HEAD
export default function RootLayout({children}: Readonly<{children: React.ReactNode;}>) {
  return (
    <html lang="en">
      <head></head>
      <body className = "gradient-background">
        <NavBar/>
        <div className="centered">
        {children}

        </div>
=======
export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <head></head>
      <body className="gradient-background">
        <ReduxProvider>
          <NavBar />
          <div className="centered">{children}</div>
        </ReduxProvider>
>>>>>>> main
      </body>
    </html>
  );
}
