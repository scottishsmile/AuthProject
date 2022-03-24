import Layout from './components/Layout';
import Login from './components/Login';
import Register from './components/Register';
import Main from './components/Main';
import UserProfile from './components/User/Profile';
import ForgotPass from './components/ForgotPass';
import Premium from './components/Premium';
import Payment from './components/Payment';
import PaymentSuccess from './components/PaymentSuccess';
import UnAuthorized401 from './components/UnAuthorized401';
import Missing404 from './components/Missing404';
import RequireAuth from './components/RequireAuth';
import { Routes, Route } from 'react-router-dom';
import './App.css';

// Routes
// The Layout page uses < Outlet /> to display any of the matching route or child routes here.
// <Route   parent route  /mypage  <Mypage />
//    <Route   child route /mypage:id  <MypageId />
//    <Route   child route /mypage:name  <MypageName />
// </Route>

// <Route path="/" element={<Main />} />
// You need to list a "/" path as that's what App.js is looking for intially coming from index.js
// It will be redirected to the login page due to the RequireAuth in the route below.
/*    
      index.js page :
      <Routes>
        <Route path="/*" element={<App />} />
      </Routes>
*/


const App = () => {

  return (
      <Routes>
        <Route path="/" element={<Layout />}>
          {/* public routes */}
          <Route path="login" element={<Login />} />
          <Route path="register" element={<Register />} />
          <Route path="forgotpass" element={<ForgotPass/>} />
          <Route path="unauthorized" element={<UnAuthorized401 />} />

          {/* Requires Authentication */}

            {/* Allow Role = Basic */}
            <Route element={<RequireAuth allowedRoles={["basic", "premium", "admin"]} />}>
              <Route path="/" element={<Main />} />
              <Route path="user/profile" element={<UserProfile />} />
              <Route path="payment" element={<Payment />} />
              <Route path="paymentsuccess" element={<PaymentSuccess />} />
            </Route>

            {/* Allow Role = Premium */}
            <Route element={<RequireAuth allowedRoles={["premium", "admin"]} />}>
              <Route path="premium" element={<Premium />} />
            </Route>

          {/* catch all 404 */}
          <Route path="*" element={<Missing404 />} />
        </Route>
      </Routes>
  );
}

export default App;
