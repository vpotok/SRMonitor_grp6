import axios from 'axios'

const API_URL = import.meta.env.VITE_BACKEND_API_URL || 'http://localhost:5266'

// Helper function to retrieve the token from the cookie
function getAuthTokenFromCookie() {
  const cookie = document.cookie
    .split('; ')
    .find(row => row.startsWith('auth_token='))
  return cookie ? cookie.split('=')[1] : null
}

// Fetch devices
export async function fetchDevices() {
  try {
    const token = getAuthTokenFromCookie()
    const response = await axios.get(`${API_URL}/api/protected/devices`, {
      headers: { Authorization: `Bearer ${token}` }
    })
    return response.data
  } catch (error) {
    console.error('Failed to fetch devices:', error)
    return []
  }
}
