<script setup>
import { usePaymentStore } from '@/stores/payment'
import { QrcodeCanvas } from 'qrcode.vue'
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/api/api'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const router = useRouter()
const payment = usePaymentStore()
const paymentSuccess = ref(false)
const copy = ref({
  accountNumber: false,
  amount: false,
  content: false,
})
const id = payment.info?.id
const name = payment.info?.name
const qrCode = payment.info?.qrCode
const bankName = 'Ngan Hang TMCP Quan Doi'
const accountNumber = payment.info?.accountNumber
const accountName = payment.info?.accountName
const amount = payment.info?.amount
const description = payment.info?.description

const handleCopy = async (text, field) => {
  try {
    await navigator.clipboard.writeText(text)
    copy.value = {
      ...copy.value,
      [field]: true,
    }
    setTimeout(() => {
      copy.value = {
        ...copy.value,
        [field]: false,
      }
    }, 2000)
  } catch (error) {
    console.error('Failed to copy:', error)
  }
}

const formatPrice = (amount) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(amount)
}

onMounted(() => {
  if (!payment.info) {
    router.push('/error')
  }
})

let timer = null
onMounted(() => {
  const orderCode = description.replace(/\D/g, "")
  if (timer) clearInterval(timer)

  timer = setInterval(async() => {
    const res = await api.get('/payments/status', {
      params: {
        orderCode: orderCode
      }
    })
    const data = res.data
    if (data === 'PAID') {
      paymentSuccess.value = true
      clearInterval(timer)
      await api.delete('/wishlists/remove', {
        params: {
          itemId: id
        }
      })
    }
  }, 5000)
})
</script>

<template>
  <div class="paymentQR">
    <div class="container">
      <div v-if="!paymentSuccess">
        <div class="header">
          <button class="backBtn" @click="handleBackToHome">
            <i class="bi bi-arrow-left"></i>
          </button>
          <h1 class="title">
            {{ t('qr.title') }}
            <i>{{ name }}</i>
          </h1>
        </div>
        <div class="content">
          <div class="qrCard">
            <div class="qrHeader">
              <h2 class="qrTitle">{{ t('qr.scan') }}</h2>
              <p class="qrSubtitle">{{ t('qr.use') }}</p>
            </div>
            <div class="qrImageWrapper">
              <div class="qrImageContainer">
                <QrcodeCanvas :value="qrCode" :size="250" level="M" class="qrImage" />
                <div class="qrOverlay">
                  <div class="corner" data-position="top-left"></div>
                  <div className="corner" data-position="top-right"></div>
                  <div className="corner" data-position="bottom-left"></div>
                  <div className="corner" data-position="bottom-right"></div>
                </div>
              </div>
            </div>
          </div>

          <div class="detailsCard">
            <div class="detailsHeader">
              <h3 class="detailsTitle">{{ t('qr.info') }}</h3>
              <span class="statusBadge">
                <i class="bi bi-circle-fill"></i>
                {{ t('qr.pending') }}
              </span>
            </div>

            <div class="detailsList">
              <div class="detailSection">
                <div class="sectionHeader">
                  <i class="bi bi-bank"></i>
                  <h4>{{ t('qr.bankInfo') }}</h4>
                </div>
                <div class="detailItem">
                  <span class="label">{{ t('qr.bankName') }}</span>
                  <span class="value">{{ bankName }}</span>
                </div>
                <div class="detailItem">
                  <span class="label"> {{ t('qr.accountName') }} </span>
                  <span class="value">
                    {{ accountName }}
                  </span>
                </div>
                <div class="detailItem">
                  <span class="label"> {{ t('qr.accountNumber') }} </span>
                  <div class="valueWithCopy">
                    <span class="value">
                      {{ accountNumber }}
                    </span>
                    <button
                      class="copyBtn"
                      @click="handleCopy(accountNumber, 'accountNumber')"
                    >
                      <div v-if="copy.accountNumber">
                        <i class="bi bi-check2"></i>
                        <span>{{ t('qr.copied') }}</span>
                      </div>
                      <div v-else>
                        <i class="bi bi-copy"></i>
                        <span>{{ t('qr.copy') }}</span>
                      </div>
                    </button>
                  </div>
                </div>
              </div>

              <div class="detailSection">
                <div class="sectionHeader">
                  <i class="bi bi-cash-coin"></i>
                  <h4>{{ t('qr.details') }}</h4>
                </div>
                <div class="detailItem">
                  <span class="label"> {{ t('qr.amount') }} </span>
                  <div class="valueWithCopy">
                    <span class="value amount">
                      {{ formatPrice(amount) }}
                    </span>
                    <button
                      class="copyBtn"
                      @click="handleCopy(amount.toString(), 'amount')"
                    >
                      <div v-if="copy.amount">
                        <i class="bi bi-check2"></i>
                        <span>{{ t('qr.copied') }}</span>
                      </div>
                      <div v-else>
                        <i class="bi bi-copy"></i>
                        <span>{{ t('qr.copy') }}</span>
                      </div>
                    </button>
                  </div>
                </div>

                <div class="detailItem">
                  <span class="label"> {{ t('qr.content') }} </span>
                  <div class="valueWithCopy">
                    <span class="value">
                      {{ description }}
                    </span>
                    <button class="copyBtn" @click="handleCopy(description, 'description')">
                      <div v-if="copy.content">
                        <i class="bi bi-check2"></i>
                        <span>{{ t('qr.copied') }}</span>
                      </div>
                      <div v-else>
                        <i class="bi bi-copy"></i>
                        <span>{{ t('qr.copy') }}</span>
                      </div>
                    </button>
                  </div>
                </div>
              </div>
            </div>

            <div class="notesSection">
              <div class="noteItem">
                <i class="bi bi-exclamation-circle"></i>
                <p>
                  {{ t('qr.noteFirst') }}
                  <strong>{{ description }}</strong>
                </p>
              </div>
              <div class="noteItem">
                <i class="bi bi-shield-check"></i>
                <p>{{ t('qr.noteSecond') }}</p>
              </div>
              <div class="noteItem">
                <i class="bi bi-telephone"></i>
                <p>{{ t('qr.noteThird') }} <strong>0833210030</strong></p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div v-else class="successModal">
        <div class="successContent">
          <div class="successIcon">
            <i class="bi bi-check-circle-fill"></i>
          </div>
          <h2 class="successTitle">
            {{ t('qr.success') }}
          </h2>
          <p class="successMessage">
            {{ t('qr.message') }} <strong>{{ name }}</strong>
          </p>
          <button class="homeBtn" @click="router.push('/my-enrollments')">
            <i class="bi bi-house-door-fill"></i>
            {{ t('qr.goTo') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.paymentQR {
  min-height: 100vh;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  padding: 40px 20px;
  padding-top: 120px;
  font-family:
    'Plus Jakarta Sans',
    -apple-system,
    BlinkMacSystemFont,
    sans-serif;
  display: flex;
  align-items: center;
  justify-content: center;
}

.container {
  max-width: 1200px;
  width: 100%;
  margin: 0 auto;
}

// Header
.header {
  display: flex;
  align-items: center;
  justify-content: center;
  position: relative;
  margin-bottom: 32px;
  animation: fadeInDown 0.5s ease;
}

.backBtn {
  position: absolute;
  left: 0;
  width: 48px;
  height: 48px;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.2);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.3);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s;

  i {
    font-size: 1.3rem;
  }

  &:hover {
    background: rgba(255, 255, 255, 0.3);
    transform: translateX(-4px);
  }
}

.title {
  font-size: 2rem;
  font-weight: 800;
  color: #ffffff;
  margin: 0;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  text-align: center;

  i {
    font-style: italic;
    font-weight: 600;
  }
}

// Content
.content {
  display: grid;
  grid-template-columns: 480px 1fr;
  gap: 32px;
  animation: fadeInUp 0.6s ease;
}

// QR Card
.qrCard {
  background: #ffffff;
  border-radius: 24px;
  padding: 40px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  display: flex;
  flex-direction: column;
  align-items: center;
}

.qrHeader {
  text-align: center;
  margin-bottom: 32px;
}

.qrTitle {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1f2937;
  margin-bottom: 8px;
}

.qrSubtitle {
  font-size: 0.95rem;
  color: #6b7280;
  margin: 0;
}

.qrImageWrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 24px;
  margin-bottom: 24px;
}

.qrImageContainer {
  position: relative;
  padding: 20px;
  background: #f9fafb;
  border-radius: 16px;
}

.qrImage {
  display: block;
  border-radius: 8px;
}

.qrOverlay {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  pointer-events: none;
}

.corner {
  position: absolute;
  width: 40px;
  height: 40px;
  border: 3px solid #667eea;

  &[data-position='top-left'] {
    top: 10px;
    left: 10px;
    border-right: none;
    border-bottom: none;
    border-radius: 8px 0 0 0;
  }

  &[data-position='top-right'] {
    top: 10px;
    right: 10px;
    border-left: none;
    border-bottom: none;
    border-radius: 0 8px 0 0;
  }

  &[data-position='bottom-left'] {
    bottom: 10px;
    left: 10px;
    border-right: none;
    border-top: none;
    border-radius: 0 0 0 8px;
  }

  &[data-position='bottom-right'] {
    bottom: 10px;
    right: 10px;
    border-left: none;
    border-top: none;
    border-radius: 0 0 8px 0;
  }
}

// Details Card
.detailsCard {
  background: #ffffff;
  border-radius: 24px;
  padding: 40px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.detailsHeader {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-bottom: 20px;
  border-bottom: 2px solid #f3f4f6;
}

.detailsTitle {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
}

.statusBadge {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: #fef3c7;
  color: #92400e;
  border-radius: 20px;
  font-weight: 700;
  font-size: 0.875rem;

  i {
    font-size: 0.5rem;
    animation: blink 1.5s infinite;
  }
}

@keyframes blink {
  0%,
  100% {
    opacity: 1;
  }
  50% {
    opacity: 0.3;
  }
}

// Detail Sections
.detailsList {
  display: flex;
  flex-direction: column;
  gap: 32px;
}

.detailSection {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.sectionHeader {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;

  i {
    font-size: 1.3rem;
    color: #667eea;
  }

  h4 {
    font-size: 1.1rem;
    font-weight: 700;
    color: #374151;
    margin: 0;
  }
}

.detailItem {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 16px 20px;
  background: #f9fafb;
  border-radius: 12px;
  transition: all 0.3s;

  &:hover {
    background: #f3f4f6;
  }
}

.label {
  font-size: 0.9rem;
  color: #6b7280;
  font-weight: 600;
}

.value {
  font-size: 1rem;
  color: #1f2937;
  font-weight: 600;
  text-align: right;

  &.amount {
    font-size: 1.5rem;
    color: #667eea;
    font-weight: 800;
  }
}

.valueWithCopy {
  display: flex;
  align-items: center;
  gap: 12px;
}

.copyBtn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  background: #667eea;
  color: #ffffff;
  border: none;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s;
  white-space: nowrap;

  i {
    font-size: 0.9rem;
  }

  &:hover {
    background: #5568d3;
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
  }

  &:active {
    transform: translateY(0);
  }
}

// Notes Section
.notesSection {
  display: flex;
  flex-direction: column;
  gap: 12px;
  padding: 20px;
  background: #f0f9ff;
  border: 1px solid #bae6fd;
  border-radius: 12px;
}

.noteItem {
  display: flex;
  gap: 12px;
  align-items: flex-start;

  i {
    color: #0284c7;
    font-size: 1.1rem;
    margin-top: 2px;
    flex-shrink: 0;
  }

  p {
    color: #075985;
    font-size: 0.9rem;
    margin: 0;
    line-height: 1.6;

    strong {
      color: #0c4a6e;
      font-weight: 700;
    }
  }
}

// Success Modal
.successModal {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 60vh;
  animation: zoomIn 0.5s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.successContent {
  background: #ffffff;
  border-radius: 32px;
  padding: 60px 80px;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
  text-align: center;
  max-width: 600px;
}

.successIcon {
  margin-bottom: 24px;
  animation: scaleIn 0.6s cubic-bezier(0.34, 1.56, 0.64, 1) 0.2s both;

  i {
    font-size: 8rem;
    color: #10b981;
    filter: drop-shadow(0 4px 12px rgba(16, 185, 129, 0.3));
  }
}

.successTitle {
  font-size: 2.5rem;
  font-weight: 800;
  color: #1f2937;
  margin-bottom: 16px;
  animation: fadeInUp 0.5s ease 0.3s both;
}

.successMessage {
  font-size: 1.1rem;
  color: #6b7280;
  line-height: 1.7;
  margin-bottom: 32px;
  animation: fadeInUp 0.5s ease 0.4s both;

  strong {
    color: #667eea;
    font-weight: 700;
  }
}

.homeBtn {
  display: inline-flex;
  align-items: center;
  gap: 12px;
  padding: 16px 40px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff;
  border: none;
  border-radius: 16px;
  font-weight: 700;
  font-size: 1.1rem;
  cursor: pointer;
  transition: all 0.3s;
  box-shadow: 0 4px 16px rgba(102, 126, 234, 0.3);
  animation: fadeInUp 0.5s ease 0.5s both;

  i {
    font-size: 1.3rem;
  }

  &:hover {
    transform: translateY(-4px);
    box-shadow: 0 8px 24px rgba(102, 126, 234, 0.5);
  }

  &:active {
    transform: translateY(-2px);
  }
}

// Animations
@keyframes fadeInDown {
  from {
    opacity: 0;
    transform: translateY(-30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes zoomIn {
  from {
    opacity: 0;
    transform: scale(0.8);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}

@keyframes scaleIn {
  from {
    transform: scale(0);
    opacity: 0;
  }
  to {
    transform: scale(1);
    opacity: 1;
  }
}

// Responsive
@media (max-width: 1024px) {
  .content {
    grid-template-columns: 1fr;
    gap: 24px;
  }

  .qrCard {
    max-width: 480px;
    margin: 0 auto;
    width: 100%;
  }
}

@media (max-width: 768px) {
  .paymentQR {
    padding: 20px;
    padding-top: 100px;
  }

  .header {
    margin-bottom: 24px;
  }

  .title {
    font-size: 1.5rem;
  }

  .backBtn {
    width: 40px;
    height: 40px;

    i {
      font-size: 1.1rem;
    }
  }

  .qrCard,
  .detailsCard {
    padding: 24px;
  }

  .successContent {
    padding: 40px 30px;
  }

  .successIcon i {
    font-size: 6rem;
  }

  .successTitle {
    font-size: 2rem;
  }

  .successMessage {
    font-size: 1rem;
  }
}

@media (max-width: 480px) {
  .qrImageContainer {
    padding: 15px;
  }

  .corner {
    width: 30px;
    height: 30px;
  }

  .detailItem {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }

  .value {
    text-align: left;
    width: 100%;
  }

  .valueWithCopy {
    width: 100%;
    flex-direction: column;
    align-items: stretch;
  }

  .copyBtn {
    width: 100%;
    justify-content: center;
  }

  .successContent {
    padding: 30px 20px;
  }

  .successIcon i {
    font-size: 5rem;
  }

  .successTitle {
    font-size: 1.75rem;
  }

  .homeBtn {
    width: 100%;
    justify-content: center;
  }
}
</style>
