import AccountListPage from '@/admin pages/AccountListPage.vue'
import AddAccountPage from '@/admin pages/AddAccountPage.vue'
import AddCoursePage from '@/admin pages/AddCoursePage.vue'
import CourseListPage from '@/admin pages/CourseListPage.vue'
import DashboardPage from '@/admin pages/DashboardPage.vue'
import EditAccountPage from '@/admin pages/EditAccountPage.vue'
import AdminLayout from '@/layouts/AdminLayout.vue'
import MainLayout from '@/layouts/MainLayout.vue'
import ErrorPage from '@/pages/ErrorPage.vue'
import HomePage from '@/pages/HomePage.vue'
import LoginPage from '@/pages/LoginPage.vue'
import MyEnrollmentsPage from '@/pages/MyEnrollmentsPage.vue'
import PublicCourseDetailsPage from '@/pages/PublicCourseDetailsPage.vue'
import PublicCoursesPage from '@/pages/PublicCoursesPage.vue'
import QrPage from '@/pages/QrPage.vue'
import RegisterPage from '@/pages/RegisterPage.vue'
import SetPasswordPage from '@/pages/SetPasswordPage.vue'
import TransactionHistoryPage from '@/pages/TransactionHistoryPage.vue'
import UserProfilePage from '@/pages/UserProfilePage.vue'
import VerifyEmailPage from '@/pages/VerifyEmailPage.vue'
import WishlistPage from '@/pages/WishlistPage.vue'
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
      beforeEnter: async () => {
        const auth = useAuthStore();
        if (!auth.user) {
          await auth.fetchUser();
        }
        switch (auth.user?.role) {
          case "Admin":
            return "/admin/dashboard";
          case "Instructor":
            return "/instructor/students"
          default:
            return "/home"
        }
      }
    },
    {
      path: '/error',
      component: ErrorPage
    },
    {
      path: '/verify-email',
      component: VerifyEmailPage,
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
          component: UserProfilePage,
          meta: {
            title: "My Profile",
            requiresAuth: true
          }
        },
        {
          path: '/public-courses',
          component: PublicCoursesPage,
          meta: {
            title: 'Public Courses'
          }
        },
        {
          path: "/public-course-details/:slug/:id",
          component: PublicCourseDetailsPage,
        },
        {
          path: '/payment/:slug/:id',
          component: QrPage,
          meta: {
            title: 'Payment',
            requiresAuth: true
          }
        },
        {
          path: '/enrollments/:id',
          component: MyEnrollmentsPage,
          meta: {
            title: 'My Enrollments',
            requiresAuth: true
          }
        },
        {
          path: '/wishlist/:id',
          component: WishlistPage,
          meta: {
            title: 'Wishlist',
            requiresAuth: true
          }
        },
        {
          path: '/transaction/:id',
          component: TransactionHistoryPage,
          meta: {
            title: "Transaction History",
            requiresAuth: true
          }
        }
      ]
    },
    {
      path: '/admin',
      component: AdminLayout,
      meta: {
        requiresAuth: true,
        roles: ['Admin']
      },
      redirect: '/admin/dashboard',
      children: [{
          path: 'dashboard',
          component: DashboardPage,
          meta: {
            title: "Dashboard"
          }
        },
        {
          path: 'accounts',
          component: AccountListPage,
          meta: {
            title: "Account List"
          }
        },
        {
          path: 'accounts/add',
          component: AddAccountPage,
          meta: {
            title: "Add Account"
          }
        },
        {
          path: 'accounts/edit/:id',
          component: EditAccountPage,
          meta: {
            title: "Edit Account"
          }
        },
        {
          path: 'courses',
          component: CourseListPage,
          meta: {
            title: "Course List"
          }
        },
        {
          path: 'courses/add',
          component: AddCoursePage,
          meta: {
            title: "Add Course"
          }
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
    {
      path: "/set-password",
      component: SetPasswordPage,
      meta: {
        title: "Reset Password",
      }
    },
  ],
})

router.afterEach((to) => {
  document.title = to.meta.title || 'Étudify'
})

router.beforeEach(async (to) => {
  const auth = useAuthStore();

  if (!auth.user) {
    await auth.fetchUser();
  }

  if (to.path === '/') {
    if (auth.user) {
      switch (auth.user.role) {
        case "Admin":
          return "/admin/dashboard";
        case "Instructor":
          return "/instructor/students";
        default:
          return "/home";
      }
    } else {
      return "/home"
    }
  }

  if (to.meta.requiresAuth && !auth.user) {
    return {
      path: '/login',
      query: {
        redirect: to.fullPath
      }
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
        return '/instructor/students';
      default:
        return '/';
    }
  }

  return true;
})

export default router
