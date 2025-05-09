<template>
  <div class="flex items-center justify-center min-h-screen p-6">
    <div class="flex flex-col gap-6 max-w-screen-md w-full">
      <!-- Top: Device Table -->
      <div class="w-full overflow-x-auto">
        <h2 class="text-xl font-semibold mb-4 text-left">Your Devices</h2>
        <div class="bg-white shadow rounded-lg overflow-hidden">
          <table class="min-w-full text-sm">
            <thead class="bg-gray-100 text-left">
              <tr>
                <th class="px-4 py-2">ID</th>
                <th class="px-4 py-2">Installed</th>
                <th class="px-4 py-2">Battery</th>
                <th class="px-4 py-2">Version</th>
                <th class="px-4 py-2">Owner</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="device in devices"
                :key="device.id"
                class="border-b hover:bg-gray-50 cursor-pointer"
                @click="selectDevice(device)"
              >
                <td class="px-4 py-2">{{ device.id }}</td>
                <td class="px-4 py-2">{{ device.installed_date }}</td>
                <td class="px-4 py-2">{{ device.battery_state }}</td>
                <td class="px-4 py-2">{{ device.version }}</td>
                <td class="px-4 py-2">{{ device.owner }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Bottom: Device Detail Card -->
      <div class="w-full">
        <div
          v-if="selectedDevice"
          class="bg-white border border-gray-200 rounded-lg shadow p-6"
        >
          <h3 class="text-lg font-bold mb-4">Device Details</h3>
          <ul class="text-gray-700 space-y-2 text-left">
            <li><strong>ID:</strong> {{ selectedDevice.id }}</li>
            <li><strong>Installed:</strong> {{ selectedDevice.installed_date }}</li>
            <li><strong>Battery:</strong> {{ selectedDevice.battery_state }}</li>
            <li><strong>Version:</strong> {{ selectedDevice.version }}</li>
            <li><strong>Owner:</strong> {{ selectedDevice.owner }}</li>
          </ul>

          <!-- Admin Action -->
          <div v-if="isAdmin" class="mt-6">
            <p class="font-medium mb-2">Admin Action:</p>
            <div class="flex gap-4 mb-4">
              <label class="flex items-center gap-1">
                <input
                  type="radio"
                  value="activate"
                  v-model="selectedAction"
                  class="accent-blue-600"
                />
                Activate
              </label>
              <label class="flex items-center gap-1">
                <input
                  type="radio"
                  value="deactivate"
                  v-model="selectedAction"
                  class="accent-red-600"
                />
                Deactivate
              </label>
            </div>
            <button
              @click="submitCardAction"
              class="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
            >
              Submit Action
            </button>
          </div>
        </div>

        <div v-else class="text-gray-500 italic text-sm mt-4">
          Select a device from the table to view its details.
        </div>
      </div>
    </div>
  </div>
</template>


<script setup>
import { ref } from 'vue'
import { defineProps } from 'vue'

const props = defineProps({
  devices: Array,
  isAdmin: Boolean
})

const selectedDevice = ref(null)
const selectedAction = ref('')

function selectDevice(device) {
  selectedDevice.value = device
  selectedAction.value = '' // Reset previous action
}

function submitCardAction() {
  if (!selectedDevice.value || !selectedAction.value) return
  console.log(`Admin selected "${selectedAction.value}" for device ${selectedDevice.value.id}`)
  // Simulated API call
}
</script>
