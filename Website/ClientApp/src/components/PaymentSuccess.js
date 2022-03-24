import { Link, useNavigate } from 'react-router-dom';
import useAuth from "../hooks/useAuth";
import { useContext } from "react";
import AuthContext from "../context/AuthProvider";


const PaymentSuccess = () => {

    const { auth } = useAuth();                         // Get the saved auth data

    const { setAuth } = useContext(AuthContext);

    const navigate = useNavigate();                     // Navigation

    const logout = async () => {
        setAuth({});                // Delete jwt token and data from memory
        navigate('/login');
    }


    return (
        <article style={{ padding: "100px" }}>
            <h1>Success</h1>
            <p>You are now a premium user!</p>
            <p>Please login again to access the premium area:</p>
            <div className="flexGrow">
                <button onClick={logout}>Log Out</button>
            </div>
        </article>
    )
}

export default PaymentSuccess;