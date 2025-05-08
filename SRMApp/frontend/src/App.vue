<template>
  <div id="app">
    <router-view />
  </div>
</template>

<script setup>
// No script needed for this basic App.vue
</script>

<style>
/* Global styles */
html, body, #app {
  height: 100%;
  margin: 0;
  padding: 0;
  display: flex;             /* Enables flexbox on the root */
  align-items: center;       /* Vertically center content */
  justify-content: center;   /* Horizontally center content */
}

#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
  line-height: 1.5;
  width: 100%;               /* Ensure full width */
}
</style>
