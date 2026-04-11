<script setup>
import authAxios from '@/function/authAxios'
import { useAuthStore } from '@/stores/auth'
import { redirect } from 'react-router-dom'
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'

const { t } = useI18n()
const router = useRouter()
const route = useRoute()
const auth = useAuthStore()
const newData = ref({})
const passwordData = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
})
const emailData = ref({
  currentEmail: '',
  newEmail: '',
})
const avatarPreview = ref(null)
const avatarFile = ref(null)
const message = ref('')
const activeTab = ref('profile')
const isEditing = ref(false)
const fileInput = ref(null)
const isUpdated = ref(false)
const showCurrentPassword = ref(false)
const showNewPassword = ref(false)
const showConfirmPassword = ref(false)
const isLoadingCurrentEmail = ref(false)
const isSaving = ref(false)
const isLoadingNewEmail = ref(false)
const currentEmailVerified = ref(false)

const maskedEmail = computed(() => {
  const email = auth.user?.email
  if (!email) return ''

  const [text, domain] = email.split('@')
  if (text.length <= 2) return email

  const visible = text.slice(-2)
  const masked = '*'.repeat(Math.min(text.length - 2, 6))
  return `${masked}${visible}@${domain}`
})

onMounted(async () => {
  try {
    await auth.fetchUser()
  } catch (error) {
    router.push({
      path: '/login',
      query: {
        redirect: route.fullPath,
      }
    })
  }

  const id = route.params.id
  if (auth.user?.id && id !== auth.user.id) {
    router.push('/error')
  }

  document.title = `${auth.user?.fullName} | My Profile`
})

const handleAvatarChange = (e) => {
  const file = e.target.files[0]
  if (file) {
    avatarPreview.value = URL.createObjectURL(file)
    avatarFile.value = file
  }
}

const handleRemoveAvatar = () => {
  avatarPreview.value = 'https://i.pinimg.com/736x/21/91/6e/21916e491ef0d796398f5724c313bbe7.jpg'
  avatarFile.value = null

  auth.user.avatarUrl =
    'https://i.pinimg.com/736x/21/91/6e/21916e491ef0d796398f5724c313bbe7.jpg'
  newData.value.removeAvatar = true

  if (fileInput.value) {
    fileInput.value.value = null
  }
}

const verifyCurrentEmail = async () => {
  if (!emailData.value.currentEmail) {
    message.value = t('profile.currentEmailRequired')
    isUpdated.value = false
    return
  }

  isLoadingCurrentEmail.value = true
  message.value = ''

  const lang = localStorage.getItem('lang') || 'en'
  let res = null
  try {
    res = await authAxios.post(`/users/check-email?email=${emailData.value.currentEmail}`, {
      headers: {
        'Accept-Language': lang,
      },
    })
  } catch (error) {
    message.value = error.response?.data?.message?.value
    isUpdated.value = false
    return
  } finally {
    isLoadingCurrentEmail.value = false
  }

  currentEmailVerified.value = true
  isUpdated.value = true
}

const sendCode = async () => {
  if (!currentEmailVerified.value) {
    message.value = t('profile.currentEmailVerified')
    isUpdated.value = false
    return
  }

  if (!emailData.value.newEmail) {
    message.value = t('profile.newEmailRequired')
    isUpdated.value = false
    return
  }

  if (emailData.value.newEmail === emailData.value.currentEmail) {
    message.value = t('profile.emailDifferent')
    isUpdated.value = false
    return
  }

  isLoadingNewEmail.value = true
  message.value = ''
  try {
    await authAxios.post(
      `/users/code-change-email?&email=${emailData.value.newEmail}`,
    )
  } catch (error) {
    message.value = error.response?.data?.message?.value
    isUpdated.value = false
    return
  } finally {
    isLoadingNewEmail.value = false
  }

  isUpdated.value = true
  message.value = t('profile.emailChecking')
}

const saveProfile = async () => {
  const formData = new FormData()
  const lang = localStorage.getItem('lang') || 'en'
  if (newData.value.fullName) {
    formData.append('fullName', newData.value.fullName)
  }
  if (newData.value.username) {
    formData.append('username', newData.value.username)
  }
  if (passwordData.value.newPassword) {
    if (passwordData.value.newPassword !== passwordData.value.confirmPassword) {
      message.value = t('profile.passwordNotMatch')
      return
    }

    if (passwordData.value.currentPassword === passwordData.value.newPassword) {
      message.value = t('profile.passwordDifferent')
      return
    }

    formData.append('currentPassword', passwordData.value.currentPassword)
    formData.append('newPassword', passwordData.value.newPassword)
  }
  if (avatarFile.value) {
    formData.append('avatar', avatarFile.value)
  }
  if (newData.value.removeAvatar) {
    formData.append("removeAvatar", "true")
  }

  isSaving.value = true
  message.value = ''
  let res = null
  try {
    res = await authAxios.patch('/users/me', formData, {
      headers: {
        'Accept-Language': lang,
      },
    })
  } catch (error) {
    message.value = error.response?.data?.message
    isUpdated.value = false
    return
  } finally {
    isSaving.value = false
  }

  message.value = res.data?.message?.value
  isUpdated.value = true

  let timer = null
  if (timer) clearTimeout(timer)

  timer = setTimeout(() => {
    auth.fetchUser()
    message.value = ''
    isEditing.value = false
    isUpdated.value = false
  }, 1500)
}
</script>

<template>
  <div class="profile">
    <div class="profile__wrapper">
      <div class="container" style="max-width: 1000px">
        <div class="profile__header">
          <h1 class="profile__title">{{ t('profile.title') }}</h1>
          <p class="profile__subtitle">{{ t('profile.subtitle') }}</p>
        </div>

        <div class="profile__card">
          <div class="custom-tabs">
            <button
              :class="{ 'custom-tab': true, 'custom-tab--active': activeTab === 'profile' }"
              @click="activeTab = 'profile'"
            >
              <i class="bi bi-person-fill"></i>
              <span>{{ t('profile.account') }}</span>
            </button>
            <button
              :class="{ 'custom-tab': true, 'custom-tab--active': activeTab === 'email' }"
              @click="activeTab = 'email'"
            >
              <i class="bi bi-envelope-fill"></i>
              <span>{{ t('profile.changeEmail') }}</span>
            </button>
            <button
              :class="{ 'custom-tab': true, 'custom-tab--active': activeTab === 'password' }"
              @click="activeTab = 'password'"
            >
              <i class="bi bi-key"></i>
              <span>{{ t('profile.changePassword') }}</span>
            </button>
          </div>

          <div class="p-4 p-md-5">
            <div v-if="activeTab === 'profile'">
              <div class="avatar">
                <div class="avatar__wrapper">
                  <img
                    :src="avatarPreview || auth.user?.avatarUrl"
                    alt="Avatar"
                    class="avatar__img"
                  />
                  <label v-if="isEditing" class="avatar__overlay">
                    <i class="bi bi-camera-fill"></i>
                    <input
                      type="file"
                      ref="fileInput"
                      accept="image/*"
                      style="display: none"
                      @change="handleAvatarChange"
                    />
                  </label>
                </div>

                <div v-if="isEditing" class="avatar-actions">
                  <button type="button" class="remove-btn" @click="handleRemoveAvatar">
                    <i class="bi bi-trash"></i>
                    {{ t('admin.addAccount.remove') }}
                  </button>
                  <p class="avatar__hint">{{ t('profile.avatar') }}</p>
                </div>
              </div>

              <div class="row g-4">
                <div class="col-md-6">
                  <label for="fullName" class="form-label form__label">
                    {{ t('register.fullName') }}
                  </label>
                  <div class="input__wrapper">
                    <i class="bi bi-person-fill input__icon"></i>
                    <input
                      type="text"
                      class="form-control form__control"
                      :value="newData.fullName ?? auth.user?.fullName"
                      @input="(e) => (newData.fullName = e.target.value)"
                      :disabled="!isEditing"
                      id="fullName"
                      :placeholder="t('register.fullName.placeholder')"
                      :required="true"
                    />
                  </div>
                </div>

                <div class="col-md-6">
                  <label for="username" class="form-label form__label">
                    {{ t('register.username') }}
                  </label>
                  <div class="input__wrapper">
                    <i class="bi bi-at input__icon"></i>
                    <input
                      type="text"
                      class="form-control form__control"
                      :value="newData.username ?? auth.user?.username"
                      @input="(e) => (newData.username = e.target.value)"
                      :disabled="!isEditing"
                      id="username"
                      :placeholder="t('register.username.placeholder')"
                      :required="true"
                    />
                  </div>
                </div>

                <div class="col-12 mt-4">
                  <button
                    v-if="!isEditing"
                    class="btn btn-gradient-primary w-100"
                    @click="isEditing = true"
                  >
                    <i class="bi bi-pencil-fill me-2"></i>
                    {{ t('profile.edit') }}
                  </button>
                  <div v-else class="row g-3">
                    <div class="col-md-6">
                      <button
                        class="btn btn-gradient-success w-100"
                        @click="saveProfile"
                        :disabled="isLoadingCurrentEmail"
                      >
                        <span v-if="!isSaving">
                          <i class="bi bi-check-circle-fill me-2"></i>
                          {{ t('profile.save') }}
                        </span>
                        <span v-else class="btn__loading">
                          <span class="spinner"></span>
                          <span>{{ t('profile.processing') }}</span>
                        </span>
                      </button>
                    </div>
                    <div class="col-md-6">
                      <button class="btn btn-secondary w-100" @click="isEditing = false">
                        <i class="bi bi-x-circle-fill me-2"></i>
                        {{ t('profile.cancel') }}
                      </button>
                    </div>
                  </div>
                </div>

                <p
                  v-if="message"
                  :class="['message', isUpdated ? 'message--success' : 'message--error']"
                >
                  {{ message }}
                </p>
              </div>
            </div>

            <div v-if="activeTab === 'email'" class="row justify-content-center">
              <div class="col-lg-8">
                <div class="row g-4">
                  <!-- Current Email -->
                  <div class="col-12">
                    <label for="currentEmail" class="form-label form__label"> {{ t('profile.currentEmail') }}:</label>
                    <span class="input__hint">{{ maskedEmail }}</span>
                    <div class="input__wrapper">
                      <i class="bi bi-envelope-fill input__icon"></i>
                      <input
                        type="email"
                        class="form-control form__control"
                        id="currentEmail"
                        v-model="emailData.currentEmail"
                        :placeholder="t('profile.currentEmail.placeholder')"
                        :disabled="currentEmailVerified"
                      />
                    </div>
                  </div>

                  <div class="col-12">
                    <button
                      class="btn btn-gradient-primary w-100"
                      @click="verifyCurrentEmail"
                      :disabled="currentEmailVerified || isLoadingCurrentEmail"
                    >
                      <span v-if="!isLoadingCurrentEmail">
                        <i class="bi bi-check-circle-fill me-2"></i>
                        {{ currentEmailVerified ? 'Verified ✓' : 'Verify Current Email' }}
                      </span>
                      <span v-else class="btn__loading">
                        <span class="spinner"></span>
                        <span>{{ t('profile.processing') }}</span>
                      </span>
                    </button>
                  </div>

                  <!-- New Email -->
                  <div class="col-12 mt-4" v-if="currentEmailVerified">
                    <label for="newEmail" class="form-label form__label"> {{ t('profile.newEmail') }} </label>
                    <div class="input__wrapper">
                      <i class="bi bi-envelope-at-fill input__icon"></i>
                      <input
                        type="email"
                        class="form-control form__control"
                        id="newEmail"
                        v-model="emailData.newEmail"
                        :placeholder="t('profile.newEmail.placeholder')"
                      />
                    </div>
                  </div>

                  <div class="col-12" v-if="currentEmailVerified">
                    <button
                      class="btn btn-gradient-success w-100"
                      @click="sendCode"
                      :disabled="!emailData.newEmail || isLoadingNewEmail"
                    >
                      <span v-if="!isLoadingNewEmail">
                        <i class="bi bi-send-fill me-2"></i>
                        {{ t('profile.sendCode') }}
                      </span>
                      <span v-else class="btn__loading">
                        <span class="spinner"></span>
                        <span>{{ t('profile.processing') }}</span>
                      </span>
                    </button>
                  </div>

                  <p
                    v-if="message"
                    :class="['message', isUpdated ? 'message--success' : 'message--error']"
                  >
                    {{ message }}
                  </p>
                </div>
              </div>
            </div>

            <div v-if="activeTab === 'password'" class="row justify-content-center">
              <div class="col-lg-8">
                <div class="row g-4">
                  <div class="col-12">
                    <label for="currentPassword" class="form-label form__label">
                      {{ t('profile.currentPassword') }}
                    </label>
                    <div class="input__wrapper with-toggle">
                      <i class="bi bi-lock-fill input__icon"></i>
                      <input
                        :type="showCurrentPassword ? 'text' : 'password'"
                        class="form-control form__control"
                        id="currentPassword"
                        v-model="passwordData.currentPassword"
                        :placeholder="t('profile.currentPassword.placeholder')"
                      />
                      <button
                        type="button"
                        class="password-toggle"
                        @click="showCurrentPassword = !showCurrentPassword"
                      >
                        <i
                          :class="['bi', showCurrentPassword ? 'bi-eye-slash-fill' : 'bi-eye-fill']"
                        ></i>
                      </button>
                    </div>
                  </div>

                  <div class="col-12">
                    <label for="newPassword" class="form-label form__label">
                      {{ t('profile.newPassword') }}
                    </label>
                    <div class="input__wrapper with-toggle">
                      <i class="bi bi-lock-fill input__icon"></i>
                      <input
                        :type="showNewPassword ? 'text' : 'password'"
                        class="form-control form__control"
                        id="newPassword"
                        v-model="passwordData.newPassword"
                        :placeholder="t('profile.newPassword.placeholder')"
                      />
                      <button
                        type="button"
                        class="password-toggle"
                        @click="showNewPassword = !showNewPassword"
                      >
                        <i
                          :class="['bi', showNewPassword ? 'bi-eye-slash-fill' : 'bi-eye-fill']"
                        ></i>
                      </button>
                    </div>
                  </div>

                  <div class="col-12">
                    <label for="confirmPassword" class="form-label form__label">
                      {{ t('profile.confirm') }}
                    </label>
                    <div class="input__wrapper with-toggle">
                      <i class="bi bi-lock-fill input__icon"></i>
                      <input
                        :type="showConfirmPassword ? 'text' : 'password'"
                        class="form-control form__control"
                        id="confirmPassword"
                        v-model="passwordData.confirmPassword"
                        :placeholder="t('profile.confirm.placeholder')"
                      />
                      <button
                        type="button"
                        class="password-toggle"
                        @click="showConfirmPassword = !showConfirmPassword"
                      >
                        <i
                          :class="['bi', showConfirmPassword ? 'bi-eye-slash-fill' : 'bi-eye-fill']"
                        ></i>
                      </button>
                    </div>
                  </div>

                  <div class="col-12">
                    <div class="alert">
                      <div class="alert__title">
                        <i class="bi bi-shield-check"></i>
                        {{ t('profile.requirements') }}
                      </div>
                      <ul>
                        <li>{{ t('profile.req.first') }}</li>
                        <li>{{ t('profile.req.second') }}</li>
                        <li>{{ t('profile.req.third') }} (@, #, $,....)</li>
                      </ul>
                    </div>
                  </div>

                  <div class="col-12">
                    <button class="btn btn-gradient-primary w-100" @click="saveProfile">
                      <i class="bi bi-check-circle-fill me-2"></i>
                      {{ t('profile.changePassword') }}
                    </button>
                  </div>

                  <p
                    v-if="message"
                    :class="['message', isUpdated ? 'message--success' : 'message--error']"
                  >
                    {{ message }}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="security">
          <div class="d-flex gap-3">
            <div class="security__icon">
              <i class="bi bi-shield-check"></i>
            </div>
            <div>
              <h3 class="security__title">{{ t('profile.security') }}</h3>
              <p class="security__text">
                {{ t('profile.security.subtitle') }}
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.profile {
  margin-top: 70px;

  &__wrapper {
    min-height: 100vh;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    padding: 40px 20px;
  }

  &__header {
    text-align: center;
    margin-bottom: 40px;
  }

  &__title {
    font-size: 3rem;
    font-weight: 900;
    color: white;
    margin-bottom: 8px;
    text-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }

  &__subtitle {
    color: rgba(255, 255, 255, 0.9);
    font-size: 1.1rem;
  }

  &__card {
    background: white;
    border-radius: 24px;
    box-shadow: 0 20px 60px rgba(0, 0, 0, 0.15);
    overflow: hidden;
  }
}

/* Custom Tabs */
.custom-tabs {
  display: flex;
  border-bottom: 2px solid #e9ecef;
  background: #fafafa;
}

.custom-tab {
  flex: 1;
  padding: 20px;
  border: none;
  background: transparent;
  font-weight: 600;
  color: #6c757d;
  cursor: pointer;
  transition: all 0.3s;
  position: relative;
  font-size: 1rem;

  &:hover {
    background: #f0f0f0;
    color: #667eea;
  }

  &--active {
    color: #667eea;
    background: white;

    &::after {
      content: '';
      position: absolute;
      bottom: -2px;
      left: 0;
      right: 0;
      height: 3px;
      background: linear-gradient(90deg, #667eea 0%, #764ba2 100%);
    }
  }

  i {
    margin-right: 8px;
    font-size: 1.2rem;
  }
}

.input {
  &__wrapper {
    position: relative;

    .form-control {
      padding-left: 48px;
    }
  }

  &__icon {
    position: absolute;
    left: 16px;
    top: 50%;
    transform: translateY(-50%);
    color: #94a3b8;
    font-size: 1.2rem;
    z-index: 1;
  }

  &__hint {
    margin-top: 6px;
    margin-left: 10px;
    font-size: 0.85rem;
    color: #94a3b8;
  }
}

/* Avatar Section */
.avatar {
  text-align: center;
  margin-bottom: 40px;

  &__wrapper {
    position: relative;
    display: inline-block;
  }

  &__img {
    width: 160px;
    height: 160px;
    border-radius: 50%;
    object-fit: cover;
    border: 6px solid #667eea;
    box-shadow: 0 12px 40px rgba(102, 126, 234, 0.3);
    transition: all 0.3s;
  }

  &__overlay {
    position: absolute;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.65);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    opacity: 0;
    cursor: pointer;
    transition: opacity 0.3s;

    &:hover {
      opacity: 1;
    }

    i {
      color: white;
      font-size: 2.5rem;
    }
  }

  &__hint {
    color: #6c757d;
    font-size: 0.9rem;
    margin-top: 12px;
  }
}

.avatar-actions {
  margin-top: 12px;
}

.remove-btn {
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

/* Form Styles */
.form__label {
  font-weight: 600;
  color: #334155;
  margin-bottom: 8px;
  font-size: 0.95rem;
}

.form__control {
  padding: 14px 16px;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  font-size: 0.95rem;
  transition: all 0.3s;

  &:focus {
    border-color: #667eea;
    box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
    outline: none;
  }

  &:disabled {
    background-color: #f8f9fa;
    color: #6c757d;
  }
}

/* Input with Icon */
.input {
  &__wrapper {
    position: relative;

    .form-control {
      padding-left: 48px;
    }

    .password-toggle {
      position: absolute;
      right: 16px;
      top: 50%;
      transform: translateY(-50%);
      background: none;
      border: none;
      color: #94a3b8;
      cursor: pointer;
      padding: 4px;
      z-index: 1;
      transition: color 0.2s;

      &:hover {
        color: #667eea;
      }

      i {
        font-size: 1.2rem;
      }
    }
  }

  &__icon {
    position: absolute;
    left: 16px;
    top: 50%;
    transform: translateY(-50%);
    color: #94a3b8;
    font-size: 1.2rem;
    z-index: 1;
  }
}

.with-toggle {
  .form-control {
    padding-right: 48px;
  }
}

/* Buttons */
.btn-gradient-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  color: white;
  font-weight: 600;
  padding: 14px 32px;
  border-radius: 12px;
  transition: all 0.3s;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
    color: white;
  }

  &:active {
    transform: translateY(0);
  }
}

.btn-gradient-success {
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  border: none;
  color: white;
  font-weight: 600;
  padding: 14px 32px;
  border-radius: 12px;
  transition: all 0.3s;
  box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(16, 185, 129, 0.4);
    color: white;
  }
}

.btn-secondary {
  background: #e2e8f0;
  border: none;
  color: #475569;
  font-weight: 600;
  padding: 14px 32px;
  border-radius: 12px;
  transition: all 0.3s;

  &:hover {
    background: #cbd5e1;
    color: #334155;
  }
}

.btn__loading {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
}

/* Alert Box */
.alert {
  background: linear-gradient(135deg, #ede9fe 0%, #e0e7ff 100%);
  border-left: 4px solid #667eea;
  border-radius: 12px;
  padding: 20px;

  &__title {
    font-weight: 700;
    color: #334155;
    margin-bottom: 8px;
    display: flex;
    align-items: center;
    gap: 8px;

    i {
      color: #667eea;
      font-size: 1.3rem;
    }
  }

  ul {
    margin: 0;
    padding-left: 20px;
    color: #64748b;
  }

  li {
    margin-bottom: 4px;
  }
}

/* Security Card */
.security {
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  margin-top: 24px;

  &__icon {
    width: 56px;
    height: 56px;
    background: linear-gradient(135deg, #ede9fe 0%, #e0e7ff 100%);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;

    i {
      color: #667eea;
      font-size: 1.8rem;
    }
  }

  &__title {
    font-weight: 700;
    color: #1e293b;
    margin-bottom: 8px;
    font-size: 1.2rem;
  }

  &__text {
    color: #64748b;
    line-height: 1.6;
    margin: 0;
  }
}

.message {
  margin-bottom: 12px;
  border-radius: 12px;
  padding: 12px 16px;
  font-size: 0.88rem;
  font-weight: 600;
  margin-bottom: 20px;
  animation: slideDown 0.3s ease;

  &__error {
    background: #fef2f2;
    color: #dc2626;
    border: 1px solid #fecaca;
  }

  &__success {
    background: #f0fdf4;
    color: #16a34a;
    border: 1px solid #bbf7d0;
  }
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  padding: 20px;
  animation: fadeIn 0.3s ease;
}

.otp-modal {
  background: white;
  border-radius: 24px;
  max-width: 480px;
  width: 100%;
  padding: 40px;
  position: relative;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  animation: slideUp 0.3s ease;

  &__close {
    position: absolute;
    top: 20px;
    right: 20px;
    width: 36px;
    height: 36px;
    border-radius: 50%;
    border: none;
    background: #f1f5f9;
    color: #64748b;
    cursor: pointer;
    transition: all 0.2s;
    display: flex;
    align-items: center;
    justify-content: center;

    &:hover {
      background: #e2e8f0;
      color: #334155;
    }
  }

  &__header {
    text-align: center;
    margin-bottom: 32px;
  }

  &__icon {
    width: 80px;
    height: 80px;
    margin: 0 auto 20px;
    border-radius: 50%;
    background: linear-gradient(135deg, #ede9fe 0%, #e0e7ff 100%);
    display: flex;
    align-items: center;
    justify-content: center;

    i {
      font-size: 2.5rem;
      color: #667eea;
    }
  }

  &__title {
    font-size: 1.8rem;
    font-weight: 700;
    color: #1e293b;
    margin-bottom: 8px;
  }

  &__subtitle {
    color: #64748b;
    font-size: 0.95rem;
    line-height: 1.6;

    strong {
      color: #667eea;
      font-weight: 600;
    }
  }

  &__resend-container {
    text-align: center;
    margin-top: 24px;
    font-size: 0.9rem;
    color: #64748b;
  }

  &__resend-link {
    color: #667eea;
    font-weight: 700;
    cursor: pointer;
    transition: all 0.3s;

    &:hover {
      color: #5568d3;
    }
  }

  &__resend-disabled {
    color: #cbd5e1;
    cursor: not-allowed;
    text-decoration: none;
    pointer-events: none;
  }

  &__countdown {
    color: #64748b;
  }

  &__countdown-number {
    color: #667eea;
    font-weight: 600;
  }
}

.otp-inputs {
  display: flex;
  gap: 12px;
  justify-content: center;
}

.otp-input {
  width: 56px;
  height: 64px;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  text-align: center;
  font-size: 1.8rem;
  font-weight: 700;
  color: #1e293b;
  outline: none;
  transition: all 0.2s;

  &:focus {
    border-color: #667eea;
    box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
  }

  &:not(:placeholder-shown) {
    border-color: #667eea;
    background: #f8f9ff;
  }
}

.message {
  margin-top: 16px;
  border-radius: 12px;
  padding: 12px 16px;
  font-size: 0.88rem;
  font-weight: 600;
  animation: slideDown 0.3s ease;

  &--error {
    background: #fef2f2;
    color: #dc2626;
    border: 1px solid #fecaca;
  }

  &--success {
    background: #f0fdf4;
    color: #16a34a;
    border: 1px solid #bbf7d0;
  }
}

.security {
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
  margin-top: 24px;

  &__icon {
    width: 56px;
    height: 56px;
    background: linear-gradient(135deg, #ede9fe 0%, #e0e7ff 100%);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    flex-shrink: 0;

    i {
      color: #667eea;
      font-size: 1.8rem;
    }
  }

  &__title {
    font-weight: 700;
    color: #1e293b;
    margin-bottom: 8px;
    font-size: 1.2rem;
  }

  &__text {
    color: #64748b;
    line-height: 1.6;
    margin: 0;
  }
}

.spinner {
  width: 18px;
  height: 18px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes fadeIn {
  from {
    opacity: 0;
  }
  to {
    opacity: 1;
  }
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(30px);
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

@media (max-width: 768px) {
  .otp-modal {
    padding: 30px 20px;
  }

  .otp-input {
    width: 48px;
    height: 56px;
    font-size: 1.5rem;
  }

  .otp-inputs {
    gap: 8px;
  }

  .custom-tab span {
    display: none;
  }

  .custom-tab i {
    margin: 0;
  }
}
</style>
