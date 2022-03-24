import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useRef, useState } from 'react';
import useAuth from "../hooks/useAuth";
import { useContext } from "react";
import AuthContext from "../context/AuthProvider";
import Loading from './LoadingSpinner/Loading';                                     // Loading spinner.

const Payment = () => {

    // API Urls
    const BASE_URL = 'https://localhost:44331/';
    const UPGRADE_URL = 'User/Paid4Premium/';

    // Auth Token
    const { auth } = useAuth();                         // Get the saved auth data
    const { setAuth } = useContext(AuthContext);

    // Navigation
    // Remember where the user was going and came from, should login interrupt the flow
    const navigate = useNavigate();
    const location = useLocation();
    const from = location.state?.from?.pathname || "/";             // default is to send them to home page "/"


    // Set focus
    const errRef = useRef();        // focus on error, useful for the screen readers.

    // State
    const [errMsg, setErrMsg] = useState('');
    const [loading, setloading] = useState(false);

    const buyPremium = async (e) => {
        
        try{
            // PATCH FetchAPI Request
            // We just need to send the username to the API no DTOs
            // https://localhost:44331/User/Paid4Premium/<username>
            // Remove this for production! Don't want people knowing this URL!
            const fetchUrl = BASE_URL + UPGRADE_URL + auth?.data?.username;

            setloading(true);       // Activate loading spinner

            fetch(fetchUrl, {
                method: 'PATCH',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${auth?.data?.token}`
                }})
                .then(response => response.json())
                .then(res => {

                    // Fetch considers a 404 not found error a success so it will continue.
                    // Usually we would check the response headers for a 200 OK
                    // const status = data.headers.get("status");
                    // But it is showing undefined, so we will just check the API's success variable for true.

                    if(res.success === true){
                        navigate('/paymentsuccess');
                    } else {
                        setErrMsg(res.message);             // Display API error message
                    }

                })
                .catch((error) => {
                    console.error('Fetch Error: ', error);
                    setErrMsg('Fetch Error - Payment Failed.');
                    errRef.current.focus();
                });
        } catch (err) {
            // ? is optional chaining. Assign variable conditionally. If there's no errors don't assign the variable, leave it as undefined. Stops exceptions.
            if (!err?.response) {
                setErrMsg('No Server Response');
            } else if (err.response?.message != null) {
                setErrMsg(err.response.message);                // Display API's error message
            } else if (err.response?.status === 400) {
                setErrMsg('Missing User ID...');
            } else if (err.response?.status === 401) {
                setErrMsg('Unauthorized');
            } else {
                setErrMsg('Error. Payment Failed');
            }
            errRef.current.focus();                     // Set focus on error so screen reader can announce it.
        }


    }


    return (
        
        <article style={{ padding: "100px" }}>
            <h1>Payment</h1>
            {loading ? <Loading /> :
                <div>
                <p>Upgrade to our premium service!</p>
                <p>Price: $10 USD.</p>
                <br />
                <p className="purpleText">Simulate a sale:</p>
                <button onClick={buyPremium}>BUY</button>
                <br />
                <p>You can sign up with Paypal or GooglePay to process payments.</p>
                <p>You will have to write any API integration yourself</p>
                </div>
            }
            <div className="flexGrow">
                <Link to="/">Main Page</Link>
            </div>
        </article>
    )
}

export default Payment;