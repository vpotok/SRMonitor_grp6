import axios from 'axios'
import { jwtDecode } from 'jwt-decode'

const API_BASE_URL = import.meta.env.VITE_BACKEND_API_URL || 'http://localhost:5266'

// Helper function to retrieve the token from the cookie
function getAuthTokenFromCookie() {
  const cookie = document.cookie
    .split('; ')
    .find(row => row.startsWith('auth_token='))
  const token = cookie ? cookie.split('=')[1] : null

  console.log('Retrieved Token:', token) // Debugging log
  return token
}

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

// Get the user's role from the token
export function getUserRole() {
  const token = getAuthTokenFromCookie()
  console.log('Retrieved Token:', token) // Debugging log
  if (!token) return null

  try {
    const decoded = jwtDecode(token)
    console.log('Decoded Token:', decoded) // Debugging log
    return decoded.role // Extract the "role" claim
  } catch (error) {
    console.error('Failed to decode token:', error)
    return null
  }
}

// Check if the user is authenticated
export function isAuthenticated() {
  return !!getAuthTokenFromCookie() // Return true if the token exists
}

// Logout user by clearing the cookie
export function logoutUser() {
  document.cookie = 'auth_token=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT; Secure; SameSite=Strict; HttpOnly'
}