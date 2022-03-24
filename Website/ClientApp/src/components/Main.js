import { useNavigate, Link } from "react-router-dom";
import { useContext } from "react";
import AuthContext from "../context/AuthProvider";

const Main = () => {

    const { setAuth } = useContext(AuthContext);
    const navigate = useNavigate();
    
    const logout = async () => {
        setAuth({});                // Delete jwt token and data from memory
        navigate('/login');
    }

    return (
        <article style={{ padding: "100px" }}>
            <h1>Main</h1>
            <p>Your app here!</p>
            <div className="flexGrow">
                <Link to="/payment">Buy Premium Access!</Link>
            </div>
            <br />
            <div className="flexGrow">
                <Link to="/premium">Premium Users</Link>
            </div>
            < br/>
            <div className="flexGrow">
                <Link to="/user/profile">[Your Profile]</Link>
            </div>
            <div className="flexGrow">
                <button onClick={logout}>Log Out</button>
            </div>
        </article>
    )
}

export default Main;