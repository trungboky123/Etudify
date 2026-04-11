import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'
import "@fortawesome/fontawesome-free/css/all.min.css";
import "bootstrap/dist/css/bootstrap.min.css";
import "bootstrap-icons/font/bootstrap-icons.css";
import "bootstrap/dist/js/bootstrap.bundle.min.js";
import i18n from './i18n'
import { useAuthStore } from './stores/auth';
const app = createApp(App)

router.beforeEach((to) => {
  document.title = to.meta.title || 'Etudify';
  return true;
})

app.use(createPinia())
const auth = useAuthStore();
await auth.fetchUser();

app.use(router)
app.use(i18n)

app.mount('#app')
