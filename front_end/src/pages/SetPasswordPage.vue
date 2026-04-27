<script setup>
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import api from '@/api/api'
import logo from '../images/logo.png'

const { t } = useI18n()
const router = useRouter()
const route = useRoute()

const passwordData = ref({
  password: '',
  confirmPassword: '',
})
const showPassword = ref(false)
const message = ref('')
const isSuccess = ref(false)
const isLoading = ref(false)
const token = route.query.token
const userId = route.query.userId

const handleSubmit = async () => {
  message.value = ''

  if (!passwordData.value.password) {
    message.value = 'Please enter a password'
    isSuccess.value = false
    return
  }

  if (passwordData.value.password !== passwordData.value.confirmPassword) {
    message.value = 'Passwords do not match'
    isSuccess.value = false
    return
  }

  isLoading.value = true

  let res = null
  try {
    res = await api.post('/auth/reset-password', {
      userId: userId,
      token: token,
      newPassword: passwordData.value.password,
    })
  } catch (error) {
    message.value = error.response?.data?.message
    isSuccess.value = false
  } finally {
    isLoading.value = false
  }

  isSuccess.value = true
  message.value = res.data?.message?.value

  setTimeout(() => {
    router.push('/login')
  }, 2000)
}

onMounted(async () => {
  // if (!route.query.userId || !route.query.token) {
  //   router.push('/error', { replace: true })
  //   return
  // }
  // // Verify token and get user email
  // try {
  //   const response = await api.get(`/auth/verify-token?token=${token.value}`)
  //   userEmail.value = response.data.email
  // } catch (error) {
  //   message.value = 'Invalid or expired link'
  //   isSuccess.value = false
  // }
})
</script>

<template>
  <div class="set-password">
    <div class="set-password__container">
      <div class="set-password__image"></div>

      <div class="set-password__card-wrapper">
        <div class="set-password__card">
          <!-- Logo & Header -->
          <div class="set-password__logo">
            <RouterLink to="/home">
              <img :src="logo" alt="Logo" class="set-password__logo-img" />
            </RouterLink>
            <h1 class="set-password__title">Set Your Password</h1>
            <p class="set-password__subtitle">Create a secure password for your account</p>
          </div>

          <form @submit.prevent="handleSubmit">
            <!-- Password Input -->
            <div class="set-password__form-group">
              <label for="password" class="set-password__label"> Password </label>
              <div class="set-password__password-wrapper">
                <input
                  :type="showPassword ? 'text' : 'password'"
                  name="password"
                  id="password"
                  v-model="passwordData.password"
                  class="set-password__input set-password__input--password"
                  placeholder="Enter your password"
                  :required="true"
                  :disabled="isLoading"
                />
                <button
                  type="button"
                  class="set-password__toggle-password"
                  @click="showPassword = !showPassword"
                  :disabled="isLoading"
                >
                  <i v-if="showPassword" class="bi bi-eye-slash-fill"></i>
                  <i v-else class="bi bi-eye-fill"></i>
                </button>
              </div>
            </div>

            <!-- Confirm Password Input -->
            <div class="set-password__form-group">
              <label for="confirmPassword" class="set-password__label"> Confirm Password </label>
              <div class="set-password__password-wrapper">
                <input
                  :type="showPassword ? 'text' : 'password'"
                  name="confirmPassword"
                  id="confirmPassword"
                  v-model="passwordData.confirmPassword"
                  class="set-password__input set-password__input--password"
                  placeholder="Confirm your password"
                  :required="true"
                  :disabled="isLoading"
                />
                <button
                  type="button"
                  class="set-password__toggle-password"
                  @click="showPassword = !showPassword"
                  :disabled="isLoading"
                >
                  <i v-if="showPassword" class="bi bi-eye-slash-fill"></i>
                  <i v-else class="bi bi-eye-fill"></i>
                </button>
              </div>
            </div>

            <!-- Password Requirements -->
            <div class="requirements">
              <div class="requirements__title">
                <i class="bi bi-shield-check"></i>
                Password Requirements
              </div>
              <ul class="requirements__list">
                <li>{{ t('profile.req.first') }}</li>
                <li>{{ t('profile.req.second') }}</li>
                <li>{{ t('profile.req.third') }} (@, #, $,....)</li>
              </ul>
            </div>

            <!-- Message -->
            <p
              v-if="message"
              :class="[
                'set-password__message',
                isSuccess ? 'set-password__message--success' : 'set-password__message--error',
              ]"
            >
              {{ message }}
            </p>

            <!-- Submit Button -->
            <button type="submit" class="set-password__button" :disabled="isLoading">
              <span v-if="!isLoading">
                <i class="bi bi-shield-lock-fill me-2"></i>
                Set Password
              </span>
              <span v-else class="set-password__button-loading">
                <span class="spinner"></span>
                <span>Processing...</span>
              </span>
            </button>

            <!-- Back to Login -->
            <div class="set-password__link">
              Already have a password?
              <RouterLink to="/login">Sign In</RouterLink>
            </div>
          </form>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.set-password {
  &__container {
    display: flex;
    height: 100vh;
    width: 100%;
    background-image: url('https://images.unsplash.com/photo-1614064641938-3bbee52942c7?q=80&w=2070');
    background-size: cover;
    background-position: center;
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
    max-width: 500px;
    animation: fadeIn 0.6s ease;
    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
    background: #fff;
    border-radius: 15px;
    padding: 40px;
  }

  &__logo {
    text-align: center;
    margin-bottom: 40px;

    &-img {
      width: 100px;
      margin-bottom: 20px;
    }
  }

  &__title {
    font-size: 2.2rem;
    font-weight: 700;
    color: #1e293b;
    margin-bottom: 10px;
  }

  &__subtitle {
    color: #64748b;
    font-size: 1rem;
    margin-bottom: 8px;
  }

  &__email {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    background: linear-gradient(135deg, #ede9fe 0%, #e0e7ff 100%);
    padding: 8px 16px;
    border-radius: 8px;
    color: #667eea;
    font-size: 0.9rem;
    font-weight: 600;
    margin-top: 12px;

    i {
      font-size: 1rem;
    }
  }

  &__form-group {
    margin-bottom: 20px;
  }

  &__label {
    font-weight: 600;
    color: #334155;
    margin-bottom: 8px;
    font-size: 0.9rem;
    display: block;
  }

  &__input {
    width: 100%;
    height: 48px;
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
    height: 50px;
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

    &:hover:not(:disabled) {
      transform: translateY(-2px);
      box-shadow: 0 8px 25px rgba(102, 126, 234, 0.4);
    }

    &:active:not(:disabled) {
      transform: translateY(0);
    }

    &:disabled {
      opacity: 0.7;
      cursor: not-allowed;
    }

    &-loading {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 10px;
    }
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
}

.password-strength {
  margin-top: 8px;
  display: flex;
  align-items: center;
  gap: 12px;

  &__bar {
    flex: 1;
    height: 6px;
    background: #e2e8f0;
    border-radius: 3px;
    overflow: hidden;
  }

  &__fill {
    height: 100%;
    transition: all 0.3s;
    border-radius: 3px;
  }

  &__text {
    font-size: 0.85rem;
    font-weight: 600;
    min-width: 60px;
  }
}

.password-match {
  margin-top: 8px;
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 0.85rem;
  font-weight: 600;

  &--success {
    color: #10b981;
  }

  &--error {
    color: #ef4444;
  }

  i {
    font-size: 1rem;
  }
}

.requirements {
  background: linear-gradient(135deg, #ede9fe 0%, #e0e7ff 100%);
  border-left: 4px solid #667eea;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 24px;

  &__title {
    font-weight: 700;
    color: #334155;
    margin-bottom: 12px;
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 0.95rem;

    i {
      color: #667eea;
      font-size: 1.2rem;
    }
  }

  &__list {
    margin: 0;
    padding: 0;
  }

  &__list li {
    color: #64748b;
    margin-bottom: 8px;
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 0.9rem;
    transition: all 0.3s;

    i {
      font-size: 1rem;
      color: #cbd5e1;
    }

    &.requirements__item--met {
      color: #10b981;

      i {
        color: #10b981;
      }
    }
  }
}

.spinner {
  width: 18px;
  height: 18px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
  display: inline-block;
  flex-shrink: 0;
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

@media (max-width: 768px) {
  .set-password {
    &__container {
      flex-direction: column;
    }

    &__image {
      height: 200px;
      flex: none;
    }

    &__card-wrapper {
      padding: 30px 20px;
    }

    &__logo-img {
      width: 70px;
    }

    &__title {
      font-size: 1.8rem;
    }
  }
}
</style>
