import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import CustomerDashboardView from '../views/CustomerDashboardView.vue'
import AdminDashboardView from '../views/AdminDashboardView.vue'
import { getUserRole, isAuthenticated } from '../services/authService.js'

const routes = [
  {
    path: '/',
    name: 'Login',
    component: LoginView
  },
  {
    path: '/dashboard',
    name: 'DashboardRedirect',
    beforeEnter: (to, from, next) => {
      if (!isAuthenticated()) {
        return next('/') // Redirect to Login if not authenticated
      }

      const role = getUserRole()

      if (role === 'admin') return next('/admin')
      if (role === 'customer') return next('/customer')

      return next('/') // Default fallback
    }
  },
  {
    path: '/customer',
    name: 'CustomerDashboard',
    component: CustomerDashboardView
  },
  {
    path: '/admin',
    name: 'AdminDashboard',
    component: AdminDashboardView
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router