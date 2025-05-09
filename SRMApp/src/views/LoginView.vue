<template>
  <form @submit.prevent="login">
    <input v-model="username" placeholder="Username" required />
    <input v-model="password" type="password" placeholder="Password" required />
    <button type="submit">Login</button>
  </form>
</template>

<script>
import { loginUser, getUserRole } from '@/services/authService'

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

        // Redirect based on the user's role
        const role = getUserRole()
        if (role === 'admin') {
          this.$router.push('/admin')
        } else if (role === 'customer') {
          this.$router.push('/customer')
        } else {
          alert('Unknown role')
          console.error('Unknown Role:', role)
        }
      } catch (error) {
        alert('Login failed. Please check your credentials.')
      }
    }
  }
}
</script>