<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import axios from 'axios'

const router = useRouter()
const route = useRoute()
const status = ref(route.query.status === 'true')
const isResending = ref(false)
const resendMessage = ref('')

const goToLogin = () => {
  router.push('/login')
}

const resendVerification = async () => {
  isResending.value = true
  resendMessage.value = ''

  try {
    // Replace with your actual API endpoint
    await axios.post('http://localhost:5062/auth/resend-verification', {
      token: route.query.token,
    })

    resendMessage.value = 'Verification email sent! Please check your email'

  } catch (error) {
    resendMessage.value = error.response?.data?.message || 'Failed to resend verification email. Please try again.'
  } finally {
    isResending.value = false
  }
}

onMounted(() => {
  if (!route.query.token) {
    router.push('/error', { replace: true })
  }
})
</script>

<template>
  <div class="result">
    <div class="result__container">
      <div class="result__card">
        <!-- Success State -->
        <div v-if="status">
          <!-- Success Icon -->
          <div class="result__icon-wrapper">
            <div class="result__icon result__icon--success">
              <i class="bi bi-check-circle-fill"></i>
            </div>
            <div class="result__pulse result__pulse--success"></div>
          </div>

          <!-- Header -->
          <h1 class="result__title">Email Verification</h1>

          <!-- Message -->
          <p class="result__message">
            Your email has been verified, you can login with your email now!
          </p>

          <!-- Actions -->
          <div class="result__actions">
            <button class="btn btn-gradient-primary" @click="goToLogin">
              <i class="bi bi-box-arrow-in-right me-2"></i>
              Go to Login
            </button>
          </div>
        </div>

        <!-- Failed State -->
        <div v-else>
          <!-- Error Icon -->
          <div class="result__icon-wrapper">
            <div class="result__icon result__icon--error">
              <i class="bi bi-x-circle-fill"></i>
            </div>
            <div class="result__pulse result__pulse--error"></div>
          </div>

          <!-- Header -->
          <h1 class="result__title result__title--error">Verification Failed</h1>

          <!-- Message -->
          <p class="result__message">
            Your Email verification link has expired! Please resend a new verification link
          </p>

          <!-- Resend Message -->
          <p
            v-if="resendMessage"
            class="result__resend-message"
            :class="
              resendMessage.includes('sent')
                ? 'result__resend-message--success'
                : 'result__resend-message--error'
            "
          >
            {{ resendMessage }}
          </p>

          <!-- Actions -->
          <div class="result__actions">
            <button
              class="btn btn-gradient-warning"
              @click="resendVerification"
              :disabled="isResending"
            >
              <span v-if="!isResending">
                <i class="bi bi-send-fill me-2"></i>
                Resend Verification Link
              </span>
              <span v-else class="btn__loading">
                <span class="spinner"></span>
                <span>Sending...</span>
              </span>
            </button>
            <button class="btn btn-outline" @click="goToLogin">
              <i class="bi bi-arrow-left me-2"></i>
              Back to Login
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.result {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 20px;
  position: relative;
  overflow: hidden;

  &__container {
    position: relative;
    z-index: 1;
    max-width: 600px;
    width: 100%;
  }

  &__card {
    background: white;
    border-radius: 32px;
    padding: 60px 40px;
    text-align: center;
    box-shadow: 0 30px 90px rgba(0, 0, 0, 0.2);
    animation: slideUp 0.6s ease;
    position: relative;
  }

  &__icon-wrapper {
    position: relative;
    display: inline-block;
    margin-bottom: 32px;
  }

  &__icon {
    width: 120px;
    height: 120px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    margin: 0 auto;
    position: relative;
    z-index: 2;

    &--success {
      background: linear-gradient(135deg, #10b981 0%, #059669 100%);
      box-shadow: 0 12px 40px rgba(16, 185, 129, 0.4);
    }

    &--error {
      background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
      box-shadow: 0 12px 40px rgba(239, 68, 68, 0.4);
    }

    i {
      font-size: 4rem;
      color: white;
      animation: bounceIn 0.8s ease;
    }
  }

  &__pulse {
    position: absolute;
    top: 0;
    left: 0;
    width: 120px;
    height: 120px;
    border-radius: 50%;
    animation: pulse 2s infinite;
    z-index: 1;

    &--success {
      background: rgba(16, 185, 129, 0.3);
    }

    &--error {
      background: rgba(239, 68, 68, 0.3);
    }
  }

  &__title {
    font-size: 2.5rem;
    font-weight: 900;
    color: #1e293b;
    margin-bottom: 16px;
    animation: fadeIn 0.8s ease 0.2s both;

    &--error {
      color: #dc2626;
    }
  }

  &__message {
    font-size: 1.1rem;
    color: #64748b;
    line-height: 1.8;
    margin-bottom: 32px;
    animation: fadeIn 0.8s ease 0.4s both;
    max-width: 480px;
    margin-left: auto;
    margin-right: auto;
  }

  &__resend-message {
    padding: 12px 20px;
    border-radius: 12px;
    font-size: 0.9rem;
    font-weight: 600;
    margin-bottom: 24px;
    animation: slideDown 0.3s ease;

    &--success {
      background: #f0fdf4;
      color: #16a34a;
      border: 1px solid #bbf7d0;
    }

    &--error {
      background: #fef2f2;
      color: #dc2626;
      border: 1px solid #fecaca;
    }
  }

  &__actions {
    display: flex;
    gap: 16px;
    justify-content: center;
    flex-wrap: wrap;
    animation: fadeIn 0.8s ease 0.8s both;
  }
}

.btn {
  padding: 14px 32px;
  border-radius: 12px;
  font-weight: 600;
  font-size: 1rem;
  border: none;
  cursor: pointer;
  transition: all 0.3s;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 180px;

  &__loading {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  &:disabled {
    opacity: 0.6;
    cursor: not-allowed;
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

.btn-gradient-primary {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);

  &:hover:not(:disabled) {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
  }

  &:active:not(:disabled) {
    transform: translateY(0);
  }
}

.btn-gradient-warning {
  background: linear-gradient(135deg, #f59e0b 0%, #d97706 100%);
  color: white;
  box-shadow: 0 4px 12px rgba(245, 158, 11, 0.3);

  &:hover:not(:disabled) {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(245, 158, 11, 0.4);
  }

  &:active:not(:disabled) {
    transform: translateY(0);
  }
}

.btn-outline {
  background: transparent;
  color: #667eea;
  border: 2px solid #667eea;

  &:hover {
    background: rgba(102, 126, 234, 0.1);
    transform: translateY(-2px);
  }

  &:active {
    transform: translateY(0);
  }
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(40px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
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

@keyframes bounceIn {
  0% {
    transform: scale(0);
  }
  50% {
    transform: scale(1.1);
  }
  100% {
    transform: scale(1);
  }
}

@keyframes pulse {
  0% {
    transform: scale(1);
    opacity: 0.6;
  }
  50% {
    transform: scale(1.3);
    opacity: 0;
  }
  100% {
    transform: scale(1);
    opacity: 0;
  }
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 768px) {
  .result {
    &__card {
      padding: 40px 24px;
    }

    &__icon {
      width: 100px;
      height: 100px;

      i {
        font-size: 3rem;
      }
    }

    &__pulse {
      width: 100px;
      height: 100px;
    }

    &__title {
      font-size: 2rem;
    }

    &__message {
      font-size: 1rem;
    }

    &__actions {
      flex-direction: column;
      width: 100%;

      .btn {
        width: 100%;
      }
    }
  }
}
</style>
