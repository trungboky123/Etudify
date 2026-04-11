<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import authAxios from '@/function/authAxios'

const { t } = useI18n()
const { sidebarCollapsed } = defineProps({
  sidebarCollapsed: Boolean,
})
const router = useRouter()
const roles = ref([])
const fileInputRef = ref(null)
const roleRef = ref(null)
const roleOpen = ref(false)
const defaultAvatar = 'https://i.pinimg.com/736x/21/91/6e/21916e491ef0d796398f5724c313bbe7.jpg'
const user = ref({
  fullName: '',
  email: '',
  username: '',
  roleId: '',
  avatarUrl: '',
  status: true,
})
const roleName = computed(() => {
  return roles.value.find((r) => r.id === user.value.roleId)?.name || ''
})
const previewAvatar = ref(null)
const avatarFile = ref(null)
const message = ref('')
const isError = ref(false)
const isSaving = ref(false)

const handleClickOutside = (e) => {
  if (roleRef.value && !roleRef.value.contains(e.target)) {
    roleOpen.value = false
  }
}

const handleAvatarClick = () => {
  fileInputRef.value.click()
}

const handleAvatarChange = (e) => {
  const file = e.target.files[0]
  if (file) {
    avatarFile.value = file
    previewAvatar.value = URL.createObjectURL(file)
  }
}

const handleRemoveAvatar = () => {
  previewAvatar.value = defaultAvatar
  avatarFile.value = null
  user.value.avatarUrl = defaultAvatar
  fileInputRef.value = null
}

const getAllRoles = async () => {
  const res = await authAxios.get('/users/roles')
  roles.value = res.data
}

const handleRoleChange = (roleId) => {
  user.value.roleId = roleId
  roleOpen.value = false
}

const roleBadge = computed(() => {
  const role = roles.value.find((r) => r.id === user.value.roleId)
  return role ? `badge${role.name}` : ''
})

onMounted(() => {
  getAllRoles()
  document.addEventListener('mousedown', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('mousedown', handleClickOutside)
})
</script>

<template>
  <div class="layout">
    <div :class="{ main: true, mainCollapsed: sidebarCollapsed }">
      <div class="wrapper">
        <div class="breadcrumb">
          <span class="breadcrumbLink" @click="router.push('/admin/accounts')">
            <i class="bi bi-people-fill"></i>
            {{ t('admin.sidebar.accounts') }}
          </span>
          <i class="bi bi-chevron-right"></i>
          <span class="breadcrumbCurrent">{{ t('admin.addAccount.title') }}</span>
        </div>

        <div class="pageHeader">
          <div>
            <h1 class="pageTitle">{{ t('admin.addAccount.title') }}</h1>
            <p class="pageSubtitle">
              {{ t('admin.addAccount.subtitle') }}
            </p>
          </div>
          <button class="backBtn" @click="router.push('/admin/accounts')">
            <i class="bi bi-arrow-left"></i>
            {{ t('admin.addAccount.back') }}
          </button>
        </div>

        <form @submit.prevent="handleSubmit">
          <div class="formGrid">
            <div class="avatarCard">
              <h3 class="cardTitle">
                <i class="bi bi-person-circle"></i>
                Avatar
              </h3>
              <div class="avatarWrapper">
                <div class="avatarPreview" @click="handleAvatarClick">
                  <img v-if="previewAvatar" :src="previewAvatar" alt="Avatar" class="avatarImg" />
                  <img
                    v-else
                    :src="defaultAvatar"
                    alt="Avatar"
                    class="avatarImg"
                  />
                  <div class="avatarOverlay">
                    <i class="bi bi-camera"></i>
                    <span>{{ t('admin.addAccount.change') }}</span>
                  </div>
                </div>
                <input
                  type="file"
                  ref="fileInputRef"
                  accept="image/*"
                  class="fileInput"
                  @change="handleAvatarChange"
                />
                <div class="avatarActions">
                  <button
                    v-if="previewAvatar && previewAvatar !== defaultAvatar"
                    type="button"
                    class="removeBtn"
                    @click="handleRemoveAvatar"
                  >
                    <i class="bi bi-trash"></i>
                    {{ t('admin.addAccount.remove') }}
                  </button>
                </div>
                <p class="avatarNote">
                  {{ t('admin.addAccount.recommended') }}
                  200x200px, JPG/PNG, max 2MB
                </p>
              </div>

              <div class="cardDivider"></div>

              <h3 class="cardTitle">
                <i class="bi bi-shield-fill"></i>
                {{ t('admin.addAccount.permissions') }}
              </h3>

              <div class="formGroup">
                <label class="label">{{ t('admin.addAccount.role') }}</label>
                <div class="dropdown" ref="roleRef">
                  <button type="button" class="filterBtn" @click="roleOpen = !roleOpen">
                    <span>{{ roleName || t('admin.addAccount.selectRole') }}</span>
                    <i :class="{ 'bi bi-chevron-down': true, rotate: roleOpen }"></i>
                  </button>

                  <div v-if="roleOpen" class="filterDropdownMenu">
                    <button
                      v-for="role in roles"
                      :key="role.id"
                      type="button"
                      :class="{ filterDropdownItem: true, active: role.id === user.roleId }"
                      @click="handleRoleChange(role.id)"
                    >
                      {{ role.name }}
                      <i v-if="user.roleId === role.id" class="bi bi-check-lg"></i>
                    </button>
                  </div>
                </div>
              </div>

              <div class="formGroup">
                <label class="label">{{ t('admin.addAccount.status') }}</label>
                <div class="radioGroup">
                  <label :class="{ radioItem: true, radioActive: user.status }">
                    <input
                      type="radio"
                      name="status"
                      v-model="user.status"
                      class="radioInput"
                      :value="true"
                    />
                    <div class="radioBox">
                      <div class="radioCircle"></div>
                      <div>
                        <span class="radioLabel">{{ t('admin.addAccount.status.active') }}</span>
                        <span class="radioDesc">{{
                          t('admin.addAccount.status.active.description')
                        }}</span>
                      </div>
                    </div>
                  </label>

                  <label :class="{ radioItem: true, radioActive: !user.status }">
                    <input
                      type="radio"
                      name="status"
                      v-model="user.status"
                      class="radioInput"
                      :value="false"
                    />
                    <div class="radioBox">
                      <div class="radioCircle"></div>
                      <div>
                        <span class="radioLabel">{{ t('admin.addAccount.status.inactive') }}</span>
                        <span class="radioDesc">{{
                          t('admin.addAccount.status.inactive.description')
                        }}</span>
                      </div>
                    </div>
                  </label>
                </div>
              </div>
            </div>

            <div class="infoCard">
              <h3 class="cardTitle">
                <i class="bi bi-person-fill"></i>
                {{ t('admin.addAccount.information') }}
              </h3>

              <div class="formGroup">
                <label class="label">{{ t('admin.addAccount.fullName') }}</label>
                <div class="inputWrapper">
                  <i class="bi bi-person"></i>
                  <input
                    type="text"
                    name="fullName"
                    v-model="user.fullName"
                    class="input"
                    :placeholder="t('admin.addAccount.fullName.placeholder')"
                    :required="true"
                  />
                </div>
              </div>

              <div class="formGroup">
                <label class="label">{{ t('admin.addAccount.email') }}</label>
                <div class="inputWrapper">
                  <i class="bi bi-envelope"></i>
                  <input
                    type="email"
                    name="email"
                    v-model="user.email"
                    class="input"
                    :placeholder="t('admin.addAccount.email.placeholder')"
                    :required="true"
                  />
                </div>
              </div>

              <div class="formGroup">
                <label class="label">{{ t('admin.addAccount.username') }}</label>
                <div class="inputWrapper">
                  <i class="bi bi-at"></i>
                  <input
                    type="text"
                    name="username"
                    v-model="user.username"
                    class="input"
                    :placeholder="t('admin.addAccount.username.placeholder')"
                    :required="true"
                  />
                </div>
              </div>

              <p class="note">
                {{ t('admin.addAccount.note') }}
              </p>

              <div class="cardDivider"></div>

              <h3 class="cardTitle">
                <i class="bi bi-eye"></i>
                {{ t('admin.addAccount.preview') }}
              </h3>
              <div class="previewBox">
                <div class="previewAvatar">
                  <img
                    v-if="previewAvatar"
                    :src="previewAvatar"
                    alt="Avatar"
                    class="previewAvatarImg"
                  />
                  <img
                    v-else
                    src="https://i.pinimg.com/736x/21/91/6e/21916e491ef0d796398f5724c313bbe7.jpg"
                    alt="Avatar"
                    class="previewAvatarImg"
                  />
                </div>
                <div class="previewInfo">
                  <span class="previewName">
                    {{ user.fullName || t('admin.addAccount.fullName') }}
                  </span>
                  <span class="previewUsername">
                    @{{ user.username || t('admin.addAccount.username') }}
                  </span>
                </div>
                <span :class="`previewBadge ${roleBadge}`">
                  {{ roleName || t('admin.addAccount.role') }}
                </span>
              </div>

              <p v-if="message" :class="['message', isError ? 'messageError' : 'messageSuccess']">
                <i :class="isError ? 'bi bi-exclamation-circle' : 'bi bi-check-circle'"></i>
                {{ message }}
              </p>

              <div class="actions">
                <button type="button" class="cancelBtn" @click="router.push('/admin/accounts')">
                  <i class="bi bi-x-lg"></i>
                  {{ t('admin.addAccount.cancel') }}
                </button>
                <button type="submit" class="saveBtn" :disabled="isSaving">
                  <span v-if="isSaving">
                    <i class="bi bi-arrow-repeat"></i>
                    {{ t('admin.addAccount.saving') }}
                  </span>
                  <span v-else>
                    <i class="bi bi-floppy"></i>
                    {{ t('admin.addAccount.save') }}
                  </span>
                </button>
              </div>
            </div>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
@import url('https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap');

// Layout
.layout {
  display: flex;
  min-height: 100vh;
  font-family:
    'Plus Jakarta Sans',
    -apple-system,
    BlinkMacSystemFont,
    sans-serif;
  background: #f1f3f8;
}

.main {
  flex: 1;
  margin-left: 280px;
  transition: margin-left 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  margin-top: 50px;

  &Collapsed {
    margin-left: 85px;
  }
}

.wrapper {
  padding: 32px 36px;
  animation: fadeIn 0.4s ease;
}

// Breadcrumb
.breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.88rem;
  color: #94a3b8;
  margin-bottom: 20px;

  i {
    font-size: 0.7rem;
    color: #cbd5e1;
  }
}

.breadcrumbLink {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #667eea;
  cursor: pointer;
  font-weight: 600;
  transition: color 0.2s;

  i {
    font-size: 0.85rem;
    color: #667eea;
  }

  &:hover {
    color: #5568d3;
  }
}

.breadcrumbCurrent {
  color: #64748b;
  font-weight: 600;
}

// Page Header
.pageHeader {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 28px;
}

.pageTitle {
  font-size: 1.75rem;
  font-weight: 800;
  color: #1e293b;
  margin: 0 0 4px;
  line-height: 1.2;
}

.pageSubtitle {
  color: #64748b;
  font-size: 0.92rem;
  margin: 0;
}

.backBtn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  background: #fff;
  color: #475569;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  font-weight: 600;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.25s;

  i {
    font-size: 1rem;
  }

  &:hover {
    border-color: #667eea;
    color: #667eea;
    background: #f5f3ff;
  }
}

// Form Grid
.formGrid {
  display: grid;
  grid-template-columns: 380px 1fr;
  gap: 28px;
  align-items: start;
}

// Cards
.avatarCard,
.infoCard {
  background: #fff;
  border-radius: 18px;
  padding: 28px;
  box-shadow:
    0 1px 3px rgba(0, 0, 0, 0.06),
    0 4px 16px rgba(0, 0, 0, 0.04);
  border: 1px solid #eef2f7;
}

.cardTitle {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 1rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 20px;

  i {
    font-size: 1.1rem;
    color: #667eea;
  }
}

.cardDivider {
  height: 1px;
  background: #eef2f7;
  margin: 24px 0;
}

// Avatar
.avatarWrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.avatarPreview {
  position: relative;
  width: 150px;
  height: 150px;
  border-radius: 50%;
  cursor: pointer;
  margin-bottom: 18px;
  overflow: hidden;
  border: 4px solid #eef2f7;
  transition: border-color 0.3s;

  &:hover {
    border-color: #667eea;
  }

  &:hover .avatarOverlay {
    opacity: 1;
  }
}

.avatarImg {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.avatarPlaceholder {
  width: 100%;
  height: 100%;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  display: flex;
  align-items: center;
  justify-content: center;
}

.avatarInitials {
  font-size: 3rem;
  font-weight: 800;
  color: #fff;
  letter-spacing: -2px;
}

.avatarOverlay {
  position: absolute;
  inset: 0;
  background: rgba(30, 41, 59, 0.55);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  opacity: 0;
  transition: opacity 0.25s;

  i {
    font-size: 1.6rem;
    color: #fff;
  }

  span {
    font-size: 0.82rem;
    color: #fff;
    font-weight: 600;
  }
}

.fileInput {
  display: none;
}

.avatarActions {
  display: flex;
  gap: 10px;
  margin-bottom: 12px;
}

.uploadBtn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 18px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  border: none;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.25s;

  i {
    font-size: 0.95rem;
  }

  &:hover {
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.35);
  }
}

.removeBtn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  background: #fff5f5;
  color: #ef4444;
  border: 1.5px solid #fecaca;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.25s;

  i {
    font-size: 0.9rem;
  }

  &:hover {
    background: #fee2e2;
    border-color: #f87171;
  }
}

.avatarNote {
  font-size: 0.78rem;
  color: #94a3b8;
  text-align: center;
  margin: 0;
}

// Form Groups
.formGroup {
  margin-bottom: 20px;
}

.label {
  display: block;
  font-size: 0.88rem;
  font-weight: 600;
  color: #374151;
  margin-bottom: 8px;
}

// Input
.inputWrapper {
  position: relative;

  i {
    position: absolute;
    left: 14px;
    top: 50%;
    transform: translateY(-50%);
    color: #94a3b8;
    font-size: 1rem;
    transition: color 0.25s;
  }

  &:focus-within i {
    color: #667eea;
  }
}

.input {
  width: 100%;
  height: 46px;
  padding: 0 16px 0 42px;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  font-size: 0.93rem;
  color: #1e293b;
  background: #fafbfc;
  outline: none;
  transition: all 0.25s;
  font-family: inherit;
  box-sizing: border-box;

  &:focus {
    border-color: #667eea;
    background: #fff;
    box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.12);
  }

  &::placeholder {
    color: #cbd5e1;
  }
}

// Custom Dropdown
.dropdown {
  position: relative;
}

.filterBtn {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  width: 100%;
  height: 46px;
  padding: 0 16px;
  background: #fafbfc;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  font-size: 0.93rem;
  font-weight: 500;
  color: #1e293b;
  cursor: pointer;
  transition: all 0.25s;
  text-align: left;

  i {
    font-size: 0.75rem;
    color: #94a3b8;
    transition: transform 0.3s;

    &.rotate {
      transform: rotate(180deg);
    }
  }

  &:hover {
    border-color: #cbd5e1;
    background: #fff;
  }

  &:focus {
    border-color: #667eea;
    background: #fff;
    box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.12);
  }
}

.filterDropdownMenu {
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  width: 100%;
  background: #ffffff;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
  padding: 6px;
  z-index: 100;
  animation: slideDown 0.2s ease;
  max-height: 240px;
  overflow-y: auto;

  &::-webkit-scrollbar {
    width: 6px;
  }

  &::-webkit-scrollbar-track {
    background: #f3f4f6;
    border-radius: 3px;
  }

  &::-webkit-scrollbar-thumb {
    background: #cbd5e1;
    border-radius: 3px;

    &:hover {
      background: #94a3b8;
    }
  }
}

.filterDropdownItem {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 12px;
  background: transparent;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 500;
  color: #1e293b;
  cursor: pointer;
  transition: all 0.2s;
  text-align: left;

  i {
    font-size: 1rem;
    color: #667eea;
    flex-shrink: 0;
  }

  &:hover {
    background: #f8fafc;
    color: #667eea;
  }

  &.active {
    background: #f0f2ff;
    color: #667eea;
    font-weight: 600;
  }
}

// Radio Group
.radioGroup {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.radioItem {
  display: flex;
  cursor: pointer;
  padding: 12px 14px;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  background: #fafbfc;
  transition: all 0.25s;

  &:hover {
    border-color: #c7d2fe;
    background: #f8f9ff;
  }

  &.radioActive {
    border-color: #667eea;
    background: #f0f2ff;
  }
}

.radioInput {
  display: none;
}

.radioBox {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
}

.radioCircle {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  border: 2px solid #cbd5e1;
  flex-shrink: 0;
  position: relative;
  transition: all 0.25s;

  .radioActive & {
    border-color: #667eea;

    &::after {
      content: '';
      position: absolute;
      top: 3px;
      left: 3px;
      right: 3px;
      bottom: 3px;
      border-radius: 50%;
      background: linear-gradient(135deg, #667eea, #764ba2);
    }
  }
}

.radioLabel {
  display: block;
  font-size: 0.9rem;
  font-weight: 600;
  color: #1e293b;
}

.radioDesc {
  display: block;
  font-size: 0.78rem;
  color: #94a3b8;
  margin-top: 1px;
}

// Preview Box
.previewBox {
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 16px;
  background: #f8fafc;
  border: 1px solid #eef2f7;
  border-radius: 12px;
  margin-bottom: 20px;
}

.previewAvatar {
  width: 46px;
  height: 46px;
  border-radius: 50%;
  overflow: hidden;
  flex-shrink: 0;
  background: linear-gradient(135deg, #667eea, #764ba2);
  display: flex;
  align-items: center;
  justify-content: center;
}

.previewAvatarImg {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.previewInitials {
  font-size: 1.1rem;
  font-weight: 700;
  color: #fff;
}

.previewInfo {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.previewName {
  font-size: 0.92rem;
  font-weight: 700;
  color: #1e293b;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.previewUsername {
  font-size: 0.8rem;
  color: #94a3b8;
}

.previewBadge {
  padding: 4px 12px;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  white-space: nowrap;
}

.badgeStudent {
  background: #eef2ff;
  color: #667eea;
}

.badgeInstructor {
  background: #f0fdf4;
  color: #16a34a;
}

.badgeAdmin {
  background: #fef3c7;
  color: #d97706;
}

// Message
.message {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 16px;
  border-radius: 12px;
  font-size: 0.88rem;
  margin-bottom: 20px;
  animation: slideDown 0.3s ease;

  i {
    font-size: 1.1rem;
    flex-shrink: 0;
  }
}

.note {
  font-size: 0.88rem;
  color: red;
  margin-top: 12px;
  margin-bottom: 16px;
  margin-left: 15px;
}

.messageError {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
}

.messageSuccess {
  background: #f0fdf4;
  color: #16a34a;
  border: 1px solid #bbf7d0;
}

// Actions
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 8px;
}

.cancelBtn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 11px 22px;
  background: #fff;
  color: #64748b;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.25s;

  i {
    font-size: 0.9rem;
  }

  &:hover {
    border-color: #cbd5e1;
    color: #475569;
    background: #f8fafc;
  }
}

.saveBtn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 11px 28px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  border: none;
  border-radius: 10px;
  font-size: 0.9rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.25s;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3);

  i {
    font-size: 1rem;
  }

  &:hover:not(:disabled) {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(102, 126, 234, 0.4);
  }

  &:active:not(:disabled) {
    transform: translateY(0);
  }

  &:disabled {
    opacity: 0.7;
    cursor: not-allowed;

    i {
      animation: spin 0.8s linear infinite;
    }
  }
}

// Animations
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(16px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes slideDown {
  from {
    opacity: 0;
    transform: translateY(-8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

// Responsive
@media (max-width: 1100px) {
  .formGrid {
    grid-template-columns: 1fr;
  }

  .avatarCard {
    max-width: 500px;
  }
}

@media (max-width: 768px) {
  .main {
    margin-left: 85px;

    &Collapsed {
      margin-left: 85px;
    }
  }

  .wrapper {
    padding: 20px;
  }

  .pageHeader {
    flex-direction: column;
    gap: 16px;
  }

  .actions {
    flex-direction: column;
  }

  .saveBtn,
  .cancelBtn {
    width: 100%;
    justify-content: center;
  }
}
</style>
