<script setup>
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler,
} from 'chart.js'
import { computed, onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import authAxios from '@/function/authAxios'
import { Line } from 'vue-chartjs'

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler,
)

const { sidebarCollapsed } = defineProps({
  sidebarCollapsed: Boolean,
})
const { t } = useI18n()
const stats = ref({
  totalUsers: 0,
  totalCourses: 0,
  totalRevenue: 0,
})
const monthlyRevenue = ref([])
const topCourses = ref([])
const recentUsers = ref([])

const formatNumber = (num) => {
  return new Intl.NumberFormat('vi-VN').format(num)
}

const formatCurrency = (amount) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(amount)
}

const statCards = computed(() => [
  {
    title: t('admin.dashboard.totalUsers'),
    value: formatNumber(stats.value.totalUsers),
    icon: 'bi-people-fill',
    color: '#667eea',
    bgColor: 'rgba(102, 126, 234, 0.1)',
    change: '+12.5%',
    changeType: 'up',
  },
  {
    title: t('admin.dashboard.totalCourses'),
    value: formatNumber(stats.value.totalCourses),
    icon: 'bi-book-fill',
    color: '#10b981',
    bgColor: 'rgba(16, 185, 129, 0.1)',
    change: '+8.2%',
    changeType: 'up',
  },
  {
    title: t('admin.dashboard.monthlyRevenue'),
    value: formatCurrency(stats.value.totalRevenue),
    icon: 'bi-currency-dollar',
    color: '#8b5cf6',
    bgColor: 'rgba(139, 92, 246, 0.1)',
    change: '+18.3%',
    changeType: 'up',
  },
])

const revenueChartData = computed(() => ({
  labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
  datasets: [
    {
      label: 'Monthly Revenue',
      data: monthlyRevenue.value,
      borderColor: '#667eea',
      backgroundColor: 'rgba(102, 126, 234, 0.1)',
      fill: true,
      tension: 0.4,
      pointRadius: 6,
      pointHoverRadius: 8,
      pointBackgroundColor: '#667eea',
      pointBorderColor: '#fff',
      pointBorderWidth: 2,
    },
  ],
}))

const revenueChartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false,
    },
    tooltip: {
      backgroundColor: '#1f2937',
      padding: 12,
      titleColor: '#fff',
      bodyColor: '#fff',
      borderColor: '#374151',
      borderWidth: 1,
      displayColors: false,
      callbacks: {
        label: (context) => formatCurrency(context.parsed.y),
      },
    },
  },
  scales: {
    y: {
      beginAtZero: true,
      grid: {
        color: '#f3f4f6',
        drawBorder: false,
      },
      ticks: {
        callback: (value) => {
          return value >= 1000000 ? `${value / 1000000}M` : value
        },
      },
    },
    x: {
      grid: {
        display: false,
      },
    },
  },
}

const getTotalUsers = async () => {
  const res = await authAxios.get('/users/total')
  stats.value.totalUsers = res.data?.totalUsers
}

const getTotalCourses = async () => {
  const res = await authAxios.get('/courses/total')
  stats.value.totalCourses = res.data?.totalCourses
}

const getTotalRevenue = async () => {
  const res = await authAxios.get('/payments/total-revenue')
  stats.value.totalRevenue = res.data?.totalRevenue
}

const getMonthlyRevenue = async () => {
  const res = await authAxios.get('/payments/monthly-revenue')
  monthlyRevenue.value = res.data.map((item) => Number(item.revenue))
}

const topSoldCourses = async () => {
  const res = await authAxios.get('/payments/top-courses')
  topCourses.value = res.data
}

const getRecentUsers = async () => {
  const res = await authAxios.get('/users/recent')
  recentUsers.value = res.data
}

onMounted(() => {
  getTotalUsers()
  getTotalCourses()
  getTotalRevenue()
  getMonthlyRevenue()
  topSoldCourses()
  getRecentUsers()
})
</script>

<template>
  <div
    class="dashboard"
    :style="{ transition: 'margin-left 0.3s', marginLeft: sidebarCollapsed ? '85px' : '280px' }"
  >
    <div class="container">
      <div class="header">
        <div>
          <h1 class="title">Dashboard</h1>
          <p class="subtitle">{{ t('admin.dashboard.subtitle') }}</p>
        </div>
      </div>

      <div class="statsGrid">
        <div v-for="(card, index) in statCards" :key="index" class="statCard">
          <div class="statCardHeader">
            <div class="statIcon" :style="{ backgroundColor: card.bgColor }">
              <i :class="`bi ${card.icon}`" :style="{ color: card.color }"></i>
            </div>
          </div>
          <div class="statCardBody">
            <h3 class="statValue">{{ card.value }}</h3>
            <p class="statTitle">{{ card.title }}</p>
          </div>
        </div>
      </div>

      <div class="chartSection">
        <div class="chartCard">
          <div class="chartHeader">
            <div>
              <h2 class="chartTitle">{{ t('admin.dashboard.revenueOverview') }}</h2>
              <p class="chartSubtitle">
                {{ t('admin.dashboard.revenueSubtitle') }}
                {{ new Date().getFullYear() }}
              </p>
            </div>
            <div class="chartLegend">
              <div class="legendItem">
                <span class="legendDot"></span>
                <span>{{ t('admin.dashboard.revenue') }}</span>
              </div>
            </div>
          </div>
          <div class="chartBody">
            <Line :data="revenueChartData" :options="revenueChartOptions" />
          </div>
        </div>
      </div>

      <div class="topSellingGrid">
        <div class="topSellingCard">
          <div class="topSellingHeader">
            <h2 class="topSellingTitle">
              <i class="bi bi-trophy-fill"></i>
              {{ t('admin.dashboard.topCourses') }}
            </h2>
          </div>
          <div class="topSellingBody">
            <div v-for="(course, index) in topCourses" :key="course.id" class="topItem">
              <div class="topItemRank">{{ index + 1 }}</div>
              <img :src="course.thumbnailUrl" :alt="course.courseName" class="topItemImage" />
              <div class="topItemInfo">
                <h4 class="topItemName">{{ course.courseName }}</h4>
                <p class="topItemStats">
                  {{ course.totalSold }} {{ t('admin.dashboard.sold') }} •
                  {{ formatCurrency(course.totalRevenue) }}
                </p>
              </div>
              <div class="topItemBadge">
                <i class="bi bi-fire"></i>
              </div>
            </div>
          </div>
        </div>

        <div class="topSellingCard">
          <div class="topSellingHeader">
            <h2 class="topSellingTitle">
              <i class="bi bi-people-fill"></i>
              {{ t('admin.dashboard.recentUsers') }}
            </h2>
          </div>
          <div class="topSellingBody">
            <div v-for="(user, index) in recentUsers" :key="user.id" class="topItem">
              <div class="topItemRank">{{ index + 1 }}</div>
              <img :src="user.avatarUrl" :alt="user.fullName" class="topItemImage" />
              <div class="topItemInfo">
                <h4 class="topItemName">{{ user.fullName }}</h4>
                <p class="topItemStats">
                  {{ user.username }}
                  <br />
                  {{ user.email }}
                </p>
              </div>
              <div class="topItemBadge">
                <i class="bi bi-star-fill"></i>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
@import url('https://fonts.googleapis.com/css2?family=Plus+Jakarta+Sans:wght@400;500;600;700;800&display=swap');

.dashboard {
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #f0f2f5 100%);
  padding: 40px;
  margin-left: 280px;
  margin-top: 70px;
  font-family:
    'Plus Jakarta Sans',
    -apple-system,
    BlinkMacSystemFont,
    sans-serif;
  transition: margin-left 0.3s cubic-bezier(0.4, 0, 0.2, 1);

  &.sidebarCollapsed {
    margin-left: 85px;
  }
}

.container {
  max-width: 1600px;
  margin: 0 auto;
}

// Header
.header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 32px;
  animation: fadeInDown 0.5s ease;
}

.title {
  font-size: 2.25rem;
  font-weight: 800;
  color: #1f2937;
  margin-bottom: 8px;
  letter-spacing: -0.02em;
}

.subtitle {
  font-size: 1rem;
  color: #6b7280;
  margin: 0;
  font-weight: 500;
}

.headerActions {
  display: flex;
  gap: 12px;
}

.refreshBtn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  color: #374151;
  font-weight: 600;
  font-size: 0.95rem;
  cursor: pointer;
  transition: all 0.3s;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);

  i {
    font-size: 1.1rem;
  }

  &:hover {
    background: #667eea;
    color: #ffffff;
    border-color: #667eea;
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);

    i {
      animation: rotate 0.6s ease;
    }
  }
}

@keyframes rotate {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

// Stats Grid
.statsGrid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 24px;
  margin-bottom: 32px;
  animation: fadeInUp 0.6s ease;
}

.statCard {
  background: #ffffff;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  border: 1px solid #f3f4f6;

  &:hover {
    transform: translateY(-4px);
    box-shadow: 0 12px 24px rgba(0, 0, 0, 0.1);
    border-color: #e5e7eb;
  }
}

.statCardHeader {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.statIcon {
  width: 56px;
  height: 56px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: transform 0.3s;

  i {
    font-size: 1.5rem;
  }

  .statCard:hover & {
    transform: scale(1.1);
  }
}

.statChange {
  display: flex;
  align-items: center;
  gap: 4px;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 0.875rem;
  font-weight: 600;

  &[data-type='up'] {
    background: #d1fae5;
    color: #065f46;
  }

  &[data-type='down'] {
    background: #fee2e2;
    color: #991b1b;
  }

  i {
    font-size: 0.75rem;
  }
}

.statCardBody {
  margin-top: 12px;
}

.statValue {
  font-size: 2rem;
  font-weight: 800;
  color: #1f2937;
  margin-bottom: 4px;
  letter-spacing: -0.02em;
}

.statTitle {
  font-size: 0.95rem;
  color: #6b7280;
  margin: 0;
  font-weight: 500;
}

// Chart Section
.chartSection {
  margin-bottom: 32px;
  animation: fadeInUp 0.7s ease;
}

.chartCard {
  background: #ffffff;
  border-radius: 16px;
  padding: 32px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  border: 1px solid #f3f4f6;
}

.chartHeader {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 32px;
}

.chartTitle {
  font-size: 1.5rem;
  font-weight: 700;
  color: #1f2937;
  margin-bottom: 4px;
  letter-spacing: -0.01em;
}

.chartSubtitle {
  font-size: 0.9rem;
  color: #6b7280;
  margin: 0;
}

.chartLegend {
  display: flex;
  gap: 24px;
}

.legendItem {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.9rem;
  color: #6b7280;
  font-weight: 500;
}

.legendDot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: #667eea;
}

.chartBody {
  height: 400px;
  position: relative;
}

// Top Selling Grid
.topSellingGrid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(500px, 1fr));
  gap: 24px;
  animation: fadeInUp 0.8s ease;
}

.topSellingCard {
  background: #ffffff;
  border-radius: 16px;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  border: 1px solid #f3f4f6;
  overflow: hidden;
}

.topSellingHeader {
  padding: 24px 24px 20px;
  border-bottom: 1px solid #f3f4f6;
}

.topSellingTitle {
  font-size: 1.25rem;
  font-weight: 700;
  color: #1f2937;
  margin: 0;
  display: flex;
  align-items: center;
  gap: 10px;

  i {
    color: #f59e0b;
    font-size: 1.3rem;
  }
}

.topSellingBody {
  padding: 8px;
}

.topItem {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px;
  border-radius: 12px;
  transition: all 0.3s;
  position: relative;
  overflow: hidden;

  &::before {
    content: '';
    position: absolute;
    left: 0;
    top: 0;
    height: 100%;
    width: 4px;
    background: linear-gradient(180deg, #667eea 0%, #764ba2 100%);
    opacity: 0;
    transition: opacity 0.3s;
  }

  &:hover {
    background: #f9fafb;
    transform: translateX(4px);

    &::before {
      opacity: 1;
    }

    .topItemImage {
      transform: scale(1.1);
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
    }

    .topItemBadge {
      transform: scale(1.1) rotate(12deg);
    }
  }
}

.topItemRank {
  width: 36px;
  height: 36px;
  border-radius: 8px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 1.1rem;
  flex-shrink: 0;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3);
}

.topItemImage {
  width: 60px;
  height: 60px;
  border-radius: 10px;
  object-fit: cover;
  flex-shrink: 0;
  border: 2px solid #f3f4f6;
  transition: all 0.3s;
}

.topItemInfo {
  flex: 1;
  min-width: 0;
}

.topItemName {
  font-size: 1rem;
  font-weight: 600;
  color: #1f2937;
  margin: 0 0 6px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.topItemStats {
  font-size: 0.875rem;
  color: #6b7280;
  margin: 0;
  font-weight: 500;
}

.topItemBadge {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: linear-gradient(135deg, #fbbf24 0%, #f59e0b 100%);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  transition: transform 0.3s;

  i {
    color: #ffffff;
    font-size: 0.9rem;
  }
}

// Animations
@keyframes fadeInDown {
  from {
    opacity: 0;
    transform: translateY(-20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(20px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

// Responsive
@media (max-width: 1200px) {
  .dashboard {
    margin-left: 85px;
    padding: 24px;
  }

  .topSellingGrid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .dashboard {
    margin-left: 0;
    padding: 20px;
    margin-top: 60px;
  }

  .header {
    flex-direction: column;
    gap: 16px;
  }

  .title {
    font-size: 1.75rem;
  }

  .statsGrid {
    grid-template-columns: 1fr;
  }

  .chartCard {
    padding: 20px;
  }

  .chartBody {
    height: 300px;
  }

  .topSellingGrid {
    grid-template-columns: 1fr;
  }

  .topItemName {
    font-size: 0.9rem;
  }

  .topItemStats {
    font-size: 0.8rem;
  }
}

@media (max-width: 480px) {
  .dashboard {
    padding: 16px;
  }

  .title {
    font-size: 1.5rem;
  }

  .statValue {
    font-size: 1.5rem;
  }

  .refreshBtn span {
    display: none;
  }
}
</style>
