<template>
  <div class="login-container">
    <h2>Login</h2>
    <form @submit.prevent="handleLogin">
      <div>
        <label for="username">Username:</label>
        <input type="text" v-model="username" id="username" required />
      </div>
      <div>
        <label for="password">Password:</label>
        <input type="password" v-model="password" id="password" required />
      </div>
      <button type="submit">Login</button>
      <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import AuthService from '@/services/authService'; // Use @ alias for src

const username = ref('');
const password = ref('');
const errorMessage = ref('');
const router = useRouter();

const handleLogin = async () => {
  errorMessage.value = ''; // Clear previous errors
  try {
    const userData = await AuthService.login({ username: username.value, password: password.value });
    console.log('Login successful:', userData);
    // MVP: Navigate based on hardcoded roles for now
    if (userData && userData.user && userData.user.role === 'admin') {
      router.push('/admin');
    } else if (userData && userData.user && userData.user.role === 'customer') {
      router.push('/customer');
    } else {
      // Fallback, should not happen with mock, but good practice
      console.warn('Login successful but unexpected role:', userData ? userData.user : 'no user data');
      router.push('/');
    }
  } catch (error) {
    console.error("Login failed:", error);
    // Display a user-friendly message
    errorMessage.value = error.response && error.response.data && error.response.data.message
                           ? error.response.data.message
                           : 'Login failed. Please check your credentials.';
  }
};
</script>

<style scoped>
.login-container {
  max-width: 300px;
  margin: 50px auto;
  padding: 20px;
  border: 1px solid #ccc;
  border-radius: 5px;
  text-align: left; /* Align form elements to the left */
}
.login-container h2 {
    text-align: center; /* Center the heading */
    margin-bottom: 20px;
}
.error {
  color: red;
  margin-top: 10px;
  text-align: center;
}
div {
  margin-bottom: 10px;
}
label {
    display: block; /* Labels on their own line */
    margin-bottom: 5px;
    font-weight: bold;
}
input[type="text"],
input[type="password"] {
    width: 100%; /* Make inputs take full width */
    padding: 8px;
    border: 1px solid #ccc;
    border-radius: 4px;
    box-sizing: border-box; /* Include padding and border in the element's total width and height */
}
button[type="submit"] {
    display: block; /* Button on its own line */
    width: 100%; /* Make button take full width */
    padding: 10px;
    background-color: #007bff;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 16px;
    margin-top: 15px;
}
button[type="submit"]:hover {
    background-color: #0056b3;
}
</style>