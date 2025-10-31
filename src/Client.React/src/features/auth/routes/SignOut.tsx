import {Navigate} from "react-router-dom";
import {useAuth} from "../../../providers/AuthProvider.tsx";

export const SignOut = () => {    
    const { authClient } = useAuth();
    authClient.signOut().then(() => {
        // Do nothing
    }).catch(() => {
        // Do nothing
    });
    
    return <Navigate to={"/"}/>;
};