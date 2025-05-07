<template>
    <div>
      <h1>Admin Dashboard (MVP)</h1>
      <p>Welcome, Admin!</p>
      <p>This is a placeholder. Admin-specific content will go here.</p>
      <button @click="handleLogout">Logout</button>
      </div>
  </template>
  
  <script setup>
  import { useRouter } from 'vue-router';
  import AuthService from '@/services/authService'; // Use @ alias for src
  
  const router = useRouter();
  const handleLogout = () => {
    AuthService.logout();
    router.push('/login');
  };
  </script>
  
  <style scoped>
  div {
      margin-top: 20px;
  }
  button {
      margin-top: 20px;
      padding: 10px 20px;
      background-color: #dc3545;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
  }
  button:hover {
      background-color: #c82333;
  }
  </style>