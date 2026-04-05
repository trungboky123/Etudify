<script setup>
import { useAuthStore } from '@/stores/auth'
import axios from 'axios'
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'

const { t, locale } = useI18n()
const { sidebarCollapsed } = defineProps({
  sidebarCollapsed: Boolean,
})
const router = useRouter()
const auth = useAuthStore()
const languages = [
  { code: 'vi', name: t('nav.lang.vietnam'), flag: '🇻🇳' },
  { code: 'en', name: t('nav.lang.english'), flag: '🇺🇸' },
  { code: 'fr', name: t('nav.lang.french'), flag: '🇫🇷' },
]
const langOpen = ref(false)
const userOpen = ref(false)
const langRef = ref(null)
const userRef = ref(null)

const handleChangeLanguage = (langcode) => {
  locale.value = langcode
  localStorage.setItem('lang', locale.value)
  langOpen.value = false
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
  await auth.fetchUser()
})

onMounted(() => {
  document.addEventListener('mousedown', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('mousedown', handleClickOutside)
})

const currentLang = computed(() => {
  return languages.find((lang) => lang.code === locale.value) ?? languages[0]
})

async function handleLogout() {
  userOpen.value = false;
  await axios.post('http://localhost:5062/auth/logout', {}, {
    withCredentials: true
  });
  await auth.fetchUser();
  router.push('/home');
}
</script>

<template>
  <header :class="{ header: true, collapsed: sidebarCollapsed }">
    <div class="container">
      <div class="left">
        <button class="homeBtn" @click="router.push('/home')">
          <i class="bi bi-house-door-fill"></i>
          <span>{{ t('admin.header.homepage') }}</span>
        </button>
      </div>

      <div class="right">
        <div class="userSection" ref="userRef">
          <button class="userBtn" @click="userOpen = !userOpen">
            <img :src="auth.user?.avatarUrl" alt="Avatar" class="avatar" />
            <span class="userName">{{ auth.user?.fullName || 'Admin' }}</span>
            <i :class="{ 'bi bi-chevron-down': true, chevron: true, chevronOpen: userOpen }"></i>
          </button>

          <div v-if="userOpen" class="dropdown">
            <RouterLink
              @click="userOpen = false"
              :to="`/profile/${auth.user?.id}`"
              class="dropdownItem"
            >
              <i class="bi bi-person"></i>
              <span>{{ t('nav.profile') }}</span>
            </RouterLink>
            <div class="dropdownDivider"></div>
            <RouterLink @click="handleLogout" class="dropdownItem">
              <i class="bi bi-box-arrow-right"></i>
              <span>{{ t('nav.logout') }}</span>
            </RouterLink>
          </div>
        </div>

        <div class="langSection" ref="langRef">
          <button :class="{ langBtn: true, langBtnOpen: langOpen }" @click="langOpen = !langOpen">
            <span class="langFlag">{{ currentLang.flag }}</span>
            <span class="langName">{{ currentLang.name }}</span>
            <i :class="{ 'bi bi-chevron-down': true, chevron: true, chevronOpen: langOpen }"></i>
          </button>

          <div v-if="langOpen" class="langDropdown">
            <button v-for="lang in languages" :key="lang.code" :class="{'langItem': true, 'langItemActive': locale === lang.code}" @click="handleChangeLanguage(lang.code)">
              <span class="langFlag">{{ lang.flag }}</span>
              <span class="langItemName">{{ lang.name }}</span>
              <i v-if="locale === lang.code" class="bi bi-check-lg"></i>
            </button>
          </div>
        </div>
      </div>
    </div>
  </header>
</template>

<style scoped lang="scss">
@import url('https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap');

.header {
  position: fixed;
  top: 0;
  left: 280px;
  right: 0;
  height: 70px;
  background: #ffffff;
  border-bottom: 1px solid #e5e7eb;
  z-index: 999;
  font-family:
    'Plus Jakarta Sans',
    -apple-system,
    BlinkMacSystemFont,
    sans-serif;
  transition: left 0.3s cubic-bezier(0.4, 0, 0.2, 1);

  &.collapsed {
    left: 85px;
  }
}

.container {
  max-width: 100%;
  height: 100%;
  padding: 0 32px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

// ─── Left ─────────────────────────────────────────────────────────────────────
.left {
  display: flex;
  align-items: center;
}

.homeBtn {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 20px;
  background: transparent;
  border: none;
  border-radius: 10px;
  color: #667eea;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;

  i {
    font-size: 1.2rem;
    transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  }

  span {
    font-weight: 600;
    letter-spacing: -0.01em;
  }

  &::before {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(135deg, #667eea 0%, #2563eb 100%);
    opacity: 0;
    transition: opacity 0.3s;
    border-radius: 10px;
    z-index: -1;
  }

  &:hover {
    color: #ffffff;
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);

    &::before {
      opacity: 1;
    }
    i {
      transform: scale(1.1);
    }
  }

  &:active {
    transform: translateY(0);
  }
}

// ─── Right ────────────────────────────────────────────────────────────────────
.right {
  display: flex;
  align-items: center;
  gap: 10px;
}

// ─── Language Dropdown ────────────────────────────────────────────────────────
.langSection {
  position: relative;
}

.langBtn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 14px;
  background: transparent;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
  font-family: inherit;

  &:hover {
    background: #f9fafb;
    border-color: #d1d5db;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  }

  &Open {
    background: #f9fafb;
    border-color: #667eea;
    box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
  }
}

.langFlag {
  font-size: 1.15rem;
  line-height: 1;
}

.langName {
  font-size: 0.88rem;
  font-weight: 600;
  color: #374151;
  white-space: nowrap;
}

.langDropdown {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  min-width: 160px;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  box-shadow:
    0 4px 6px -1px rgba(0, 0, 0, 0.1),
    0 2px 4px -1px rgba(0, 0, 0, 0.06),
    0 0 0 1px rgba(0, 0, 0, 0.05);
  padding: 6px;
  animation: slideDown 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  transform-origin: top right;
  z-index: 100;
}

.langItem {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 9px 10px;
  background: transparent;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  font-family: inherit;
  text-align: left;

  > i.bi-check-lg {
    font-size: 0.9rem;
    color: #667eea;
    margin-left: auto;
    flex-shrink: 0;
  }

  &:hover {
    background: #f3f4f6;
  }

  &Active {
    background: #f0f2ff;

    .langItemName {
      color: #667eea;
      font-weight: 700;
    }
  }
}

.langItemName {
  font-size: 0.88rem;
  font-weight: 500;
  color: #374151;
  flex: 1;
}

// ─── User Dropdown ────────────────────────────────────────────────────────────
.userSection {
  position: relative;
}

.userBtn {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 16px;
  background: transparent;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);

  &:hover {
    background: #f9fafb;
    border-color: #d1d5db;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
  }
}

.avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  object-fit: cover;
  border: 2px solid #e5e7eb;
  transition: all 0.3s;

  .userBtn:hover & {
    border-color: #3b82f6;
  }
}

.userName {
  font-size: 0.95rem;
  font-weight: 600;
  color: #1f2937;
  letter-spacing: -0.01em;
  white-space: nowrap;
  max-width: 150px;
  overflow: hidden;
  text-overflow: ellipsis;
}

.chevron {
  font-size: 0.875rem;
  color: #6b7280;
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);

  &Open {
    transform: rotate(180deg);
  }
}

.dropdown {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  min-width: 200px;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  box-shadow:
    0 4px 6px -1px rgba(0, 0, 0, 0.1),
    0 2px 4px -1px rgba(0, 0, 0, 0.06),
    0 0 0 1px rgba(0, 0, 0, 0.05);
  padding: 8px;
  animation: slideDown 0.2s cubic-bezier(0.4, 0, 0.2, 1);
  transform-origin: top right;
  z-index: 100;
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-8px) scale(0.95);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

.dropdownItem {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
  background: transparent;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 0.9rem;
  font-weight: 500;
  color: #374151;
  text-align: left;
  font-family: inherit;
  text-decoration: none;

  i {
    font-size: 1.1rem;
    color: #6b7280;
    transition: all 0.2s;
  }

  span {
    flex: 1;
  }

  &:hover {
    background: #f3f4f6;
    color: #1f2937;

    i {
      color: #3b82f6;
      transform: translateX(2px);
    }
  }

  &:active {
    background: #e5e7eb;
  }

  &:last-child {
    color: #dc2626;
    i {
      color: #dc2626;
    }

    &:hover {
      background: #fef2f2;
      color: #b91c1c;
      i {
        color: #b91c1c;
      }
    }
  }
}

.dropdownDivider {
  height: 1px;
  background: #e5e7eb;
  margin: 8px 0;
}

// ─── Responsive ──────────────────────────────────────────────────────────────
@media (max-width: 768px) {
  .header {
    left: 85px;
    &.collapsed {
      left: 85px;
    }
  }

  .container {
    padding: 0 16px;
  }

  .homeBtn {
    padding: 8px 12px;
    span {
      display: none;
    }
    i {
      font-size: 1.3rem;
    }
  }

  .langName {
    display: none;
  }

  .langBtn {
    padding: 8px 10px;
    gap: 4px;
  }

  .userName {
    display: none;
  }

  .userBtn {
    padding: 6px;
    border: none;
    background: transparent;
    &:hover {
      background: #f9fafb;
    }
  }

  .avatar {
    width: 36px;
    height: 36px;
  }

  .dropdown,
  .langDropdown {
    right: -8px;
  }
}

@media (max-width: 480px) {
  .header {
    height: 60px;
  }
  .container {
    padding: 0 12px;
  }
  .homeBtn {
    padding: 6px 10px;
  }
}
</style>
