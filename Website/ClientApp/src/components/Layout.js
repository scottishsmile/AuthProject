import { Outlet } from "react-router-dom";    // Outlet compenent covers different child routes.

const Layout = () => {
    return (
        <main className="App">
            <Outlet />
        </main>
    )
}

export default Layout;