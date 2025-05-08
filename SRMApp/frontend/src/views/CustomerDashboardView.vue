<template>
    <div>
      <h1>Customer Dashboard</h1>
      <DeviceList :devices="deviceList" :isAdmin="false" />
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
  