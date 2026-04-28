<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import logo from '../images/logo.png'
import defaultAvatar from '../images/default-avatar.png'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import api from '@/api/api'
import { useAuthStore } from '@/stores/auth'

const { t, locale } = useI18n()
const router = useRouter();
const route = useRoute();
const auth = useAuthStore();
const languages = computed(() => [
  { code: 'vi', name: t('nav.lang.vietnam'), flag: '🇻🇳' },
  { code: 'en', name: t('nav.lang.english'), flag: '🇺🇸' },
  { code: 'fr', name: t('nav.lang.french'), flag: '🇫🇷' },
])
const langOpen = ref(false)
const userOpen = ref(false)
const langRef = ref(null)
const userRef = ref(null)

const handleChangeLanguage = (langcode) => {
  locale.value = langcode
  localStorage.setItem('lang', locale.value)
  langOpen.value = false
}

async function handleLogout() {
  userOpen.value = false;
  await api.post('/auth/logout', {}, {
    withCredentials: true
  });
  await auth.fetchUser();
  router.push('/home');
}

const handleClickOutside = (e) => {
  if (langRef.value && !langRef.value.contains(e.target)) {
    langOpen.value = false
  }

  if (userRef.value && !userRef.value.contains(e.target)) {
    userOpen.value = false
  }
}

onMounted(async () => {
  await auth.fetchUser();
})

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('mousedown', handleClickOutside)
})

const isActive = (path) => {
  return route.path === path
}

const currentLang = computed(() => {
  return languages.value.find((lang) => lang.code === locale.value) ?? languages.value[0]
})
</script>

<template>
  <nav class="navbar navbar-expand-lg fixed-top">
    <div class="container">
      <RouterLink to="/home" class="navbar-brand">
        <img :src="logo" alt="logo" style="height: 45px" />
      </RouterLink>

      <button
        class="navbar-toggler"
        type="button"
        data-bs-toggle="collapse"
        data-bs-target="#navbarNav"
      >
        <span class="navbar-toggler-icon"></span>
      </button>

      <div class="collapse navbar-collapse" id="navbarNav">
        <ul class="navbar-nav me-auto ms-4">
          <li class="nav-item">
            <RouterLink
              :class="{ 'nav-link': true, active: isActive('/public-courses') }"
              to="/public-courses"
            >
              {{ t('nav.course') }}
            </RouterLink>
          </li>
          <li class="nav-item">
            <RouterLink :class="{ 'nav-link': true, active: isActive('/blog') }" to="/blog">
              {{ t('nav.blog') }}
            </RouterLink>
          </li>
        </ul>

        <div v-if="!auth.user?.role" class="d-flex align-items-center">
          <div class="auth-buttons me-2">
            <RouterLink to="/login" class="btn btn-outline-primary">
              {{ t('nav.login') }}
            </RouterLink>
            <RouterLink to="/register" class="btn btn-primary">
              {{ t('nav.register') }}
            </RouterLink>
          </div>

          <div class="hd-dropdown" ref="langRef">
            <button
              :class="{ 'hd-lang-btn': true, open: langOpen }"
              @click="() => (langOpen = !langOpen)"
            >
              <span class="flag-icon">
                {{ currentLang.flag }}
              </span>
              <span class="language-name">
                {{ currentLang.name }}
              </span>
              <i :class="{ 'bi bi-chevron-down hd-chevron': true, rotate: langOpen }"></i>
            </button>

            <div v-if="langOpen" class="hd-menu lang-menu">
              <button
                v-for="lang in languages"
                :key="lang.code"
                :class="{ 'hd-item': true, active: locale === lang.code }"
                @click="handleChangeLanguage(lang.code)"
              >
                <span class="flag-icon">{{ lang.flag }}</span>
                <span class="language-name">{{ lang.name }}</span>
                <i v-if="locale === lang.code" class="bi bi-check-lg hd-check"></i>
              </button>
            </div>
          </div>
        </div>

        <div v-if="auth.user?.role === 'Admin' || auth.user?.role === 'Instructor'" class="d-flex align-items-center">
          <div class="hd-dropdown" ref="userRef">
            <button
              :class="{ 'hd-user-btn': true, open: userOpen }"
              @click="() => (userOpen = !userOpen)"
            >
              <img :src="auth.user?.avatarUrl || defaultAvatar" alt="Avatar" class="hd-avatar" />
              <span class="hd-username">{{ auth.user?.fullName }}</span>
              <i :class="{ 'bi bi-chevron-down hd-chevron': true, rotate: userOpen }"></i>
            </button>

            <div v-if="userOpen" class="hd-menu user-menu">
              <RouterLink class="hd-item" :to="`/profile/${auth.user?.id}`" @click="() => (userOpen = false)">
                <i class="bi bi-person"></i>
                {{ t('nav.profile') }}
              </RouterLink>
              <RouterLink
                class="hd-item"
                :to="auth.user?.role === 'Admin' ? '/admin/dashboard' : '/instructor/student-list'"
                @click="() => (userOpen = false)"
              >
                <i class="bi bi-grid"></i>
                {{ auth.user?.role === 'Admin' ? 'Dashboard' : 'Instructor Pages' }}
              </RouterLink>
              <div class="hd-divider"></div>
              <button class="hd-item danger" @click="handleLogout">
                <i class="bi bi-box-arrow-right"></i>
                {{ t('nav.logout') }}
              </button>
            </div>
          </div>

          <div class="hd-dropdown ms-2" ref="langRef">
            <button
              :class="{ 'hd-lang-btn': true, open: langOpen }"
              @click="() => (langOpen = !langOpen)"
            >
              <span class="flag-icon">{{ currentLang.flag }}</span>
              <span class="language-name">{{ currentLang.name }}</span>
              <i :class="{ 'bi bi-chevron-down hd-chevron': true, rotate: langOpen }"></i>
            </button>

            <div v-if="langOpen" class="hd-menu lang-menu">
              <button
                v-for="lang in languages"
                :key="lang.code"
                :class="{ 'hd-item': true, active: locale === lang.code }"
                @click="handleChangeLanguage(lang.code)"
              >
                <span class="flag-icon">{{ lang.flag }}</span>
                <span class="language-name">{{ lang.name }}</span>
                <i v-if="locale === lang.code" class="bi bi-check-lg hd-check"></i>
              </button>
            </div>
          </div>
        </div>

        <div v-if="auth.user?.role === 'Student'" class="d-flex align-items-center">
          <div class="hd-dropdown" ref="userRef">
            <button
              :class="{ 'hd-user-btn': true, open: userOpen }"
              @click="() => (userOpen = !userOpen)"
            >
              <img :src="auth.user?.avatarUrl || defaultAvatar" alt="Avatar" class="hd-avatar" />
              <span class="hd-username">{{ auth.user?.fullName }}</span>
              <i :class="{ 'bi bi-chevron-down hd-chevron': true, rotate: userOpen }"></i>
            </button>

            <div v-if="userOpen" class="hd-menu user-menu">
              <RouterLink class="hd-item" :to="`/profile/${auth.user?.id}/`" @click="() => (userOpen = false)">
                <i class="bi bi-person"></i>
                {{ t('nav.profile') }}
              </RouterLink>
              <RouterLink class="hd-item" :to="`/wishlist/${auth.user?.id}`" @click="() => (userOpen = false)">
                <i class="bi bi-heart"></i>
                {{ t('nav.wishlist') }}
              </RouterLink>
              <RouterLink class="hd-item" :to="`/enrollments/${auth.user?.id}`" @click="() => (userOpen = false)">
                <i class="bi bi-journal-bookmark"></i>
                {{ t('nav.myEnrollments') }}
              </RouterLink>
              <RouterLink
                class="hd-item"
                to="/transaction-history"
                @click="() => (userOpen = false)"
              >
                <i class="bi bi-clock-history"></i>
                {{ t('nav.history') }}
              </RouterLink>
              <div class="hd-divider"></div>
              <button class="hd-item danger" @click="handleLogout">
                <i class="bi bi-box-arrow-right"></i>
                {{ t('nav.logout') }}
              </button>
            </div>
          </div>

          <div class="hd-dropdown ms-2" ref="langRef">
            <button
              :class="{ 'hd-lang-btn': true, open: langOpen }"
              @click="() => (langOpen = !langOpen)"
            >
              <span class="flag-icon">{{ currentLang.flag }}</span>
              <span class="language-name">{{ currentLang.name }}</span>
              <i :class="{ 'bi bi-chevron-down hd-chevron': true, rotate: langOpen }"></i>
            </button>

            <div v-if="langOpen" class="hd-menu lang-menu">
              <button
                v-for="lang in languages"
                :key="lang.code"
                :class="{ 'hd-item': true, active: locale === lang.code }"
                @click="handleChangeLanguage(lang.code)"
              >
                <span class="flag-icon">{{ lang.flag }}</span>
                <span class="language-name">{{ lang.name }}</span>
                <i v-if="locale === lang.code" class="bi bi-check-lg hd-check"></i>
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </nav>
</template>

<style scoped lang="scss">
@import url('https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap');

/* ── Base ──────────────────────────────────────────────────────────────────── */
.navbar {
  background-color: #fff;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  font-family:
    'Plus Jakarta Sans',
    -apple-system,
    BlinkMacSystemFont,
    sans-serif;
  transition: all 0.3s ease;
}

.navbar-brand {
  font-weight: 700;
  font-size: 28px;
  transition: transform 0.3s ease;
}

.navbar-brand:hover {
  transform: scale(1.05);
}

/* ── Nav links ─────────────────────────────────────────────────────────────── */
.nav-link {
  font-weight: 500;
  margin-left: 15px;
  position: relative;
  transition: color 0.3s ease;
  color: #6c757d;
  padding: 8px 12px !important;
  border-radius: 8px;
}

.nav-link:hover {
  color: #667eea;
  background: rgba(102, 126, 234, 0.05);
}

.nav-link.active {
  color: #667eea;
  font-weight: 600;
}

.nav-link.active::after {
  content: '';
  position: absolute;
  bottom: -8px;
  left: 0;
  right: 0;
  height: 3px;
  background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
  border-radius: 2px;
  animation: slideIn 0.3s ease;
}

@keyframes slideIn {
  from {
    width: 0;
    left: 50%;
  }

  to {
    width: 100%;
    left: 0;
  }
}

/* ── Auth buttons ──────────────────────────────────────────────────────────── */
.auth-buttons {
  display: flex;
  gap: 10px;
}

.btn-outline-primary {
  border: 2px solid #667eea;
  color: #667eea;
  background: transparent;
  font-weight: 600;
  padding: 8px 20px;
  border-radius: 10px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
}

.btn-outline-primary::before {
  content: '';
  position: absolute;
  inset: 0;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  opacity: 0;
  transition: opacity 0.3s;
  z-index: -1;
}

.btn-outline-primary:hover {
  color: white;
  border-color: #667eea;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
}

.btn-outline-primary:hover::before {
  opacity: 1;
}

.btn-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  color: white;
  font-weight: 600;
  padding: 8px 20px;
  border-radius: 10px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.2);
}

.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 16px rgba(102, 126, 234, 0.4);
}

.btn-primary:active,
.btn-outline-primary:active {
  transform: translateY(0);
}

/* ═══════════════════════════════════════════════════════════════════════════
   CUSTOM DROPDOWN  (prefix: hd-)
   Replaces all data-bs-toggle="dropdown" — no Bootstrap JS, no jitter
   ═══════════════════════════════════════════════════════════════════════════ */

.hd-dropdown {
  position: relative;
}

/* ── User button ───────────────────────────────────────────────────────────── */
.hd-user-btn {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 6px 14px 6px 6px;
  background: transparent;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  font-family: inherit;
  font-size: 0.95rem;
  font-weight: 600;
  color: #1f2937;
}

.hd-user-btn:hover,
.hd-user-btn.open {
  background: #f9fafb;
  border-color: #d1d5db;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.hd-user-btn.open {
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

.hd-avatar {
  width: 38px;
  height: 38px;
  border-radius: 50%;
  object-fit: cover;
  border: 2px solid #e5e7eb;
  transition: border-color 0.25s;
  flex-shrink: 0;
}

.hd-user-btn:hover .hd-avatar,
.hd-user-btn.open .hd-avatar {
  border-color: #667eea;
}

.hd-username {
  max-width: 130px;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* ── Language button ───────────────────────────────────────────────────────── */
.hd-lang-btn {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 8px 12px;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  cursor: pointer;
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  font-family: inherit;
  font-size: 0.9rem;
  font-weight: 500;
  color: #374151;
}

.hd-lang-btn:hover,
.hd-lang-btn.open {
  background: #f9fafb;
  border-color: #d1d5db;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.hd-lang-btn.open {
  border-color: #667eea;
  box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
}

/* ── Chevron ───────────────────────────────────────────────────────────────── */
.hd-chevron {
  font-size: 0.72rem;
  color: #9ca3af;
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  flex-shrink: 0;
}

.hd-chevron.rotate {
  transform: rotate(180deg);
}

/* ── Dropdown menus ────────────────────────────────────────────────────────── */
.hd-menu {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  box-shadow:
    0 4px 6px -1px rgba(0, 0, 0, 0.1),
    0 2px 4px -1px rgba(0, 0, 0, 0.06),
    0 0 0 1px rgba(0, 0, 0, 0.04);
  padding: 6px;
  z-index: 1050;
  animation: hdDrop 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  transform-origin: top right;
}

.user-menu {
  min-width: 220px;
}

.lang-menu {
  min-width: 165px;
}

@keyframes hdDrop {
  from {
    opacity: 0;
    transform: translateY(-8px) scale(0.96);
  }

  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

/* ── Items (works for both <button> and <Link>) ────────────────────────────── */
.hd-item {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 9px 10px;
  background: transparent;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  font-size: 0.9rem;
  font-weight: 500;
  color: #374151;
  text-align: left;
  text-decoration: none;
  font-family: inherit;
  margin-bottom: 1px;
}

.hd-item i {
  font-size: 1rem;
  color: #9ca3af;
  transition: all 0.2s;
  width: 18px;
  flex-shrink: 0;
}

.hd-item:hover {
  background: #f3f4f6;
  color: #1f2937;
  transform: translateX(3px);
}

.hd-item:hover i {
  color: #667eea;
}

/* Active language item */
.hd-item.active {
  background: #f0f2ff;
  color: #667eea;
  font-weight: 600;
}

.hd-item.active i {
  color: #667eea;
}

/* Danger (logout) */
.hd-item.danger {
  color: #dc2626;
}

.hd-item.danger i {
  color: #dc2626;
}

.hd-item.danger:hover {
  background: #fef2f2;
  color: #b91c1c;
  transform: translateX(3px);
}

.hd-item.danger:hover i {
  color: #b91c1c;
}

/* Check icon */
.hd-check {
  font-size: 0.9rem !important;
  color: #667eea !important;
  margin-left: auto;
  width: auto !important;
}

/* Divider */
.hd-divider {
  height: 1px;
  background: #e5e7eb;
  margin: 6px 0;
}

/* ── Flag / language name ──────────────────────────────────────────────────── */
.flag-icon {
  font-size: 1.15rem;
  line-height: 1;
}

.language-name {
  font-size: 0.88rem;
  font-weight: 500;
  color: #374151;
}

/* ── Mobile ────────────────────────────────────────────────────────────────── */
@media (max-width: 991px) {
  .navbar-collapse {
    background: #fff;
    padding: 20px;
    border-radius: 12px;
    margin-top: 10px;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }

  .nav-link {
    margin-left: 0;
    margin-bottom: 8px;
  }

  .auth-buttons {
    margin-top: 16px;
    flex-direction: column;
  }

  .btn-outline-primary,
  .btn-primary {
    width: 100%;
  }

  /* On mobile, menus sit inline (no absolute positioning) */
  .hd-menu {
    position: static;
    box-shadow: none;
    border: 1px solid #f1f5f9;
    background: #fafbfc;
    margin-top: 6px;
    animation: none;
    transform: none;
  }

  .hd-user-btn,
  .hd-lang-btn {
    width: 100%;
    justify-content: flex-start;
  }

  .hd-item:hover {
    transform: none;
  }

  .ms-2 {
    margin-left: 0 !important;
    margin-top: 8px;
  }
}

@media (max-width: 576px) {
  .navbar-brand {
    font-size: 24px;
  }

  .hd-username {
    display: none;
  }

  .language-name {
    display: none;
  }
}
</style>
