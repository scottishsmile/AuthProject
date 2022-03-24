import { createContext, useState } from "react";

// Make use of React's Context API. We can access the auth token directly rather than passing the it down react layers.
// Easier and more direct than passing data through props from parent to child components.
// Context API is also useful if several elements want access to the same data.


const AuthContext = createContext({});

export const AuthProvider = ({ children }) => {

    const [auth, setAuth] = useState({});                       // The Auth token

    // Surround the App with the AuthProvider tags in index.js to allow everything to have access to the auth data.
    /*
        <React.StrictMode>
            <AuthProvider>
                <App />
            </AuthProvider>
        </React.StrictMode>
    */
   // In index.js use AuthProvider to allow <App /> access to the Auth data.
   // In login.js make use of useContext and AuthContext to actually access the auth data.
   // In login.js -   const { setAuth } = useContext(AuthContext);   - save the login token to the global auth state.

    return (
        <AuthContext.Provider value={{ auth, setAuth }}>
            {children}
        </AuthContext.Provider>
    )
}

export default AuthContext;