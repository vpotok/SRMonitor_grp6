import axios from 'axios'


const API_BASE_URL = 'http://localhost:5001'

// Login user and set the token in the cookie (already handled by the backend)
export async function loginUser(credentials) {
  try {
    const response = await axios.post(`${API_BASE_URL}/api/Auth/login`, credentials, {
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
    await axios.post(`${API_BASE_URL}/api/Auth/logout`, {}, {
      withCredentials: true // Include cookies
    })
    window.location.href = '/' // Redirect to login page
  } catch (error) {
    console.error('Logout failed:', error)
  }
}



export async function getUserRole() {
  try {
    const response = await axios.get(`${API_BASE_URL}/api/Auth/role`, {
      withCredentials: true
    });
    return response.data.role;
  } catch (error) {
    if (error.response && error.response.status === 401) {
      // Not authorized, return null or handle as needed
      return null;
    }
    throw error;
  }
}
