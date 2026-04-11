import AccountListPage from '@/admin pages/AccountListPage.vue'
import AddAccountPage from '@/admin pages/AddAccountPage.vue'
import DashboardPage from '@/admin pages/DashboardPage.vue'
import AdminLayout from '@/layouts/AdminLayout.vue'
import MainLayout from '@/layouts/MainLayout.vue'
import ErrorPage from '@/pages/ErrorPage.vue'
import HomePage from '@/pages/HomePage.vue'
import LoginPage from '@/pages/LoginPage.vue'
import RegisterPage from '@/pages/RegisterPage.vue'
import UserProfile from '@/pages/UserProfile.vue'
import VerifyEmail from '@/pages/VerifyEmail.vue'
import {
  useAuthStore
} from '@/stores/auth'
import {
  createRouter,
  createWebHistory
} from 'vue-router'

const router = createRouter({
  history: createWebHistory(
    import.meta.env.BASE_URL),
  routes: [{
      path: '/',
      redirect: '/home'
    },
    {
      path: '/error',
      component: ErrorPage
    },
    {
      path: '/verify-email',
      component: VerifyEmail,
      meta: {
        title: "Verify Email"
      }
    },
    {
      path: '/',
      component: MainLayout,
      children: [{
          path: '/home',
          component: HomePage,
          meta: {
            title: "Home"
          }
        },
        {
          path: '/profile/:id/',
          component: UserProfile,
          meta: {
            title: "My Profile"
          }
        }
      ]
    },
    {
      path: '/admin',
      component: AdminLayout,
      meta: { requiresAuth: true, roles: ['Admin'] },
      redirect: '/admin/dashboard',
      children: [
        {
          path: 'dashboard',
          component: DashboardPage,
          meta: { title: "Dashboard" }
        },
        {
          path: 'accounts',
          component: AccountListPage,
          meta: { title: "Account List" }
        },
        {
          path: 'accounts/add',
          component: AddAccountPage,
          meta: { title: "Add Account" }
        }
      ]
    },
    {
      path: '/login',
      component: LoginPage,
      meta: {
        title: "Login",
        guessOnly: true
      }
    },
    {
      path: '/register',
      component: RegisterPage,
      meta: {
        title: "Register",
        guessOnly: true
      }
    },
  ],
})

router.afterEach((to) => {
  document.title = to.meta.title || 'Étudify'
})

router.beforeEach((to) => {
  const auth = useAuthStore();

  if (to.meta.requiresAuth && !auth.user) {
    return {
      path: '/login',
      query: { redirect: to.fullPath }
    }
  }

  if (to.meta.roles) {
    if (!to.meta.roles.includes(auth.user?.role)) {
      return '/error';
    }
  }

  if (to.meta.guessOnly && auth.user) {
    switch (auth.user?.role) {
      case 'Admin':
        return '/admin/dashboard';
      case 'Student':
        return '/home';
      case 'Instructor':
        return '/instructor/student-list';
      default:
        return '/';
    }
  }

  return true;
})

export default router
