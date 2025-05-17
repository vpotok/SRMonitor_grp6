import { createApp } from 'vue'; // 1. Import the createApp function
import App from './App.vue'; // 2. Import your root component App.vue
import router from './router'; // 3. Import your configured router instance

// 4. Create the Vue application instance, using App.vue as the root component
const app = createApp(App);

// 5. Tell the application to use the router
app.use(router);

// 6. Mount the application to the HTML element with the id 'app'
// This element is typically found in your public/index.html file
app.mount('#app');

console.log('Vue application created and mounted.'); // Optional: add a log to confirm it runs