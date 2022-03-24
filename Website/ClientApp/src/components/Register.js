import { useRef, useState, useEffect } from "react";
import {faCheck, faTimes, faInfoCircle } from "@fortawesome/free-solid-svg-icons";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Link } from "react-router-dom";
import Loading from './LoadingSpinner/Loading';                                     // Loading spinner.
import Validation from '../Validation/Validation.js';


// Email Validation
// Accepts subdomains you@subdomain.you.com
// Doesn't accept yoursite..com, two @s, you.com7 
// 4 to 50 characters
const EMAIL_REGEX = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

// Username Validation
// Must be 4 to 50 characters. 
// Start with a lower or uppercase letter [a-zA-Z]
// Then be followed by lower or uppercase letters, digits, hyphens or underscores
const USER_REGEX = /^[A-z][A-z0-9-_]{3,50}$/;


// Password Validation
// Must be 8 to 30 characters. 
// Requires at least 1 lowercase letter (?=.*[a-z])
// Requires at least 1 uppercase letter (?=.*[A-Z])
// Requires at least 1 digit (?=.*[0-9])
// Requires at least 1 special caracter (?=.*[!@#$%^&*])
const PWD_REGEX = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&*]).{8,50}$/;


// API Urls
const BASE_URL = 'https://localhost:44331/';
const REGISTER_URL = 'Auth/Register';

// check password doesn't conain "password" or "abc" etc?

const Register = () => {

    // Focus
    const emailRef = useRef();
    const userRef = useRef();
    const errRef = useRef();

    // Loaing Spinner State
    const [loading, setloading] = useState(false);

    // Email State
    const [email, setEmail] = useState('');
    const [validEmail, setValidEmail] = useState(false);
    const [emailFocus, setEmailFocus] = useState(false);

    // User State
    const [user, setUser] = useState('');
    const [validName, setValidName] = useState(false);
    const [userFocus, setUserFocus] = useState(false);

    // Password State
    const [pwd, setPwd] = useState('');
    const [validPwd, setValidPwd] = useState(false);
    const [pwdFocus, setPwdFocus] = useState(false);

    // Match Password State
    const [matchPwd, setMatchPwd] = useState('');
    const [validMatch, setValidMatch] = useState(false);
    const [matchFocus, setMatchFocus] = useState(false);

    // Newsletter Subscription State
    const [newsletter, setNewsletter] = useState(true);

    // Newsletter - toggle checkbox
    const newsletterChange = () => {
        setNewsletter(!newsletter);
      };

    // Error State
    const [errMsg, setErrMsg] = useState('');
    const [success, setSuccess] = useState(false);


    // Set focus on username input when component loads.
    useEffect(() => {
        userRef.current.focus();
    }, [])

    useEffect(() => {
        setValidEmail(EMAIL_REGEX.test(email));
    }, [email])

    useEffect(() => {
        setValidName(USER_REGEX.test(user));
    }, [user])

    useEffect(() => {
        setValidPwd(PWD_REGEX.test(pwd));
        setValidMatch(pwd === matchPwd);
    }, [pwd, matchPwd])


    // Form submission
    const handleSubmit = async (e) => {
        e.preventDefault();
        // A hacker could enable the submit button using JS, bypassing our live form field validation.
        // This checks to make sure the username and password are actually validated when the submit button is pressed.
        // .test() tests for a match in a string, if it matches it returns true.
        const emailInputCheck = EMAIL_REGEX.test(email);
        const userInputCheck = USER_REGEX.test(user);
        const passInputCheck = PWD_REGEX.test(pwd);

        if (!emailInputCheck || !userInputCheck || !passInputCheck) {
            setErrMsg("Regex Error. Email, Username or Password doesn't meet our criteria. Check for spaces, no @ symbol or a missing dot '.com' in the email address.");
            return;
        }

        // Validation
        const usernameValidation = Validation(user);
        const passValidation = Validation(pwd);

        // User Input Validation
        // Check if the validation returns true, and notify user of the error.
        if (usernameValidation || passValidation) {
            setErrMsg("Validation Error. We don't like that username or password. Avoid weak words like 'test', 'abc' , '123', 'qwerty' and 'pass'. No SQL commands like 'select', 'delete' or filenames like '.exe'");
            return;
        }

        try {
            // POST FetchAPI Request
            // The data must match the API's UserRegisterDto which needs Email, Username, Password, Newsletter
            const data = { Email: email, Username: user, Password: pwd, Newsletter: newsletter };
            const fetchUrl = BASE_URL + REGISTER_URL;

            setloading(true);       // Activate loading spinner

            fetch(fetchUrl, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data),
            })
            .then(response => response.json())
            .then(data => {

                // Fetch considers a 404 not found error a success so it will continue.
                // Usually we would check the response headers for a 200 OK
                // const status = data.headers.get("status");
                // But it is showing undefined, so we will just check the API's success variable for true.

                if(data.success === true){
                    setSuccess(true);           // Success is now true so load success fragment
                    setloading(false);                      // Disable Loading Spinner
                } else {
                    setErrMsg(data.message);        // Display API error message
                    setloading(false);                      // Disable Loading Spinner
                }

            })
            .catch((error) => {
                console.error('Fetch Error: ', error);
                setErrMsg('Fetch Error - Registration Failed.');
                setloading(false);                                  // Disable Loading Spinner
                errRef.current.focus();
            });

            setUser('');
            setPwd('');
            setMatchPwd('');

        } catch(err){
            setErrMsg('Something went wrong! Registration Failed.');
            setloading(false);                      // Disable Loading Spinner
            errRef.current.focus();
        }
    }

    // Visually impared users may have a screen reader to uses audio for the error messages.
    // Use aria-live to tell the screen reader this is dynamic content and to announce it when it changes.
    // Using the "offscreen" css class moves the text outside the screen so it can't be seen, but it will still be available to screen readers for audio.

    // <> is a fragment. Use conditional statement to display Success fragment after form is submitted. setSuccess(true) is inside the form's handleSubmit().

    return (
        <>
            {success ? (
                <section>
                    <h1>Success!</h1>
                    <p>We sent you an email!</p>
                    <p>Validate your email address before signing in.</p>
                    <p>
                        <Link to="/">Sign In</Link>
                    </p>
                </section>
            ) : (
            <section>
                <p ref={errRef} className={errMsg ? "errmsg" : "offscreen"} aria-live="assertive">{errMsg}</p>
                <h1>Register</h1>
                {loading ? <Loading /> :
                    <form onSubmit={handleSubmit}>
                                <label htmlFor="email">
                                    Email:
                                    {/* Green tick and red cross icons to show validation status */}
                                    <FontAwesomeIcon icon={faCheck} className={validEmail ? "valid" : "hide"} />
                                    <FontAwesomeIcon icon={faTimes} className={validEmail || !email ? "hide" : "invalid"} />
                                </label>
                                <input
                                    type="text"
                                    id="email"
                                    ref={emailRef}
                                    autoComplete="off"                                          // No need for browser autocomplete suggestions
                                    onChange={(e) => setEmail(e.target.value)}                   // Ties user input to the state
                                    value={email}
                                    required
                                    aria-invalid={validEmail ? "false" : "true"}                 // Accessibility. Audio. False to begin with, true when username is entered. Reads username out loud.
                                    aria-describedby="emailnote"                                  // Describes the field to the user.
                                    onFocus={() => setEmailFocus(true)}                          // Sharpen field when user clicks.
                                    onBlur={() => setEmailFocus(false)}                          // Blur field when user leaves.
                                />

                                {/* Aria accessibility audio notes */}
                                <p id="emailnote" className={emailFocus && email && !validEmail ? "instructions" : "offscreen"}>
                                    <FontAwesomeIcon icon={faInfoCircle} />
                                    Enter email address. <br />
                                    4 to 50 characters.<br />
                                </p>

                                <label htmlFor="username">
                                    Username:
                                    {/* Green tick and red cross icons to show validation status */}
                                    <FontAwesomeIcon icon={faCheck} className={validName ? "valid" : "hide"} />
                                    <FontAwesomeIcon icon={faTimes} className={validName || !user ? "hide" : "invalid"} />
                                </label>
                                <input
                                    type="text"
                                    id="username"
                                    ref={userRef}
                                    autoComplete="off"                                          // No need for browser autocomplete suggestions
                                    onChange={(e) => setUser(e.target.value)}                   // Ties user input to the state
                                    value={user}
                                    required
                                    aria-invalid={validName ? "false" : "true"}                 // Accessibility. Audio. False to begin with, true when username is entered. Reads username out loud.
                                    aria-describedby="uidnote"                                  // Describes the field to the user.
                                    onFocus={() => setUserFocus(true)}                          // Sharpen field when user clicks.
                                    onBlur={() => setUserFocus(false)}                          // Blur field when user leaves.
                                />

                                {/* Aria accessibility audio notes */}
                                <p id="uidnote" className={userFocus && user && !validName ? "instructions" : "offscreen"}>
                                    <FontAwesomeIcon icon={faInfoCircle} />
                                    Enter username. <br />
                                    4 to 50 characters.<br />
                                    Must begin with a letter.<br />
                                    Letters, numbers, underscores, hyphens allowed.
                                </p>

                                <label htmlFor="password">
                                    Password:
                                    <FontAwesomeIcon icon={faCheck} className={validPwd ? "valid" : "hide"} />
                                    <FontAwesomeIcon icon={faTimes} className={validPwd || !pwd ? "hide" : "invalid"} />
                                </label>
                                <input
                                    type="password"
                                    id="password"
                                    onChange={(e) => setPwd(e.target.value)}
                                    value={pwd}
                                    required
                                    aria-invalid={validPwd ? "false" : "true"}
                                    aria-describedby="pwdnote"
                                    onFocus={() => setPwdFocus(true)}
                                    onBlur={() => setPwdFocus(false)}
                                />
                                <p id="pwdnote" className={pwdFocus && !validPwd ? "instructions" : "offscreen"}>
                                    <FontAwesomeIcon icon={faInfoCircle} />
                                    8 to 30 characters.<br />
                                    Must include uppercase and lowercase letters, a number and a special character.<br />
                                    Allowed special characters: <span aria-label="exclamation mark">!</span> <span aria-label="at symbol">@</span> <span aria-label="hashtag">#</span> <span aria-label="dollar sign">$</span> <span aria-label="percent">%</span>
                                </p>


                                <label htmlFor="confirm_pwd">
                                    Confirm Password:
                                    <FontAwesomeIcon icon={faCheck} className={validMatch && matchPwd ? "valid" : "hide"} />
                                    <FontAwesomeIcon icon={faTimes} className={validMatch || !matchPwd ? "hide" : "invalid"} />
                                </label>
                                <input
                                    type="password"
                                    id="confirm_pwd"
                                    onChange={(e) => setMatchPwd(e.target.value)}
                                    value={matchPwd}
                                    required
                                    aria-invalid={validMatch ? "false" : "true"}
                                    aria-describedby="confirmnote"
                                    onFocus={() => setMatchFocus(true)}
                                    onBlur={() => setMatchFocus(false)}
                                />
                                <p id="confirmnote" className={matchFocus && !validMatch ? "instructions" : "offscreen"}>
                                    <FontAwesomeIcon icon={faInfoCircle} />
                                    Must match the first password input field.
                                </p>

                                <div className={"checkboxdiv"}>
                                    <label htmlFor="newsletter_subscription">
                                        Subscribe to Newsletter?
                                    </label>
                                    <input
                                        type="checkbox"
                                        id="newsletter_subscription"
                                        value="Subscribed"
                                        aria-describedby="newsletternote"
                                        checked={newsletter}                                    // Holds the checkbox state. True by default.
                                        onChange={newsletterChange}                               // Toggles the checkbox value
                                    />
                                </div>
                                <p id="newsletternote" className={"offscreen"}>
                                    <FontAwesomeIcon icon={faInfoCircle} />
                                    Subscribe to our newsletter checkbox. Default is true, checkbox is checked.
                                </p>


                                {/* Only enable button when all fields are sucessfully validated.*/}
                                <button type="submit" disabled={!validName || !validPwd || !validMatch ? true : false}>Sign Up</button>
                            </form>
                        }
                        <p>
                            Already registered?<br />
                            <span className="line">
                                <Link to="/">Sign In</Link>
                            </span>
                        </p>
                        <br />
            </section>
            )}
        </>
    )
}

export default Register;
