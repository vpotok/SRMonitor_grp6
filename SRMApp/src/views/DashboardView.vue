<template>
  <div>
    <!-- Navbar -->
    <nav class="navbar">
      <div class="navbar-left">
        <button @click="logout" class="btn">Logout</button>
        <button @click="viewProfile" class="btn">Profile</button>
      </div>
      <div class="navbar-right">
        <button
          v-if="role === 'admin'"
          @click="activeTab = 'admin'"
          :class="{ active: activeTab === 'admin' }"
          class="btn"
        >
          Admin Panel
        </button>
        <button
          v-if="role === 'manager'"
          @click="activeTab = 'manager'"
          :class="{ active: activeTab === 'manager' }"
          class="btn"
        >
          Manager Panel
        </button>
        <button
          v-if="role === 'user'"
          @click="activeTab = 'user'"
          :class="{ active: activeTab === 'user' }"
          class="btn"
        >
          User Panel
        </button>
      </div>
    </nav>

    <!-- Content -->
    <div class="content">
      <h1 v-if="activeTab === 'admin'">Admin Dashboard</h1>
      <h1 v-else-if="activeTab === 'manager'">Manager Dashboard</h1>
      <h1 v-else-if="activeTab === 'user'">User Dashboard</h1>
      <h1 v-else>Unknown Role</h1>

      <!-- Display devices only for admin and manager -->
      <DeviceList
        v-if="(role === 'admin' || role === 'manager') && activeTab !== 'user'"
        :devices="deviceList"
        :isAdmin="role === 'admin'"
      />

      <!-- Display a message for users without access to devices -->
      <p v-else-if="activeTab === 'user'">You do not have access to device information.</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import DeviceList from '../components/DeviceList.vue'
import { fetchDevices } from '../services/device'
import { logoutUser, getUserRole } from '../services/authService'

const deviceList = ref([])
const role = ref('')
const activeTab = ref('') // Tracks the currently active tab

onMounted(async () => {
  try {
    // Fetch the user's role
    role.value = await getUserRole()

    // Set the default active tab based on the user's role
    activeTab.value = role.value

    // Fetch devices only if the user is an admin or manager
    if (role.value === 'admin' || role.value === 'manager') {
      deviceList.value = await fetchDevices()
    }
  } catch (error) {
    console.error('Failed to fetch data:', error)
    alert('Failed to load data. Please try again later.')
    window.location.href = '/' // Redirect to login page
  }
})

const logout = async () => {
  await logoutUser()
}

const viewProfile = () => {
  alert('Profile functionality is not implemented yet.')
}
</script>

<style scoped>
.navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px;
  background-color: #f8f9fa;
  border-bottom: 1px solid #ddd;
}

.navbar-left,
.navbar-right {
  display: flex;
  gap: 10px;
}

.btn {
  padding: 8px 12px;
  border: none;
  background-color: #007bff;
  color: white;
  cursor: pointer;
  border-radius: 4px;
  font-size: 14px;
}

.btn:hover {
  background-color: #0056b3;
}

.btn.active {
  background-color: #0056b3;
}

.content {
  padding: 20px;
}
</style>