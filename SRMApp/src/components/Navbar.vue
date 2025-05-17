<template>
  <nav class="navbar">
    <div class="navbar-left">
      <h1>SRMonitor</h1>
    </div>
    <div class="navbar-right">
      <template v-if="role === 'customeradmin'">
        <button @click="logout" class="btn">Logout</button>
      </template>
      <template v-else-if="role === 'customer'">
        <button @click="logout" class="btn">Logout</button>
      </template>
    </div>
  </nav>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { logoutUser, getUserRole } from '@/services/authService'
import { useRouter } from 'vue-router'

const router = useRouter()
const role = ref('')

onMounted(async () => {
  try {
    role.value = await getUserRole()
  } catch (error) {
    console.error('Failed to fetch user role:', error)
    role.value = 'unauthorized'
  }
})

const navigateTo = (route) => {
  router.push(`/${route}`)
}

const logout = async () => {
  await logoutUser()
}
</script>

<style scoped>
.navbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px 20px; /* Adjusted padding for better spacing */
  background-color: #007bff;
  color: white;
  position: fixed;
  top: 0;
  left: 0;
  width: calc(100% - 40px); /* Prevent content from being cut off */
  margin: 0 20px; /* Added margin to ensure proper alignment */
  z-index: 1000;
  box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
}

.navbar-left h1 {
  margin: 0;
  font-size: 24px;
}

.navbar-right .btn {
  margin-left: 10px;
  padding: 8px 25px;
  background-color: #0056b3;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.navbar-right .btn:hover {
  background-color: #003f7f;
}
</style>