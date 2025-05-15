<template>
  <div class="device-logs-container">
    <h2>Device Logs</h2>

    <!-- Logs List -->
    <ul class="logs-list">
      <li v-for="(log, index) in logs" :key="index" class="log-item">
        <span>{{ log }}</span>
      </li>
    </ul>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

// Example logs (replace with API call to fetch logs)
const logs = ref([])

onMounted(() => {
  // Simulate fetching logs from an API
  logs.value = [
    'Device 192.168.1.1 is online',
    'Device 10.0.0.1 failed to respond',
    'Device 192.168.1.2 is offline',
    'Device 10.0.0.2 responded successfully'
  ]
})
</script>

<style scoped>
.device-logs-container {
  padding: 20px;
  background-color: #fff;
  border-radius: 8px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
  margin-top: 20px;
}

.logs-list {
  list-style: none;
  padding: 0;
}

.log-item {
  padding: 10px;
  border: 1px solid #ccc;
  border-radius: 4px;
  margin-bottom: 10px;
  background-color: #f8f9fa;
}
</style>