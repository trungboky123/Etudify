<script setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import api from '@/api/api'

const { t, locale } = useI18n()
const route = useRoute()
const router = useRouter()
const isLoading = ref(false)

const options = [
  {
    sortBy: 'default',
    sortDir: 'default',
    label: 'Default',
    icon: 'bi bi-grid',
  },
  {
    sortBy: 'date',
    sortDir: 'desc',
    label: 'Newest First',
    icon: 'bi bi-sort-up',
  },
  {
    sortBy: 'date',
    sortDir: 'asc',
    label: 'Oldest First',
    icon: 'bi bi-sort-down',
  },
  {
    sortBy: 'price',
    sortDir: 'desc',
    label: 'Price: High to Low',
    icon: 'bi bi-sort-numeric-down',
  },
  {
    sortBy: 'price',
    sortDir: 'asc',
    label: 'Price: Low to High',
    icon: 'bi bi-sort-numeric-up',
  },
]
const categories = ref([])
const courses = ref([])

const sortRef = ref(null)
const sortOpen = ref(false)
const categoryRef = ref(null)
const categoryOpen = ref(false)

const searchKeyword = ref(route.query.keyword || '')
const sortBy = ref(route.query.sortBy || 'default')
const sortDir = ref(route.query.sortDir || 'default')
const selectedSort = computed(() => {
  if (sortBy.value === 'date' && sortDir.value === 'desc') return 'Newest First'
  if (sortBy.value === 'date' && sortDir.value === 'asc') return 'Oldest First'
  if (sortBy.value === 'price' && sortDir.value === 'desc') return 'Price: High to Low'
  if (sortBy.value === 'price' && sortDir.value === 'asc') return 'Price: Low to High'
  return 'Default'
})
const selectedCategory = computed(() => {
  return categories.value.find((cat) => cat.name === route.query.category)?.id || 0
})
const selectedCategoryName = computed(() => {
  return categories.value.find((cat) => cat.id === selectedCategory.value)?.name || ''
})

const handleCategoryChange = (categoryId) => {
  const category = categories.value.find((cat) => cat.id === categoryId)
  router.replace({
    query: {
      ...route.query,
      category: category ? category.name : undefined,
    },
  })
  categoryOpen.value = false
}

const handleSort = (by, dir) => {
  sortBy.value = by
  sortDir.value = dir
  sortOpen.value = false
  router.replace({
    query: {
      ...route.query,
      sortBy: sortBy.value !== 'default' ? sortBy.value : undefined,
      sortDir: sortDir.value !== 'default' ? sortDir.value : undefined,
    },
  })
}

const handleClickOutside = (e) => {
  if (sortRef.value && !sortRef.value.contains(e.target)) {
    sortOpen.value = false
  }
  if (categoryRef.value && !categoryRef.value.contains(e.target)) {
    categoryOpen.value = false
  }
}

const handleRemove = async (itemId) => {
  try {
    await api.delete(`/wishlists/remove?itemId=${itemId}`)
  } catch (error) {
    console.log('Error in removing from your wishlist: ' + error.response.data)
    return
  }

  getAllCourses()
}

const getAllCategories = async () => {
  const res = await api.get('/categories/all')
  categories.value = res.data
}

const getAllCourses = async () => {
  isLoading.value = true
  const res = await api.get('/wishlists/user', {
    params: {
      keyword: route.query.keyword || '',
      categoryId: selectedCategory.value || '',
      sortBy: sortBy.value !== 'default' ? sortBy.value : '',
      sortDir: sortDir.value !== 'default' ? sortDir.value : '',
    },
  })
  isLoading.value = false
  courses.value = res.data
}

const calculateDiscount = (listed, sale) => {
  if (sale === null) return 0
  const discount = ((listed - sale) / listed) * 100
  return Math.round(discount)
}

const formatPrice = (price) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(price)
}

const formatDate = (isoString) =>
  computed(() => {
    if (!isoString) return ''
    const iso = isoString.split('.')[0].replace(' ', 'T') + 'Z'
    const utcDate = new Date(iso)

    if (locale.value === 'vi') {
      return utcDate.toLocaleDateString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        timeZone: 'Asia/Ho_Chi_Minh',
      })
    } else if (locale.value === 'fr') {
      return utcDate.toLocaleDateString('fr-FR', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        timeZone: 'Asia/Ho_Chi_Minh',
      })
    } else {
      return utcDate.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        timeZone: 'Asia/Ho_Chi_Minh',
      })
    }
  })

onMounted(() => {
  getAllCategories()
  getAllCourses()
  document.addEventListener('click', handleClickOutside)
})

watch(
  () => route.query,
  () => getAllCourses(),
)

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})

let timer = null
watch(searchKeyword, (newVal) => {
  if (timer) clearTimeout(timer)

  timer = setTimeout(() => {
    router.replace({
      query: {
        ...route.query,
        keyword: newVal || undefined,
      },
    })
  }, 300)
})
</script>

<template>
  <div class="wishlist">
    <div class="container">
      <div class="header">
        <div class="headerContent">
          <h1 class="title">
            <i class="bi bi-heart-fill"></i>
            My Wishlist
          </h1>
          <p class="subtitle">Save your favorite courses for later</p>
        </div>
      </div>

      <div class="filters">
        <div class="filterGroup">
          <div class="searchBox">
            <i class="bi bi-search"></i>
            <input type="text" placeholder="Search courses..." v-model="searchKeyword" />
          </div>

          <div class="dropdown" ref="sortRef">
            <button class="filterBtn" @click="sortOpen = !sortOpen">
              <span>{{ selectedSort }}</span>
              <i :class="{ 'bi bi-chevron-down': true, rotate: sortOpen }"></i>
            </button>
            <div v-if="sortOpen" class="dropdownMenu">
              <button
                v-for="(opt, index) in options"
                :key="index"
                :class="{
                  dropdownItem: true,
                  dropdownItemActive: sortBy === opt.sortBy && sortDir === opt.sortDir,
                }"
                @click="handleSort(opt.sortBy, opt.sortDir)"
              >
                <span class="dropdownItemLeft">
                  <i :class="`${opt.icon}`"></i>
                  {{ opt.label }}
                </span>
                <i
                  v-if="sortBy === opt.sortBy && sortDir === opt.sortDir"
                  class="bi bi-check-lg"
                ></i>
              </button>
            </div>
          </div>

          <div class="dropdown" ref="categoryRef">
            <button class="filterBtn" @click="categoryOpen = !categoryOpen">
              <span>{{ selectedCategoryName || t('enrollments.category') }}</span>
              <i :class="{ 'bi bi-chevron-down': true, rotate: categoryOpen }"></i>
            </button>
            <div v-if="categoryOpen" class="dropdownMenu">
              <button
                :class="{ dropdownItem: true, dropdownItemActive: selectedCategory === 0 }"
                @click="handleCategoryChange(0)"
              >
                <span class="dropdownItemLeft">
                  {{ t('enrollments.category') }}
                </span>
                <i v-if="selectedCategory === 0" class="bi bi-check-lg"></i>
              </button>
              <button
                v-for="category in categories"
                :key="category.id"
                :class="{
                  dropdownItem: true,
                  dropdownItemActive: selectedCategory === category.id,
                }"
                @click="handleCategoryChange(category.id)"
              >
                <span class="dropdownItemLeft">
                  {{ category.name }}
                </span>
                <i v-if="selectedCategory === category.id" class="bi bi-check-lg"></i>
              </button>
            </div>
          </div>
        </div>
      </div>

      <div class="content">
        <div v-if="isLoading" class="loading">
          <div class="spinner"></div>
          <p>Loading...</p>
        </div>
        <div v-else-if="courses.length === 0" class="empty">
          <i class="bi bi-inbox"></i>
          <h3>No Items Found!</h3>
        </div>
        <div v-else class="grid">
          <div v-for="course in courses" :key="course.itemId" class="card">
            <div class="cardImage">
              <img :src="course.thumbnailUrl" :alt="course.itemName" />
              <div v-if="course.salePrice" class="discountBadge">
                {{ calculateDiscount(course.listedPrice, course.salePrice) }}% OFF
              </div>
            </div>

            <div class="cardBody">
              <div class="cardHeader">
                <h3 class="cardTitle">{{ course.itemName }}</h3>
                <button
                  class="removeBtn"
                  @click="handleRemove(course.itemId)"
                  title="Remove from wishlist"
                >
                  <i class="bi bi-x-lg"></i>
                </button>
              </div>

              <div class="priceSection">
                <div class="prices">
                  <span class="currentPrice">
                    {{ formatPrice(course.salePrice || course.listedPrice) }}
                  </span>
                  <span v-if="course.salePrice" class="originalPrice">
                    {{ formatPrice(course.listedPrice) }}
                  </span>
                </div>
              </div>

              <div class="cardFooter">
                <div class="addedDate">
                  <i class="bi bi-calendar-event"></i>
                  <span>Added {{ formatDate(course.addedAt) }}</span>
                </div>
                <button
                  class="viewBtn"
                  @click="router.push(`/public-course-details/${course.slug}/${course.itemId}`)"
                >
                  View Details
                  <i class="bi bi-arrow-right"></i>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.wishlist {
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #e9ecef 100%);
  padding: 40px 20px 80px;
  margin-top: 70px;
}

.header {
  margin-bottom: 40px;
  text-align: center;

  &Content {
    max-width: 600px;
    margin: 0 auto;
  }
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
    color: #ef4444;
    animation: heartbeat 1.5s ease-in-out infinite;
  }
}

@keyframes heartbeat {
  0%,
  100% {
    transform: scale(1);
  }
  10%,
  30% {
    transform: scale(1.1);
  }
  20% {
    transform: scale(0.95);
  }
}

.subtitle {
  font-size: 1.1rem;
  color: #64748b;
}

// Tabs
.tabs {
  display: flex;
  gap: 16px;
  margin-bottom: 32px;
  justify-content: center;
  flex-wrap: wrap;
}

.tab {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px 32px;
  background: white;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  font-size: 1.1rem;
  font-weight: 600;
  color: #64748b;
  cursor: pointer;
  transition: all 0.3s;

  i {
    font-size: 1.3rem;
  }

  &:hover {
    border-color: #667eea;
    color: #667eea;
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.2);
  }

  &Active {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border-color: #667eea;
    color: white;
    box-shadow: 0 4px 16px rgba(102, 126, 234, 0.3);

    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
    }
  }

  &Count {
    padding: 4px 10px;
    background: rgba(255, 255, 255, 0.2);
    border-radius: 20px;
    font-size: 0.9rem;
    font-weight: 700;
  }
}

// Filters
.filters {
  background: white;
  padding: 24px;
  border-radius: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  margin-bottom: 32px;
}

.filterGroup {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  margin-bottom: 16px;
}

.searchBox {
  position: relative;
  flex: 1;
  min-width: 250px;

  i {
    position: absolute;
    left: 16px;
    top: 50%;
    transform: translateY(-50%);
    color: #94a3b8;
    font-size: 1.1rem;
  }

  input {
    width: 100%;
    padding: 12px 16px 12px 45px;
    border: 2px solid #e2e8f0;
    border-radius: 10px;
    font-size: 1rem;
    transition: all 0.3s;

    &:focus {
      outline: none;
      border-color: #667eea;
      box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
    }
  }
}

.filterSelect {
  padding: 12px 16px;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  font-size: 1rem;
  color: #475569;
  background: white;
  cursor: pointer;
  transition: all 0.3s;
  min-width: 180px;

  &:focus {
    outline: none;
    border-color: #667eea;
    box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
  }

  &:hover {
    border-color: #cbd5e1;
  }
}

.resetBtn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 20px;
  background: #f8fafc;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  font-weight: 600;
  color: #64748b;
  cursor: pointer;
  transition: all 0.3s;

  &:hover {
    background: #667eea;
    border-color: #667eea;
    color: white;
  }

  i {
    font-size: 1.1rem;
  }
}

.resultCount {
  color: #64748b;
  font-size: 0.95rem;
  font-weight: 500;
}

// Content
.content {
  min-height: 400px;
}

.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 24px;
}

.card {
  background: white;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  transition: all 0.3s;

  &:hover {
    transform: translateY(-4px);
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  }

  &Image {
    position: relative;
    width: 100%;
    height: 200px;
    overflow: hidden;

    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      transition: transform 0.3s;
    }

    &:hover img {
      transform: scale(1.05);
    }
  }

  &Body {
    padding: 20px;
  }

  &Header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    gap: 12px;
    margin-bottom: 12px;
  }

  &Title {
    font-size: 1.2rem;
    font-weight: 700;
    color: #1e293b;
    line-height: 1.4;
    flex: 1;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }

  &Footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 16px;
    padding-top: 16px;
    border-top: 1px solid #e2e8f0;
  }
}

.removeBtn {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #fee2e2;
  border: none;
  border-radius: 8px;
  color: #ef4444;
  cursor: pointer;
  transition: all 0.3s;
  flex-shrink: 0;

  &:hover {
    background: #ef4444;
    color: white;
    transform: rotate(90deg);
  }

  i {
    font-size: 1rem;
    font-weight: 700;
  }
}

.discountBadge {
  position: absolute;
  top: 12px;
  right: 12px;
  padding: 6px 12px;
  background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
  color: white;
  border-radius: 8px;
  font-weight: 700;
  font-size: 0.85rem;
  box-shadow: 0 2px 8px rgba(239, 68, 68, 0.3);
}

.priceSection {
  margin-bottom: 8px;
}

.prices {
  display: flex;
  align-items: center;
  gap: 12px;
}

.currentPrice {
  font-size: 1.8rem;
  font-weight: 800;
  color: #667eea;
}

.originalPrice {
  font-size: 1rem;
  color: #94a3b8;
  text-decoration: line-through;
}

.addedDate {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #64748b;
  font-size: 0.85rem;

  i {
    color: #94a3b8;
  }
}

.viewBtn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.3s;

  &:hover {
    transform: translateX(4px);
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
  }

  i {
    font-size: 1rem;
    transition: transform 0.3s;
  }

  &:hover i {
    transform: translateX(4px);
  }
}

// Empty State
.empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 80px 20px;
  text-align: center;

  i {
    font-size: 5rem;
    color: #e2e8f0;
    margin-bottom: 24px;
  }

  h3 {
    font-size: 1.8rem;
    font-weight: 700;
    color: #1e293b;
    margin-bottom: 12px;
  }

  p {
    font-size: 1.1rem;
    color: #64748b;
    max-width: 500px;
    margin-bottom: 24px;
  }

  &Btn {
    padding: 12px 24px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border: none;
    border-radius: 10px;
    font-weight: 600;
    font-size: 1rem;
    cursor: pointer;
    transition: all 0.3s;

    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
    }
  }
}

.dropdown {
  position: relative;
}

.filterBtn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 11px 16px;
  background: white;
  border: 2px solid #e2e8f0;
  border-radius: 10px;
  font-size: 0.97rem;
  font-weight: 500;
  color: #475569;
  cursor: pointer;
  transition: all 0.3s;
  white-space: nowrap;
  font-family: inherit;

  > i:first-child {
    font-size: 0.95rem;
    color: #94a3b8;
  }

  > i.bi-chevron-down {
    font-size: 0.72rem;
    color: #94a3b8;
    transition: transform 0.3s;
    margin-left: 2px;

    &.rotate {
      transform: rotate(180deg);
    }
  }

  &:hover {
    border-color: #c7d2fe;
    color: #667eea;

    > i:first-child {
      color: #667eea;
    }
  }

  &Open {
    border-color: #667eea;
    box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
    color: #667eea;

    > i:first-child {
      color: #667eea;
    }
  }

  &Active {
    border-color: #667eea;
    background: #f0f2ff;
    color: #667eea;
    font-weight: 600;

    > i {
      color: #667eea !important;
    }
  }
}

.dropdownMenu {
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  min-width: max(100%, 180px);
  background: white;
  border: 2px solid #e2e8f0;
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  padding: 6px;
  z-index: 300;
  animation: slideDown 0.2s ease;
  max-height: 280px;
  overflow-y: auto;

  &::-webkit-scrollbar {
    width: 6px;
  }
  &::-webkit-scrollbar-track {
    background: #f1f5f9;
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

.dropdownItem {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 9px 12px;
  background: transparent;
  border: none;
  border-radius: 8px;
  font-size: 0.93rem;
  font-weight: 500;
  color: #374151;
  cursor: pointer;
  transition: all 0.2s;
  text-align: left;
  font-family: inherit;

  > i.bi-check-lg {
    font-size: 0.95rem;
    color: #667eea;
    flex-shrink: 0;
  }

  &:hover {
    background: #f8fafc;
  }

  &Active {
    background: #f0f2ff;
    color: #667eea;
    font-weight: 600;
  }
}

.dropdownItemLeft {
  display: flex;
  align-items: center;
  gap: 8px;

  i {
    font-size: 0.95rem;
    color: #667eea;
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

// Loading State
.loading {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 100px 20px;
  text-align: center;

  p {
    font-size: 1.1rem;
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

// Responsive
@media (max-width: 768px) {
  .title {
    font-size: 2rem;
  }

  .tabs {
    flex-direction: column;
  }

  .tab {
    width: 100%;
    justify-content: center;
  }

  .filterGroup {
    flex-direction: column;
  }

  .searchBox,
  .filterSelect {
    width: 100%;
  }

  .grid {
    grid-template-columns: 1fr;
  }

  .cardFooter {
    flex-direction: column;
    gap: 12px;
    align-items: flex-start;
  }

  .viewBtn {
    width: 100%;
    justify-content: center;
  }
}

@media (max-width: 480px) {
  .wishlist {
    padding: 20px 10px 60px;
  }

  .title {
    font-size: 1.6rem;
  }

  .subtitle {
    font-size: 0.95rem;
  }

  .currentPrice {
    font-size: 1.5rem;
  }
}
</style>
