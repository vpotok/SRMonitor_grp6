<template>
  <div class="device-logs-container">
    <h2>Device Logs</h2>
    <ul class="logs-list">
      <li v-for="(log, index) in logs" :key="index" class="log-item">
        <div>
          <strong>{{ formatTimestamp(log.timestamp) }}</strong>
          <span class="log-type">[{{ log.type }}]</span>
        </div>
        <div>
          <span v-if="parsedMessage(log.message)">
            ShellyId: {{ parsedMessage(log.message).ShellyId }},
            Temp: {{ parsedMessage(log.message).CurrentTemp }},
            Door: {{ parsedMessage(log.message).DoorOpen ? 'Open' : 'Closed' }},
            KeepAlive: {{ parsedMessage(log.message).KeepAliveTimestamp }}
          </span>
          <span v-else>
            {{ log.message }}
          </span>
        </div>
      </li>
    </ul>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { fetchDeviceLogs } from '@/services/logService'

const logs = ref([])

onMounted(async () => {
  try {
    logs.value = await fetchDeviceLogs()
  } catch (e) {
    logs.value = [{ timestamp: '', type: 'error', message: 'Failed to load logs' }]
  }
})

function parsedMessage(message) {
  try {
    return JSON.parse(message)
  } catch {
    return null
  }
}

function formatTimestamp(ts) {
  return ts ? new Date(ts).toLocaleString() : ''
}
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
.log-type {
  color: #888;
  margin-left: 8px;
}
</style>