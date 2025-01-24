import type { Metadata } from "next";
import AppProvider from "./provider";
import NavBar from "./components/navBar/navBar";
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
        <AppProvider>        
          <NavBar/>
        <div className="centered">
        {children}
        </div>
        </AppProvider>
      </body>
    </html>
  );
}
