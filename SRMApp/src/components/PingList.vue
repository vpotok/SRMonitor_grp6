<template>
  <div class="pinglist-container">
    <h2>Ping List Management</h2>

    <!-- Add/Edit IP Form -->
    <form @submit.prevent="handleSubmit" class="add-ip-form">
      <input v-model="newIp" placeholder="Enter IP Address" required />
      <button type="submit" class="btn">{{ editingIndex !== null ? 'Update IP' : 'Add IP' }}</button>
    </form>

    <!-- IP List -->
    <ul class="ip-list">
      <li v-for="(ip, index) in ipList" :key="index" class="ip-item">
        <span>{{ ip }}</span>
        <button @click="deleteIp(ip)" class="btn delete">Delete</button>
      </li>
    </ul>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { fetchIps, handleAddIp, handleDeleteIp } from '@/services/pingService.js'

const ipList = ref([])
const newIp = ref('')
const editingIndex = ref(null)

// Load IPs from backend
const loadIps = async () => {
  try {
    ipList.value = await fetchIps()
  } catch (error) {
    alert('Failed to load IPs')
  }
}

onMounted(loadIps)

// Handle form submission for adding or updating an IP
const handleSubmit = async () => {
  try {
    if (editingIndex.value !== null) {
      // Update existing IP
      const ipToUpdate = ipList.value[editingIndex.value]
      await handleUpdateIp(ipToUpdate.id, newIp.value)
      await loadIps()
      editingIndex.value = null
    } else {
      // Add new IP
      await handleAddIp(newIp.value)
      await loadIps()
    }
    newIp.value = ''
  } catch (error) {
    alert('failed to save ip')
  }
}

// Edit an IP
const editIp = (index) => {
  newIp.value = ipList.value[index].ip
  editingIndex.value = index
}

// Delete an IP
const deleteIp = async (id) => {
  try {
    await handleDeleteIp(id)
    await loadIps()
  } catch (error) {
    alert('failed to delete ip')
  }
}
</script>

<style scoped>
/* ...existing styles... */
.pinglist-container {
  padding: 20px;
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
  margin-top: 20px;
}

.add-ip-form {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
}

.add-ip-form input {
  flex: 1;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.add-ip-form .btn {
  padding: 10px 15px;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.add-ip-form .btn:hover {
  background-color: #0056b3;
}

.ip-list {
  list-style: none;
  padding: 0;
}

.ip-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
  margin-bottom: 10px;
}

.ip-item .btn {
  margin-left: 10px;
  padding: 5px 10px;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.ip-item .btn.edit {
  background-color: #ffc107;
  color: white;
}

.ip-item .btn.edit:hover {
  background-color: #e0a800;
}

.ip-item .btn.delete {
  background-color: #dc3545;
  color: white;
}

.ip-item .btn.delete:hover {
  background-color: #c82333;
}
</style>