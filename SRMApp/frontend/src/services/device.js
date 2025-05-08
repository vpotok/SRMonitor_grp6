// src/services/device.js
import axios from 'axios'

const API_URL = import.meta.env.VITE_BACKEND_API_URL || 'http://localhost:3000'

// Assumes the JWT is stored in localStorage after login
const getAuthHeader = () => {
  const token = localStorage.getItem('token')
  return { Authorization: `Bearer ${token}` }
}

export async function fetchDevices() {
  try {
    //const response = await axios.get(`${API_URL}/devices`, {
    const response = await axios.get('/api/devices', {
      headers: getAuthHeader()
    })
    return response.data
  } catch (error) {
    console.error('Failed to fetch devices:', error)
    return []
  }
}

export async function updateDeviceAction(deviceId, action) {
  try {
    const response = await axios.post(
      `${API_URL}/devices/${deviceId}/action`,
      { action },
      { headers: getAuthHeader() }
    )
    return response.data
  } catch (error) {
    console.error(`Failed to update device ${deviceId}:`, error)
    throw error
  }
}
