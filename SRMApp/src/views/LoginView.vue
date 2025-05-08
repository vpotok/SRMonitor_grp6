<template>
  <form @submit.prevent="login">
    <input v-model="username" placeholder="Username" required />
    <input v-model="password" type="password" placeholder="Password" required />
    <button type="submit">Login</button>
  </form>
</template>

<script>
export default {
  data() {
    return { username: '', password: '' };
  },
  methods: {
    async login() {
      const res = await fetch("/auth/login", {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username: this.username, password: this.password })
      });
      if (res.ok) alert("Logged in!");
      else alert("Login failed.");
    }
  }
};
</script>