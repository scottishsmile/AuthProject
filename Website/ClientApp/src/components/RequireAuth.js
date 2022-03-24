import { useLocation, Navigate, Outlet } from "react-router-dom";
import useAuth from "../hooks/useAuth";

console.log("RequireAuth.js - Authenticating....");

// Pass in roles like basic, premium or admin as a requirement to access a route.
const RequireAuth = ({ allowedRoles }) => {
    const { auth } = useAuth();                         // Get the JWT auth token.
    const location = useLocation();                     // Remember what page the user came from.

    // allowedRoles?.includes(auth?.role)    Is our user's role in the list of allowed roles for this page?

    // If we have a role/jwt token then load the <Outlet /> (this is from Layout.js and renders child routes) which loads the rest of the app's routes.

    // If we have an <Outlet /> then check there's a username in the auth data.

    // If there's a username then the user has already logged in, but their role doesn't allow them access so show "unauthorized".
    // Navigate to unauthorized.js .
    //          Remember the location (previous page) the user came from and replace the url to send the user back there. User can press back button on browser and it'll work.
    //          location is also used to remember where we want to send the user if the login page interupts the flow.
    // else
    // Navigate to login.js . Send the user to login page.
    //          The user may have been sent to login from another page while inside the app.
    //          So remember that location (previous page) and replace the url after login to send the user back there. User can press back button on browser and it'll work.
    //          location is also used to remember where we want to send the user if the login page interupts the flow.

    return (

        allowedRoles?.includes(auth?.role)
            ? <Outlet />
            : auth?.data?.username
                ? <Navigate to="/unauthorized" state={{ from: location }} replace />
                : <Navigate to="/login" state={{ from: location }} replace />
    );
}

export default RequireAuth;