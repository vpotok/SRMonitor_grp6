import axios from 'axios'

const API_BASE_URL = 'http://localhost:5001/api/IP'

// Fetch all IPs
export async function fetchIps() {
  const response = await axios.get(API_BASE_URL, { withCredentials: true })
  return response.data
}

export async function handleAddIp(ip) {
  // Simple IPv4 regex
  const ipv4Regex = /^(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)){3}$/;
  if (!ipv4Regex.test(ip)) {
    throw new Error('Invalid IPv4 address');
  }

  const response = await axios.post(
    API_BASE_URL,
    `"${ip}"`,
    {
      withCredentials: true,
      headers: { 'Content-Type': 'application/json' }
    }
  );
  return response.data;
}
// Delete an IP
export async function handleDeleteIp(id) {
  const response = await axios.delete(`${API_BASE_URL}/${id}`, { withCredentials: true })
  return response.data
}