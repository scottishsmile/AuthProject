import { useNavigate, Link } from "react-router-dom";
import useAuth from "../../hooks/useAuth";


const UserProfile = () => {

    // Auth Token
    const { auth } = useAuth();                         // Get the saved auth data

    const navigate = useNavigate();

    return (
        <article style={{ padding: "100px" }}>
            <h1>My Profile</h1>
            <p>Username: <span className="purpleText">{auth?.data?.username}</span></p>
            <p>Email: <span className="purpleText">{auth?.data?.email}</span></p>
            <p>Membership Level: <span className="purpleText">{auth?.data?.role}</span></p>
            <br />
            <div className="flexGrow">
                <Link to="/forgotpass">[Change Password]</Link>
            </div>
            < br />
            <div className="flexGrow">
                <Link to="/">Main</Link>
            </div>
        </article>
    )
}

export default UserProfile;