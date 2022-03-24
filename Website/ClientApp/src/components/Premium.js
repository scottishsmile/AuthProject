import { Link } from "react-router-dom";

const Premium = () => {
    return (
        <article style={{ padding: "100px" }}>
            <h1>Premium Users Section</h1>
            <p>Only premium users allowed here!</p>
            <div className="flexGrow">
                <Link to="/">Main Page</Link>
            </div>
        </article>
    )
}

export default Premium;