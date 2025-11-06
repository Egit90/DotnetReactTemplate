import { House } from "lucide-react";
import { SignUpForm } from "../components/SignUpForm.tsx";
import { Link } from "react-router-dom";

export const SignUp = () => {
    return (
        <>
            <div className="flex min-h-screen min-w-full w-full items-center justify-center p-6 md:p-10 relative">
                <Link
                    to="/"
                    className="absolute top-4 left-4 flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-foreground transition-colors"
                >
                    <House className="h-4 w-4" />
                    Home
                </Link>
                <div className="w-full max-w-md">
                    <SignUpForm />
                </div>
            </div>
        </>
    );
};

