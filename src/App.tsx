 import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Login from './pages/Login';
import Register from './pages/Register';

function App() {
  return (
    <Router>
      <Routes>
        {/* Default page */}
        <Route path="/" element={<Login />} />
        
        {/* Login routes */}
        <Route path="/login" element={<Login />} />
        <Route path="/Auth/Login" element={<Login />} />
        
        {/* Register routes */}
        <Route path="/register" element={<Register />} />
        <Route path="/Auth/Register" element={<Register />} />
      </Routes>
    </Router>
  );
}

export default App;