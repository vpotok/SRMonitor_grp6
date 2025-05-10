import { createRouter, createWebHistory } from 'vue-router'
import LoginView from '../views/LoginView.vue'
import DashboardView from '../views/DashboardView.vue'
import { isAuthenticated } from '../services/authService.js'

const routes = [
  {
    path: '/',
    name: 'Login',
    component: LoginView
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: DashboardView,
    beforeEnter: async (to, from, next) => {
      // Check if the user is authenticated
      const authenticated = await isAuthenticated()
      if (!authenticated) {
        return next('/') // Redirect to Login if not authenticated
      }

      next() // Allow access to the dashboard
    }
  },
  {
    path: '/:pathMatch(.*)*', // Catch-all route for unknown paths
    redirect: '/' // Redirect to the login page
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router