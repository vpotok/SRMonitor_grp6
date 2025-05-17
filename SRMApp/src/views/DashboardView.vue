<template>
  <Navbar />
  <div class="dashboard-container">
    <h1>Dashboard</h1>

    <div v-if="role === 'customeradmin'" class="role-content">
      <h2>Customer Admin Panel</h2>
      <p>Welcome, Customer Admin! You can manage your organization's users.</p>
      <PingList /> 
      <DeviceLogs />
    </div>

    <div v-else-if="role === 'customer'" class="role-content">
      <h2>Customer Dashboard</h2>
      <p>Welcome, Customer! You can view your account details and usage.</p>
      <DeviceLogs />
    </div>

    <div v-else>
      <p>Unauthorized access. Please contact support.</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import Navbar from '@/components/Navbar.vue'
import PingList from '@/components/PingList.vue' 
import { getUserRole } from '@/services/authService'
import DeviceLogs from '@/components/DeviceLogs.vue' 

const role = ref('')

onMounted(async () => {
  try {
    role.value = await getUserRole()
    console.log('User role:', role.value)
  } catch (error) {
    console.error('Failed to fetch user role:', error)
    role.value = 'unauthorized'
  }
})
</script>

<style scoped>
.dashboard-container {
  padding: 35px;
  background-color: #f8f9fa;
  height: 100vh; /* Full viewport height */
  width: 100%; /* Ensure full width */
  box-sizing: border-box; /* Include padding in width/height calculations */
}

.role-content {
  margin-top: 20px;
}

.btn {
  display: inline-block;
  margin: 10px 0;
  padding: 10px 15px;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.btn:hover {
  background-color: #0056b3;
}
</style>