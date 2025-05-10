import axios from 'axios'


const API_BASE_URL = import.meta.env.VITE_BACKEND_API_URL || 'http://localhost:5266'

// Login user and set the token in the cookie (already handled by the backend)
export async function loginUser(credentials) {
  try {
    const response = await axios.post(`${API_BASE_URL}/api/auth/login`, credentials, {
      withCredentials: true // Ensure cookies are sent and received
    })
    console.log('Login successful:', response.data.message)
  } catch (error) {
    console.error('Login failed:', error)
    throw error
  }
}

export async function logoutUser() {
  try {
    await axios.post('http://localhost:5266/api/auth/logout', {}, {
      withCredentials: true // Include cookies
    })
    window.location.href = '/' // Redirect to login page
  } catch (error) {
    console.error('Logout failed:', error)
  }
}


export async function getUserRole() {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/auth/role`, {
      withCredentials: true
    });
    return response.data.role;
  } catch (error) {
    if (error.response && error.response.status === 401) {
      await axios.post(`${API_BASE_URL}/api/auth/refresh-token`, {}, {
        withCredentials: true
      });
      const response = await axios.get(`${API_BASE_URL}/api/auth/role`, {
        withCredentials: true
      });
      return response.data.role;
    }
    throw error;
  }
}

// Check if the user is authenticated by validating the token with the backend
export async function isAuthenticated() {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/auth/validate`, {
      withCredentials: true // Include the cookie in the request
    })
    return response.data.isAuthenticated
  } catch (error) {
    console.error('Failed to validate authentication:', error)
    return false
  }
}
