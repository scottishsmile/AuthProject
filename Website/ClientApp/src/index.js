import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthProvider';
import registerServiceWorker from './registerServiceWorker';

const rootElement = document.getElementById('root');

ReactDOM.render(
  <BrowserRouter>
    <AuthProvider>
        <Routes>
          <Route path="/*" element={<App />} />
        </Routes>
      </AuthProvider>
  </BrowserRouter>,
  rootElement);

registerServiceWorker();



