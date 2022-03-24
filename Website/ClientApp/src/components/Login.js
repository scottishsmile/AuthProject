import { useRef, useState, useEffect } from 'react';
import useAuth from '../hooks/useAuth';                                         // AuthContext and useContext to access the Auth token data.
import { Link, useNavigate, useLocation } from 'react-router-dom';
import Validation from '../Validation/Validation.js';
import Loading from './LoadingSpinner/Loading';                                     // Loading spinner.

// API Urls
const BASE_URL = 'https://localhost:44331/';
const LOGIN_URL = 'Auth/Login';

const Login = () => {

    // Authentication
    // AuthContext and useContext to access the Auth token data.
    // Use setAuth to save the login token to the global auth state.
    const { setAuth } = useAuth();


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
    const [pwd, setPwd] = useState('');
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
        const passValidation= Validation(pwd);

        // User Input Validation
        // Check if the validation returns true, and notify user of the error.
        if (usernameValidation || passValidation) {
            setErrMsg("Validation Error. We don't like that username or password.");
            return;
        }

        try {
            // POST FetchAPI Request
            // The data must match the API's UserLoginDto which needs Email, Username, Password
            const userInfo = { Username: user, Password: pwd };
            const fetchUrl = BASE_URL + LOGIN_URL;

            fetch(fetchUrl, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(userInfo),
                })
                .then(response => response.json())
                .then(res => {

                    // Save Auth Token
                    /*
                        UserLoginDto supplies token as "data.token"
                        {
                        "data": {
                            "token": "eyJhbGciOi...",
                            "id": 0,
                            "email": "bhyyh@yahoo.net",
                            "username": "bhyyh",
                            "role": "basic"
                        },
                        "success": true,
                        "message": null
                        }
                    */
                    const data = res?.data;           // ? is optional chaining. Assign variable conditionally. If there's no data don't assign the variable, leave it as undefined. Stops exceptions.
                    const role = res?.data?.role;        // Save the role, so we can compare against allowedRoles in RequireAuth.js
                    setAuth({ user, pwd, role, data });      // Save token to global state using context API.

                    // Fetch considers a 404 not found error a success so it will continue.
                    // Usually we would check the response headers for a 200 OK
                    // const status = data.headers.get("status");
                    // But it is showing undefined, so we will just check the API's success variable for true.

                    if(res.success === true){
                        navigate(from, { replace: true });          // Navigate user to the page they were going to (default home page) and remember where they came from.
                    } else {
                        setErrMsg(res.message);             // Display API error message
                        setloading(false);                      // Disable Loading Spinner
                    }

                })
                .catch((error) => {
                    console.error('Fetch Error: ', error);
                    setErrMsg('Fetch Error - Login Failed.');
                    setloading(false);                      // Disable Loading Spinner
                    errRef.current.focus();
                });

            setUser('');
            setPwd('');

        } catch (err) {
            // ? is optional chaining. Assign variable conditionally. If there's no errors don't assign the variable, leave it as undefined. Stops exceptions.
            if (!err?.response) {
                setErrMsg('No Server Response');
            } else if (err.response?.message != null) {
                setErrMsg(err.response.message);                // Display API's error message
            } else if (err.response?.status === 400) {
                setErrMsg('Missing Username or Password');
            } else if (err.response?.status === 401) {
                setErrMsg('Unauthorized');
            } else {
                setErrMsg('Error. Login Failed');
            }
            setloading(false);                          // Disable Loading Spinner
            errRef.current.focus();                     // Set focus on error so screen reader can announce it.
        }
    }



    // Aria-live is for audio screen readers for blind users.
    // errRef will focus on the error message and aria will announce it in an assertive tone.
    // No aria input help here as it is a login and password form, they should know it. There's no specific characters to use etc that was done in registration.

    return(
        <section>
            <p ref={errRef} className={errMsg ? "errmsg" : "offscreen"} aria-live="assertive">{errMsg}</p>
            <h1>Sign In</h1>
            {loading ? <Loading /> :
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

                    <label htmlFor="password">Password:</label>
                    <input
                        type="password"
                        id="password"
                        onChange={(e) => setPwd(e.target.value)}
                        value={pwd}
                        required
                    />
                    <button>Sign In</button>
                    <p>
                        Need an Account?<br />
                        <span className="line">
                            <Link to="/register">Sign Up</Link>
                        </span>
                    </p>
                    <br />
                    <p>
                        Forgot Password?<br />
                        <span className="line">
                            <Link to="/forgotpass">Reset</Link>
                        </span>
                    </p>
                </form>
            }
            <br />
            <div className="admin-dotted-div">
                <p><b>Admin Tools</b></p>
                <p>REMOVE this link when in production, no one needs to know the admin url.</p>
                <p><a href='https://localhost:44378/Brains'>Admin Login</a></p>
                <p><a href='https://localhost:44331/'>Swagger API</a></p>
            </div>
        </section>
    )
}

export default Login;