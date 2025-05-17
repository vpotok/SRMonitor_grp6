import axios from 'axios'

const API_BASE_URL = 'http://localhost:5001/api/CoreService/logs'

export async function fetchDeviceLogs() {
  const response = await axios.get(API_BASE_URL, { withCredentials: true })
  return response.data
}