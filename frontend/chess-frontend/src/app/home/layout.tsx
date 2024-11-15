import type { Metadata } from "next";
import NavBar from "../components/navBar";
import "./style.css";

export const metadata: Metadata = {
  title: "Chess.net",
  description: "Web-app for playing chess",
};

export default function HomeLayout({children}: Readonly<{children: React.ReactNode;}>) {
  return (
    <html lang="en">
      <head></head>
      <body className = "gradient-background">
        <NavBar/>
        <div>
            <div className="centered">
                {children}
            </div>
        </div>
      </body>
    </html>
  );
}
