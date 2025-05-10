<template>
  <form @submit.prevent="login">
    <input v-model="username" placeholder="Username" required />
    <input v-model="password" type="password" placeholder="Password" required />
    <button type="submit">Login</button>
  </form>
</template>

<script>
import { loginUser } from '@/services/authService'

export default {
  data() {
    return {
      username: '',
      password: ''
    }
  },
  methods: {
    async login() {
      try {
        // Call the loginUser function from authService
        await loginUser({ username: this.username, password: this.password })

        // Redirect to the dashboard
        this.$router.push('/dashboard')
      } catch (error) {
        alert('Login failed. Please check your credentials.')
      }
    }
  }
}
</script>