import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '@/views/LoginView.vue'
import DashboardView from '@/views/DashboardView.vue'
import { getUserRole } from '@/services/authService'

const routes = [
  { path: '/', component: LoginView },
  {
    path: '/dashboard',
    component: DashboardView,
  },
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router