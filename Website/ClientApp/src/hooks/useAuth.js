import { useContext } from "react";
import AuthContext from "../context/AuthProvider";

// Custom hook for using authentication.
// We will use useAuth when we want to access the JWT authentication token.

const useAuth = () => {
    return useContext(AuthContext);
}

export default useAuth;