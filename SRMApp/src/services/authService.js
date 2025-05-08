import axios from 'axios'
import { jwtDecode } from 'jwt-decode'

const API_BASE_URL = process.env.VITE_BACKEND_API_URL || 'http://localhost:5266'

export async function loginUser(credentials) {
  try {
    // Send login request to SRMCore
    const response = await axios.post(`${API_BASE_URL}/api/auth/login`, credentials)
    console.log('Response:', response)

    // Extract the token from the response
    const token = response.data.token
    localStorage.setItem('token', token) // Store the token in localStorage
    return token
  } catch (error) {
    console.error('Login failed:', error)
    throw error
  }
}

export function getUserRole() {
  const token = localStorage.getItem('token')
  if (!token) return null
  const decoded = jwtDecode(token)
  return decoded.role
}

export function isAuthenticated() {
  return !!localStorage.getItem('token')
}

export function logoutUser() {
  localStorage.removeItem('token')
}