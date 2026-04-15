 import React, { useState } from 'react';

const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');

  const validate = () => {
    if (!email.includes('@')) {
      setError('Please enter a valid email address.');
      return false;
    }
    if (password.length < 6) {
      setError('Password must be at least 6 characters long.');
      return false;
    }
    setError('');
    return true;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validate()) {
      console.log("Logging in with:", email, password);
      // Backend data submission logic goes here
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-blue-50">
      <div className="bg-white p-8 rounded-lg shadow-lg w-full max-w-md border-t-4 border-green-500">
        <h2 className="text-3xl font-bold text-center text-gray-800 mb-6">Login</h2>
        
        {error && <p className="bg-red-100 text-red-600 p-2 rounded mb-4 text-sm">{error}</p>}

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-gray-700">Email Address</label>
            <input 
              type="email" 
              className={`w-full px-4 py-2 mt-2 border rounded-md focus:outline-none focus:ring-2 ${error.includes('email') ? 'border-red-500' : 'focus:ring-blue-400'}`}
              placeholder="example@mail.com"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
            />
          </div>
          <div>
            <label className="block text-gray-700">Password</label>
            <input 
              type="password" 
              className={`w-full px-4 py-2 mt-2 border rounded-md focus:outline-none focus:ring-2 ${error.includes('Password') ? 'border-red-500' : 'focus:ring-blue-400'}`}
              placeholder="********"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
            />
          </div>
          <button 
            type="submit" 
            className="w-full bg-green-600 text-white py-2 rounded-md hover:bg-green-700 transition duration-300 font-semibold mb-4"
          >
            Login
          </button>
          
          <div className="text-center">
            <a href="/Auth/Register" className="text-blue-600 hover:underline text-sm font-medium">
              Don't have an account? Register here.
            </a>
          </div>
        </form>
      </div>
    </div>
  );
};

export default Login;