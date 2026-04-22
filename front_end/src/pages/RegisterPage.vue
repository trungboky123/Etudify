<script setup>
import { useI18n } from 'vue-i18n'
import logo from '../images/logo.png'
import { ref } from 'vue'
import api from '@/api/api'

const { t } = useI18n()
const user = ref({
  fullName: '',
  username: '',
  email: '',
  password: '',
  confirmPassword: '',
})
const showPassword = ref(false)
const isRegistered = ref(false)
const message = ref('')
const isLoading = ref(false) // Loading state

const handleSubmit = async () => {
  const fullName = user.value.fullName.trim()
  const username = user.value.username.trim()
  const email = user.value.email.trim()
  const password = user.value.password.trim()
  const confirmPassword = user.value.confirmPassword.trim()

  if (password !== confirmPassword) {
    message.value = t('register.password.notMatch')
    isRegistered.value = false
    return
  }

  isLoading.value = true
  message.value = ''

  const lang = localStorage.getItem('lang') || 'en'
  let res = null
  try {
    res = await api.post(
      '/auth/register',
      {
        fullName,
        username,
        email,
        password,
      },
      {
        headers: {
          'Accept-Language': lang,
        },
      },
    )
  } catch (error) {
    message.value = error.response?.data?.message
    isRegistered.value = false
    return
  } finally {
    isLoading.value = false
  }

  isRegistered.value = true
  message.value = res.data?.message?.value
}
</script>

<template>
  <div class="register min-h-screen flex">
    <div class="register__container">
      <div class="register__image"></div>

      <div class="register__card-wrapper">
        <div class="register__card">
          <div class="register__logo">
            <RouterLink to="/home">
              <img :src="logo" alt="Logo" class="register__logo-img" />
            </RouterLink>
            <h1 class="register__title">{{ t('register.text') }}</h1>
            <p class="register__subtitle">{{ t('register.subtitle') }}</p>
          </div>

          <form @submit.prevent="handleSubmit">
            <div class="register__form-group">
              <label for="fullName" class="register__label">{{ t('register.fullName') }}</label>
              <input
                type="text"
                name="fullName"
                id="fullName"
                v-model="user.fullName"
                class="register__input"
                :placeholder="t('register.fullName.placeholder')"
                :required="true"
                :disabled="isLoading"
              />
            </div>

            <div class="register__form-group">
              <label for="username" class="register__label">{{ t('register.username') }}</label>
              <input
                type="text"
                name="username"
                id="username"
                v-model="user.username"
                class="register__input"
                :placeholder="t('register.username.placeholder')"
                :required="true"
                :disabled="isLoading"
              />
            </div>

            <div class="register__form-group">
              <label for="email" class="register__label">{{ t('register.email') }}</label>
              <input
                type="email"
                name="email"
                id="email"
                v-model="user.email"
                class="register__input"
                :placeholder="t('register.email.placeholder')"
                :required="true"
                :disabled="isLoading"
              />
            </div>

            <div class="register__form-group">
              <label for="password" class="register__label">{{ t('register.password') }}</label>
              <div class="register__password-wrapper">
                <input
                  :type="showPassword ? 'text' : 'password'"
                  name="password"
                  id="password"
                  v-model="user.password"
                  class="register__input register__input--password"
                  :placeholder="t('register.password.placeholder')"
                  :required="true"
                  :disabled="isLoading"
                />
                <button
                  type="button"
                  class="register__toggle-password"
                  @click="showPassword = !showPassword"
                  :disabled="isLoading"
                >
                  <svg
                    v-if="showPassword"
                    width="20"
                    height="20"
                    fill="currentColor"
                    viewBox="0 0 16 16"
                  >
                    <path
                      d="M13.359 11.238C15.06 9.72 16 8 16 8s-3-5.5-8-5.5a7.028 7.028 0 0 0-2.79.588l.77.771A5.944 5.944 0 0 1 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.134 13.134 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755-.165.165-.337.328-.517.486l.708.709z"
                    />
                    <path
                      d="M11.297 9.176a3.5 3.5 0 0 0-4.474-4.474l.823.823a2.5 2.5 0 0 1 2.829 2.829l.822.822zm-2.943 1.299.822.822a3.5 3.5 0 0 1-4.474-4.474l.823.823a2.5 2.5 0 0 0 2.829 2.829z"
                    />
                    <path
                      d="M3.35 5.47c-.18.16-.353.322-.518.487A13.134 13.134 0 0 0 1.172 8l.195.288c.335.48.83 1.12 1.465 1.755C4.121 11.332 5.881 12.5 8 12.5c.716 0 1.39-.133 2.02-.36l.77.772A7.029 7.029 0 0 1 8 13.5C3 13.5 0 8 0 8s.939-1.721 2.641-3.238l.708.709zm10.296 8.884-12-12 .708-.708 12 12-.708.708z"
                    />
                  </svg>
                  <svg v-else width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
                    <path
                      d="M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8zM1.173 8a13.133 13.133 0 0 1 1.66-2.043C4.12 4.668 5.88 3.5 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.133 13.133 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755C11.879 11.332 10.119 12.5 8 12.5c-2.12 0-3.879-1.168-5.168-2.457A13.134 13.134 0 0 1 1.172 8z"
                    />
                    <path
                      d="M8 5.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5zM4.5 8a3.5 3.5 0 1 1 7 0 3.5 3.5 0 0 1-7 0z"
                    />
                  </svg>
                </button>
              </div>
            </div>

            <div class="register__form-group">
              <label for="confirmPassword" class="register__label">{{
                t('register.confirmPassword')
              }}</label>
              <div class="register__password-wrapper">
                <input
                  :type="showPassword ? 'text' : 'password'"
                  name="confirmPassword"
                  id="confirmPassword"
                  v-model="user.confirmPassword"
                  class="register__input register__input--password"
                  :placeholder="t('register.confirm.placeholder')"
                  :required="true"
                  :disabled="isLoading"
                />
                <button
                  type="button"
                  class="register__toggle-password"
                  @click="showPassword = !showPassword"
                  :disabled="isLoading"
                >
                  <svg
                    v-if="showPassword"
                    width="20"
                    height="20"
                    fill="currentColor"
                    viewBox="0 0 16 16"
                  >
                    <path
                      d="M13.359 11.238C15.06 9.72 16 8 16 8s-3-5.5-8-5.5a7.028 7.028 0 0 0-2.79.588l.77.771A5.944 5.944 0 0 1 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.134 13.134 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755-.165.165-.337.328-.517.486l.708.709z"
                    />
                    <path
                      d="M11.297 9.176a3.5 3.5 0 0 0-4.474-4.474l.823.823a2.5 2.5 0 0 1 2.829 2.829l.822.822zm-2.943 1.299.822.822a3.5 3.5 0 0 1-4.474-4.474l.823.823a2.5 2.5 0 0 0 2.829 2.829z"
                    />
                    <path
                      d="M3.35 5.47c-.18.16-.353.322-.518.487A13.134 13.134 0 0 0 1.172 8l.195.288c.335.48.83 1.12 1.465 1.755C4.121 11.332 5.881 12.5 8 12.5c.716 0 1.39-.133 2.02-.36l.77.772A7.029 7.029 0 0 1 8 13.5C3 13.5 0 8 0 8s.939-1.721 2.641-3.238l.708.709zm10.296 8.884-12-12 .708-.708 12 12-.708.708z"
                    />
                  </svg>
                  <svg v-else width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
                    <path
                      d="M16 8s-3-5.5-8-5.5S0 8 0 8s3 5.5 8 5.5S16 8 16 8zM1.173 8a13.133 13.133 0 0 1 1.66-2.043C4.12 4.668 5.88 3.5 8 3.5c2.12 0 3.879 1.168 5.168 2.457A13.133 13.133 0 0 1 14.828 8c-.058.087-.122.183-.195.288-.335.48-.83 1.12-1.465 1.755C11.879 11.332 10.119 12.5 8 12.5c-2.12 0-3.879-1.168-5.168-2.457A13.134 13.134 0 0 1 1.172 8z"
                    />
                    <path
                      d="M8 5.5a2.5 2.5 0 1 0 0 5 2.5 2.5 0 0 0 0-5zM4.5 8a3.5 3.5 0 1 1 7 0 3.5 3.5 0 0 1-7 0z"
                    />
                  </svg>
                </button>
              </div>
            </div>

            <p
              v-if="message"
              :class="[
                'register__message',
                isRegistered ? 'register__message--success' : 'register__message--error',
              ]"
            >
              {{ message }}
            </p>

            <button
              type="submit"
              class="register__button"
              :disabled="isLoading"
              :class="{ 'register__button--loading': isLoading }"
            >
              <span v-if="!isLoading">{{ t('register.text') }}</span>
              <span v-else class="register__button-content">
                <span class="register__spinner"></span>
                <span>Processing...</span>
              </span>
            </button>

            <div class="register__link">
              {{ t('register.hadAccount') }}
              <RouterLink to="/login">{{ t('login.text') }}</RouterLink>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.register {
  &__container {
    display: flex;
    height: 100vh;
    width: 100%;
  }

  &__image {
    flex: 1.5;
    background: url('https://codequotient.com/blog/wp-content/uploads/2020/12/6-Reasons-Why-Having-a-Professional-Coder-Certification-Is-Crucial.jpg');
    background-size: cover;
    background-position: center;
    min-height: 100vh;
    position: relative;

    &::before {
      content: '';
      position: absolute;
      inset: 0;
    }
  }

  &__card-wrapper {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
    background: white;
    padding: 40px;
    overflow-y: auto;
  }

  &__card {
    width: 100%;
    max-width: 450px;
    animation: fadeIn 0.6s ease;
    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
    background: #fff;
    border-radius: 15px;
    padding: 30px;
    margin-top: 150px;
  }

  &__logo {
    text-align: center;
    margin-bottom: 40px;

    &-img {
      width: 150px;
      margin: 0;
    }
  }

  &__title {
    font-size: 2rem;
    font-weight: 700;
    color: #1e293b;
    margin-bottom: 10px;
  }

  &__subtitle {
    color: #64748b;
    font-size: 0.95rem;
  }

  &__form-group {
    margin-bottom: 18px;
  }

  &__label {
    font-weight: 600;
    color: #334155;
    margin-bottom: 8px;
    font-size: 0.85rem;
    display: block;
  }

  &__input {
    width: 100%;
    height: 45px;
    border: 2px solid #e2e8f0;
    border-radius: 12px;
    padding: 12px 16px;
    font-size: 0.95rem;
    transition: all 0.3s;
    outline: none;

    &:focus {
      border-color: #667eea;
      box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
    }

    &--password {
      padding-right: 50px;
    }

    &:disabled {
      background: #f8fafc;
      cursor: not-allowed;
      opacity: 0.6;
    }
  }

  &__password-wrapper {
    position: relative;
  }

  &__toggle-password {
    position: absolute;
    right: 16px;
    top: 50%;
    transform: translateY(-50%);
    background: none;
    border: none;
    color: #64748b;
    cursor: pointer;
    font-size: 1.2rem;
    padding: 4px;
    transition: color 0.3s;

    &:hover:not(:disabled) {
      color: #667eea;
    }

    &:disabled {
      cursor: not-allowed;
      opacity: 0.5;
    }
  }

  &__message {
    font-size: 0.9rem;
    margin-bottom: 12px;
    padding: 12px 16px;
    border-radius: 12px;
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

  &__button {
    width: 100%;
    height: 45px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border: none;
    border-radius: 12px;
    color: white;
    font-weight: 700;
    font-size: 1rem;
    text-transform: uppercase;
    letter-spacing: 0.5px;
    transition: all 0.3s;
    box-shadow: 0 4px 15px rgba(102, 126, 234, 0.3);
    cursor: pointer;
    position: relative;

    &:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4);
    }

    &:active:not(:disabled) {
      transform: translateY(0);
    }

    &:disabled {
      cursor: not-allowed;
      opacity: 0.7;
    }

    &--loading {
      pointer-events: none;
    }

    &-content {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 10px;
    }
  }

  &__spinner {
    width: 18px;
    height: 18px;
    border: 3px solid rgba(255, 255, 255, 0.3);
    border-top-color: white;
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
  }

  &__link {
    text-align: center;
    margin-top: 24px;
    font-size: 0.9rem;
    color: #64748b;

    a {
      color: #667eea;
      text-decoration: none;
      font-weight: 700;
      transition: color 0.3s;

      &:hover {
        color: #5568d3;
      }
    }
  }

  @media (max-width: 768px) {
    &__container {
      flex-direction: column;
    }

    &__card-wrapper {
      padding: 30px 20px;
    }

    &__image {
      display: none !important;
      height: 0 !important;
      min-height: 0 !important;
      flex: 0 !important;
    }

    &__title {
      font-size: 1.5rem;
    }
  }
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(20px);
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
</style>
