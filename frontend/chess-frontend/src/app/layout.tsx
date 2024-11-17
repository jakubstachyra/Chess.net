import type { Metadata } from "next";
import NavBar from "./components/navBar/navBar";
import "./globals.css";
import ReduxProvider from "./store/ReduxProvider"; 
export const metadata: Metadata = {
  title: "Chess.net",
  description: "Web-app for playing chess",
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en">
      <head></head>
export default function RootLayout({children}: Readonly<{children: React.ReactNode;}>) {
  return (
    <html lang="en">
      <head></head>
      <body className = "gradient-background">
        <NavBar/>
        <div className="centered">
        {children}

        </div>
      <body className="gradient-background">
        <ReduxProvider>
          <NavBar />
          <div className="centered">{children}</div>
        </ReduxProvider>
      </body>
    </html>
  );
}
