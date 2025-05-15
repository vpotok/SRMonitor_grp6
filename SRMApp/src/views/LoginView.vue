<template>
  <div class="login-container">
    <form @submit.prevent="login" class="login-form">
      <h2>Login</h2>
      <input v-model="username" placeholder="Username" required />
      <input v-model="password" type="password" placeholder="Password" required />

      <div class="preset-buttons">
        <button type="button" @click="fillAdmin('alpha')">Alpha Admin</button>
        <button type="button" @click="fillAdmin('beta')">Beta Admin</button>
      </div>

      <button type="submit" class="btn">Login</button>
    </form>
  </div>
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
        await loginUser({ username: this.username, password: this.password })
        this.$router.push('/dashboard')
      } catch (error) {
        alert('Login failed. Please check your credentials.')
      }
    },
    fillAdmin(company) {
      if (company === 'alpha') {
        this.username = 'alpha_admin'
        this.password = 'admin123'
      } else if (company === 'beta') {
        this.username = 'beta_admin'
        this.password = 'admin123'
      }
    }
  }
}
</script>

<style scoped>
.login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background-color: #f8f9fa;
}

.login-form {
  background: white;
  padding: 30px;
  border-radius: 8px;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
  width: 300px;
  text-align: center;
}

.login-form h2 {
  margin-bottom: 20px;
  font-size: 24px;
  color: #333;
}

.login-form input {
  width: 100%;
  padding: 10px;
  margin-bottom: 15px;
  border: 1px solid #ccc;
  border-radius: 4px;
}

.preset-buttons {
  display: flex;
  justify-content: space-between;
  margin-bottom: 15px;
}

.preset-buttons button {
  flex: 1;
  margin: 0 5px;
  padding: 8px;
  background-color: #6c757d;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.preset-buttons button:hover {
  background-color: #5a6268;
}

.login-form .btn {
  width: 100%;
  padding: 10px;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}

.login-form .btn:hover {
  background-color: #0056b3;
}
</style>
