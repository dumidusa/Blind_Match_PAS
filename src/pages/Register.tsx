 import React, { useState } from 'react';

const Register: React.FC = () => {
  const [formData, setFormData] = useState({
    fullName: '',
    email: '',
    password: '',
    role: 'Student'
  });
  const [errors, setErrors] = useState<any>({});

  const validate = () => {
    let tempErrors: any = {};
    if (!formData.fullName) tempErrors.fullName = "Full name is required.";
    if (!formData.email.includes('@')) tempErrors.email = "Please enter a valid email address.";
    if (formData.password.length < 6) tempErrors.password = "Password must be at least 6 characters long.";
    
    setErrors(tempErrors);
    return Object.keys(tempErrors).length === 0;
  };

  const handleChange = (e: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (validate()) {
      console.log("Registering User:", formData);
      // Backend submission logic goes here
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-blue-50">
      <div className="bg-white p-8 rounded-lg shadow-lg w-full max-w-md border-t-4 border-blue-500">
        <h2 className="text-3xl font-bold text-center text-gray-800 mb-6">Create Account</h2>
        
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-gray-700">Full Name</label>
            <input 
              name="fullName"
              type="text" 
              className={`w-full px-4 py-2 mt-2 border rounded-md focus:outline-none focus:ring-2 ${errors.fullName ? 'border-red-500' : 'focus:ring-green-400'}`}
              placeholder="Nadith Athapaththu"
              onChange={handleChange}
            />
            {errors.fullName && <p className="text-red-500 text-xs mt-1">{errors.fullName}</p>}
          </div>

          <div>
            <label className="block text-gray-700">Email Address</label>
            <input 
              name="email"
              type="email" 
              className={`w-full px-4 py-2 mt-2 border rounded-md focus:outline-none focus:ring-2 ${errors.email ? 'border-red-500' : 'focus:ring-green-400'}`}
              placeholder="nadith@example.com"
              onChange={handleChange}
            />
            {errors.email && <p className="text-red-500 text-xs mt-1">{errors.email}</p>}
          </div>

          <div>
            <label className="block text-gray-700">Password</label>
            <input 
              name="password"
              type="password" 
              className={`w-full px-4 py-2 mt-2 border rounded-md focus:outline-none focus:ring-2 ${errors.password ? 'border-red-500' : 'focus:ring-green-400'}`}
              placeholder="********"
              onChange={handleChange}
            />
            {errors.password && <p className="text-red-500 text-xs mt-1">{errors.password}</p>}
          </div>

          <div>
            <label className="block text-gray-700">Select Role</label>
            <select 
              name="role"
              className="w-full px-4 py-2 mt-2 border rounded-md bg-white focus:outline-none focus:ring-2 focus:ring-green-400"
              onChange={handleChange}
            >
              <option value="Student">Student</option>
              <option value="Supervisor">Supervisor</option>
              <option value="ModuleLeader">Module Leader</option>
              <option value="Admin">Admin</option>
            </select>
          </div>

          <button 
            type="submit" 
            className="w-full bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700 transition duration-300 font-semibold mb-4"
          >
            Register
          </button>

          <div className="text-center">
            <a href="/Auth/Login" className="text-green-600 hover:underline text-sm font-medium">
              Already have an account? Login here.
            </a>
          </div>
        </form>
      </div>
    </div>
  );
};

export default Register;