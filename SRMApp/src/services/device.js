import axios from 'axios'

export async function fetchDevices() {
  try {
    const response = await axios.get('http://localhost:5266/api/devices', {
      withCredentials: true, // Include cookies (auth_token and refresh_token)
    })
    return response.data
  } catch (error) {
    if (error.response && error.response.status === 401) {
      // Redirect to login if unauthorized
      window.location.href = '/'
    }
    console.error('Failed to fetch devices:', error)
    throw error
  }
}