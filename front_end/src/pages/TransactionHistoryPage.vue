<script setup>
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import api from '@/api/api'

const { t, locale } = useI18n()
const isLoading = ref(false)
const groupedData = ref([])

const getAllItems = async () => {
  isLoading.value = true
  const res = await api.get('/payments/transaction')
  groupedData.value = res.data
  isLoading.value = false
}

const formatDate = (dateString) => {
  const [year, month, day] = dateString.split('-')
  return `${day}/${month}/${year}`
}

const formatDateTime = (utcString) =>
  computed(() => {
    if (!utcString) return ''
    const iso = utcString.split('.')[0].replace(' ', 'T') + 'Z'

    const date = new Date(iso)

    if (locale.value === 'vi') {
      return date.toLocaleString('vi-VN', {
        timeZone: 'Asia/Ho_Chi_Minh',
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false,
      })
    } else if (locale.value === 'fr') {
      return date.toLocaleString('fr-FR', {
        timeZone: 'Asia/Ho_Chi_Minh',
        day: 'numeric',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false,
      })
    } else {
      return date.toLocaleString('en-US', {
        timeZone: 'Asia/Ho_Chi_Minh',
        day: 'numeric',
        month: 'short',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit',
        hour12: false,
      })
    }
  })

const formatCurrency = (amount) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(amount)
}

const calculateTotalByDate = (payments) => {
  return payments.reduce((sum, p) => sum + p.amount, 0)
}

const grandTotal = computed(() => {
  return groupedData.value.reduce((sum, group) => sum + calculateTotalByDate(group.payments), 0)
})

const totalTransactions = computed(() => {
  return groupedData.value.reduce((sum, group) => sum + group.payments.length, 0)
})

onMounted(() => {
  getAllItems()
})
</script>

<template>
  <div class="history">
    <div class="container">
      <div class="header">
        <div class="headerContent">
          <h1 class="title">
            <i class="bi bi-receipt"></i>
            Transaction History
          </h1>
          <p class="subtitle">View your payment history and transaction details.</p>
        </div>
      </div>

      <div class="summary">
        <div class="summaryCard">
          <div class="summaryIcon">
            <i class="bi bi-cart-check"></i>
          </div>
          <div class="summaryContent">
            <p class="summaryLabel">Total Transactions</p>
            <p class="summaryValue">{{ totalTransactions }}</p>
          </div>
        </div>
        <div class="summaryCard">
          <div class="summaryIcon">
            <i class="bi bi-wallet2"></i>
          </div>
          <div class="summaryContent">
            <p class="summaryLabel">Total Spent</p>
            <p class="summaryValue">{{ formatCurrency(grandTotal) }}</p>
          </div>
        </div>
      </div>

      <div class="timeline">
        <div v-if="isLoading" class="loading">
          <div class="spinner"></div>
          <p>Loading...</p>
        </div>
        <div v-else-if="groupedData.length === 0" class="empty">
          <i class="bi bi-inbox"></i>
          <h3>No Transactions Yet</h3>
          <p>You haven't made any purchases yet</p>
        </div>
        <div v-else>
          <div v-for="group in groupedData" :key="group.date" class="dateGroup">
            <div class="dateHeader">
              <div class="dateBadge">
                <i class="bi bi-calendar-event"></i>
                <span class="dateText">{{ formatDate(group.date) }}</span>
              </div>
              <div class="dateLine"></div>
              <div class="dateTotal">
                {{ formatCurrency(calculateTotalByDate(group.payments)) }}
              </div>
            </div>

            <div class="cards">
              <div v-for="payment in group.payments" :key="payment.itemId" class="card">
                <div class="cardImage">
                  <img :src="payment.thumbnailUrl" :alt="payment.itemName" />
                </div>
                <div class="cardBody">
                  <h3 class="cardTitle">{{ payment.itemName }}</h3>
                  <div class="cardMeta">
                    <div class="metaItem">
                      <i class="bi bi-clock"></i>
                      <span>{{ formatDateTime(payment.paidAt) }}</span>
                    </div>
                  </div>
                  <div class="cardFooter">
                    <div class="amount">
                      <span class="amountLabel">Amount Paid</span>
                      <span class="amountValue">
                        {{ formatCurrency(payment.amount) }}
                      </span>
                    </div>
                    <div class="status statusPaid">
                      <i class="bi bi-check-circle-fill"></i>
                      <span>{{ payment.status }}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.history {
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #e9ecef 100%);
  padding: 40px 20px 80px;
  margin-top: 70px;
  font-family:
    'Plus Jakarta Sans',
    -apple-system,
    BlinkMacSystemFont,
    sans-serif;
}

// ─── Header ──────────────────────────────────────────────────────────────────
.header {
  margin-bottom: 40px;
  text-align: center;
}

.headerContent {
  max-width: 600px;
  margin: 0 auto;
}

.title {
  font-size: 2.5rem;
  font-weight: 800;
  color: #1e293b;
  margin-bottom: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;

  i {
    color: #667eea;
    font-size: 2.3rem;
  }
}

.subtitle {
  font-size: 1.1rem;
  color: #64748b;
}

// ─── Summary Cards ───────────────────────────────────────────────────────────
.summary {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 20px;
  margin-bottom: 48px;
}

.summaryCard {
  display: flex;
  align-items: center;
  gap: 20px;
  background: white;
  padding: 24px;
  border-radius: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.06);
  transition: all 0.3s;

  &:hover {
    transform: translateY(-4px);
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.1);
  }
}

.summaryIcon {
  width: 64px;
  height: 64px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 16px;
  flex-shrink: 0;

  i {
    font-size: 1.8rem;
    color: white;
  }
}

.summaryContent {
  flex: 1;
}

.summaryLabel {
  font-size: 0.9rem;
  font-weight: 500;
  color: #64748b;
  margin: 0 0 6px;
}

.summaryValue {
  font-size: 1.8rem;
  font-weight: 800;
  color: #1e293b;
  margin: 0;
}

// ─── Timeline ────────────────────────────────────────────────────────────────
.timeline {
  position: relative;
}

.dateGroup {
  margin-bottom: 48px;

  &:last-child {
    margin-bottom: 0;
  }
}

// ─── Date Header ─────────────────────────────────────────────────────────────
.dateHeader {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 24px;
}

.dateBadge {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 20px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
  flex-shrink: 0;

  i {
    font-size: 1.1rem;
    color: white;
  }
}

.dateText {
  font-size: 1rem;
  font-weight: 700;
  color: white;
  letter-spacing: 0.02em;
}

.dateLine {
  flex: 1;
  height: 2px;
  background: linear-gradient(90deg, #e2e8f0 0%, transparent 100%);
  border-radius: 2px;
}

.dateTotal {
  font-size: 1.1rem;
  font-weight: 700;
  color: #667eea;
  padding: 8px 16px;
  background: #f0f2ff;
  border-radius: 10px;
  border: 1.5px solid #e0e7ff;
}

// ─── Transaction Cards ───────────────────────────────────────────────────────
.cards {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 20px;
}

.card {
  background: white;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.07);
  transition: all 0.3s;
  display: flex;
  flex-direction: column;
  border: 2px solid transparent;

  &:hover {
    transform: translateY(-4px);
    box-shadow: 0 12px 28px rgba(0, 0, 0, 0.12);
    border-color: #667eea;
  }
}

.cardImage {
  position: relative;
  width: 100%;
  height: 180px;
  overflow: hidden;
  flex-shrink: 0;

  img {
    width: 100%;
    height: 100%;
    object-fit: cover;
    transition: transform 0.4s ease;
  }

  &:hover img {
    transform: scale(1.05);
  }
}

.typeBadge {
  position: absolute;
  top: 12px;
  left: 12px;
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 6px 12px;
  background: rgba(255, 255, 255, 0.95);
  backdrop-filter: blur(4px);
  border-radius: 8px;
  font-size: 0.75rem;
  font-weight: 700;
  color: #667eea;
  letter-spacing: 0.3px;
  text-transform: uppercase;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);

  i {
    font-size: 0.85rem;
  }
}

// ─── Card Body ───────────────────────────────────────────────────────────────
.cardBody {
  padding: 18px 20px 20px;
  display: flex;
  flex-direction: column;
  flex: 1;
  gap: 12px;
}

.cardTitle {
  font-size: 1.05rem;
  font-weight: 700;
  color: #1e293b;
  line-height: 1.45;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  margin: 0;
}

// ─── Card Meta ───────────────────────────────────────────────────────────────
.cardMeta {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.metaItem {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.85rem;
  color: #64748b;
  font-weight: 500;

  i {
    font-size: 0.95rem;
    color: #94a3b8;
  }
}

// ─── Card Footer ─────────────────────────────────────────────────────────────
.cardFooter {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  margin-top: auto;
  padding-top: 14px;
  border-top: 1px solid #e2e8f0;
  gap: 12px;
}

.amount {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.amountLabel {
  font-size: 0.75rem;
  font-weight: 500;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.amountValue {
  font-size: 1.4rem;
  font-weight: 800;
  color: #667eea;
  line-height: 1;
}

.status {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 12px;
  background: #dcfce7;
  border-radius: 8px;
  font-size: 0.8rem;
  font-weight: 600;
  color: #166534;
  white-space: nowrap;

  i {
    font-size: 0.9rem;
    color: #10b981;
  }
}

// ─── Empty State ─────────────────────────────────────────────────────────────
.empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 100px 20px;
  text-align: center;

  i {
    font-size: 5rem;
    color: #e2e8f0;
    margin-bottom: 24px;
    display: block;
  }
  h3 {
    font-size: 1.8rem;
    font-weight: 700;
    color: #1e293b;
    margin-bottom: 12px;
  }
  p {
    font-size: 1.05rem;
    color: #64748b;
    max-width: 480px;
    margin-bottom: 24px;
  }
}

.emptyBtn {
  padding: 12px 28px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 10px;
  font-weight: 700;
  font-size: 0.97rem;
  cursor: pointer;
  transition: all 0.3s;
  font-family: inherit;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
  }
}

// ─── Loading ─────────────────────────────────────────────────────────────────
.loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 100px 20px;
  text-align: center;

  p {
    font-size: 1.05rem;
    color: #64748b;
    margin-top: 20px;
    font-weight: 500;
  }
}

.spinner {
  width: 50px;
  height: 50px;
  border: 4px solid #e2e8f0;
  border-top-color: #667eea;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

// ─── Responsive ──────────────────────────────────────────────────────────────
@media (max-width: 768px) {
  .title {
    font-size: 2rem;
  }

  .summary {
    grid-template-columns: 1fr;
  }

  .dateHeader {
    flex-wrap: wrap;
  }

  .dateLine {
    display: none;
  }

  .dateTotal {
    width: 100%;
    text-align: center;
  }

  .cards {
    grid-template-columns: 1fr;
  }

  .cardFooter {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  .status {
    width: 100%;
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .history {
    padding: 20px 10px 60px;
  }
  .title {
    font-size: 1.6rem;
  }
  .subtitle {
    font-size: 0.95rem;
  }
  .summaryValue {
    font-size: 1.5rem;
  }
  .amountValue {
    font-size: 1.2rem;
  }
}
</style>
