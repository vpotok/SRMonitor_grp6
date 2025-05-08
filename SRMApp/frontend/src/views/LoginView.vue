<template>
  <div>
    <input v-model="username" placeholder="Username" />
    <input v-model="password" placeholder="Password" type="password" />
    <button @click="handleLogin">Login</button>
    <p v-if="error">{{ error }}</p>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { loginUser } from '../services/authService' // Ensure path is correct
import { useRouter } from 'vue-router'

const username = ref('')
const password = ref('')
const error = ref('')
const router = useRouter()

const handleLogin = async () => {
  error.value = ''; // Clear previous errors
  try {
    // loginUser now calls the mock backend, which sets the HttpOnly cookie.
    // The function waits for the API call to complete.
    await loginUser({ username: username.value, password: password.value });

    // On successful response (even if data is minimal), the cookie is set by the mock.
    // The browser will automatically include this cookie in the next request (like the dashboard route).
    console.log("Login successful, redirecting...");
    router.replace('/dashboard'); // Redirect on success

  } catch (err) {
    console.error("Login failed in component:", err.message);
    // Display the error message from the authService (e.g., 'Invalid credentials')
    error.value = err.message;
  }
}
</script>