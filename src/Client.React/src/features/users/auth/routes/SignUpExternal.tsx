import { useNavigate } from "react-router-dom";
import { useAuth } from "@/providers/AuthProvider.tsx";
import { useEffect } from "react";

export const SignUpExternal = () => {
    const navigate = useNavigate();
    const { authClient } = useAuth();

    useEffect(() => {
        authClient.signUpExternal({}).then(() => {
            navigate("/profile")
        }).catch(() => {
            navigate("/signin?error=external-signin-failed")
        });
    }, []);

    return (
        <>
            <h1>Signing up...</h1>
        </>
    );
};
