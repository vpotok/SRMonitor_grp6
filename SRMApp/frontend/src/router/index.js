import { createRouter, createWebHistory } from 'vue-router';
import LoginView from '../views/LoginView.vue';
import AdminDashboardView from '../views/AdminDashboardView.vue';
import CustomerDashboardView from '../views/CustomerDashboardView.vue';
import AuthService from '../services/authService'; // For route guards

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: LoginView,
  },
  {
    path: '/admin',
    name: 'AdminDashboard',
    component: AdminDashboardView,
    meta: { requiresAuth: true, role: 'admin' },
  },
  {
    path: '/customer',
    name: 'CustomerDashboard',
    component: CustomerDashboardView,
    meta: { requiresAuth: true, role: 'customer' },
  },
  { // Redirect root to login for MVP
    path: '/',
    redirect: '/login',
  },
  // Optional: Catch all other routes and redirect to login or a 404 page
  // {
  //   path: '/:catchAll(.*)',
  //   redirect: '/login' // Or a dedicated 404 page
  // }
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

// MVP Route Guard
router.beforeEach((to, from, next) => {
  const loggedInUser = AuthService.getCurrentUser();
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth);
  const requiredRole = to.meta.role;

  if (requiresAuth && !loggedInUser) {
    // If the route requires auth and user is not logged in, redirect to login
    console.log('Route requires auth, user not logged in. Redirecting to /login');
    next('/login');
  } else if (requiresAuth && loggedInUser) {
    // If the route requires auth and user is logged in, check role if required
    if (requiredRole && loggedInUser.role !== requiredRole) {
      // If role mismatch, redirect to their respective dashboard or login if no role
      console.log(`User is logged in as ${loggedInUser.role}, but ${requiredRole} is required. Redirecting.`);
      if (loggedInUser.role === 'admin') {
        next('/admin');
      } else if (loggedInUser.role === 'customer') {
        next('/customer');
      } else {
        // Fallback if logged-in user has an unexpected role
        AuthService.logout(); // Clear invalid session
        next('/login');
      }
    } else {
      // If auth is required, user is logged in, and roles match or no specific role is required
      console.log('Route requires auth, user logged in, role matches or not required. Proceeding.');
      next();
    }
  } else if (to.path === '/login' && loggedInUser) {
      // If trying to access login page while already logged in, redirect to appropriate dashboard
      console.log('User is already logged in. Redirecting from /login.');
      if (loggedInUser.role === 'admin') {
          next('/admin');
      } else if (loggedInUser.role === 'customer') {
          next('/customer');
      } else {
          // Fallback for unexpected role
          next('/');
      }
  }
   else {
    // For routes that do not require authentication, allow access
    console.log('Route does not require auth. Proceeding.');
    next();
  }
});

export default router;