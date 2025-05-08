<template>
    <div>
      <h1>Admin Dashboard</h1>
      <DeviceList :devices="deviceList" :isAdmin="true" />
    </div>
  </template>
  
  <script setup>
  import { ref, onMounted } from 'vue'
  import DeviceList from '../components/DeviceList.vue'
  import { fetchDevices } from '../services/device'
  
  const deviceList = ref([])
  
  onMounted(async () => {
    deviceList.value = await fetchDevices()
  })
  </script>
  