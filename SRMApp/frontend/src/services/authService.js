import axios from 'axios'
import { jwtDecode } from 'jwt-decode'


export async function loginUser(credentials) {
  const response = await axios.post('/api/login', credentials)
  console.log('Response:', response)
  const token = response.data.token
  localStorage.setItem('token', token)
  return token
}

export function getUserRole() {
  const token = localStorage.getItem('token')
  console.log('Token:', token)
  if (!token) return null
  const decoded = jwtDecode(token)
  console.log('decoded:', decoded)
  return decoded.role
}

export function isAuthenticated() {
  return !!localStorage.getItem('token')
}

export function logoutUser() {
  localStorage.removeItem('token')
}
