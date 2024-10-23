import NavBar from "../components/NavBar";

export default function HomeLayout({ children }: Readonly<{ children: React.ReactNode }>) {
    return (
        <html lang="en">
            <head></head>
            <body className="gradient-background">
                <NavBar></NavBar>
                {children}
            </body>
        </html>
    );
}
