import { useRef, useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import Validation from '../Validation/Validation.js';
import Loading from './LoadingSpinner/Loading';                                     // Loading spinner.

// API Urls
const BASE_URL = 'https://localhost:44331/';
const RESET_URL = 'Auth/ForgotPass/';

const ForgotPass = () => {

    // Navigation
    // Remember where the user was going and came from, should login interrupt the flow
    const navigate = useNavigate();
    const location = useLocation();
    const from = location.state?.from?.pathname || "/";             // default is to send them to home page "/"


    // Set focus
    const userRef = useRef();       // focus on user input
    const errRef = useRef();        // focus on error, useful for the screen readers.

    // State
    const [user, setUser] = useState('');
    const [errMsg, setErrMsg] = useState('');
    const [loading, setloading] = useState(false);

    // On page load, set focus on user input. blank [] means page load.
    useEffect(() => {
        userRef.current.focus();
    }, [])



    // Form Submit
    const handleSubmit = async (e) => {
        e.preventDefault();

        setloading(true);       // Activate loading spinner

        // Validation
        const usernameValidation = Validation(user);

        // User Input Validation
        // Check if the validation returns true, and notify user of the error.
        if (usernameValidation) {
            setErrMsg("Validation Error. We don't like that username or email.");
            return;
        }

        try {
            // GET FetchAPI Request
            // We just need to send the Email address or Username to the API, no DTOs.
            // We are sending the email/username in the url. https://localhost:44331/Auth/ForgotPass/your@email.com'

            const fetchUrl = BASE_URL + RESET_URL + user;

            fetch(fetchUrl, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }})
                .then(response => response.json())
                .then(res => {

                    // Fetch considers a 404 not found error a success so it will continue.
                    // Usually we would check the response headers for a 200 OK
                    // const status = data.headers.get("status");
                    // But it is showing undefined, so we will just check the API's success variable for true.

                    if(res.success === true){
                        navigate(from, { replace: true });          // Navigate user to the page they were going to (default home page) and remember where they came from.
                    } else {
                        setErrMsg(res.message);             // Display API error message
                        setloading(false);                  // Disable Loading Spinner
                    }

                })
                .catch((error) => {
                    console.error('Fetch Error: ', error);
                    setErrMsg('Fetch Error - Login Failed.');
                    setloading(false);                      // Disable Loading Spinner
                    errRef.current.focus();
                });

                setUser('');

        } catch (err) {
            // ? is optional chaining. Assign variable conditionally. If there's no errors don't assign the variable, leave it as undefined. Stops exceptions.
            if (!err?.response) {
                setErrMsg('No Server Response');
            } else if (err.response?.message != null) {
                setErrMsg(err.response.message);                // Display API's error message
            } else if (err.response?.status === 400) {
                setErrMsg('Missing Email or Username');
            } else if (err.response?.status === 401) {
                setErrMsg('Unauthorized');
            } else {
                setErrMsg('Error. Reset Failed');
            }
            setloading(false);                          // Disable Loading Spinner
            errRef.current.focus();                     // Set focus on error so screen reader can announce it.
        }

    }

    return (
        <article style={{ padding: "100px" }}>
            {loading ? <Loading /> :
            <div>
                <h1>Reset Password</h1>
                <p>A password reset email will be sent to you.</p>
                    <form onSubmit={handleSubmit}>
                        <label htmlFor="username">Email / Username:</label>
                        <input
                            type="text"
                            id="username"
                            ref={userRef}                                           // Set focus on the username field, should happen at page load.
                            autoComplete="off"
                            onChange={(e) => setUser(e.target.value)}               // User input changes the user value via setUser hook.
                            value={user}                                            // We need to assign a value to the form field if we want to clear it.
                            required
                        />

                        <button>Request Reset</button>
                        <p ref={errRef} className={errMsg ? "errmsg" : "offscreen"} aria-live="assertive">{errMsg}</p>
                    </form>
            </div>
            }
            <br />
            <p>
                <br />
                <span className="line">
                    <Link to="/">Back</Link>
                </span>
            </p>
        </article>
    )
}

export default ForgotPass;