<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import api from '@/api/api'

const { t } = useI18n()
const router = useRouter()
const route = useRoute()
const isLoading = ref(false)

const searchKeyword = ref(route.query.keyword || '')
const sortBy = ref(route.query.sortBy || 'default')
const sortDir = ref(route.query.sortDir || 'default')
const sortRef = ref(null)
const sortOpen = ref(false)
const selectedSort = computed(() => {
  if (sortDir.value === 'asc') return 'Oldest First'
  else if (sortDir.value === 'desc') return 'Newest First'
  else return 'Default'
})
const categoryRef = ref(null)
const categoryOpen = ref(false)
const selectedCategory = computed(() => {
  return categories.value.find((cat) => cat.name === route.query.category)?.id || 0
})
const selectedCategoryName = computed(() => {
  return categories.value.find((cat) => cat.id === selectedCategory.value)?.name || ''
})

const categories = ref([])
const courses = ref([])

const handleSort = (by, dir) => {
  sortBy.value = by
  sortDir.value = dir
  sortOpen.value = false
  router.replace({
    query: {
      ...route.query,
      sortBy: sortBy.value !== 'default' ? sortBy.value : undefined,
      sortDir: sortDir.value !== 'default' ? sortDir.value : undefined
    }
  })
}

const handleCategoryChange = (categoryId) => {
  const category = categories.value.find((cat) => cat.id === categoryId)
  router.replace({
    query: {
      ...route.query,
      category: category ? category.name : undefined
    }
  })
  categoryOpen.value = false
}

const getAllCategories = async() => {
  const res = await api.get('/categories/all')
  categories.value = res.data
}

const getAllCourses = async() => {
  isLoading.value = true
  const res = await api.get('/enrollments/user', {
    params: {
      keyword: route.query.keyword || '',
      categoryId: selectedCategory.value || '',
      sortBy: sortBy.value !== 'default' ? sortBy.value : '',
      sortDir: sortDir.value !== 'default' ? sortDir.value : ''
    }
  })
  isLoading.value = false
  courses.value = res.data
}

const handleClickOutside = (e) => {
  if (sortRef.value && !sortRef.value.contains(e.target)) {
    sortOpen.value = false
  }
  if (categoryRef.value && !categoryRef.value.contains(e.target)) {
    categoryOpen.value = false
  }
}

const formatDate = (date) => {
  const utcDate = new Date(date)

  return utcDate.toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
    timeZone: "Asia/Ho_Chi_Minh"
  })
}

const handleLearn = async(course) => {
  const res = await api.get(`/lessons/${course.id}`)
  const lessons = res.data
  router.push(`/courses/${course.slug}/${course.id}/${lessons[0].slug}`)
}

onMounted(() => {
  getAllCategories()
  getAllCourses()
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<template>
  <div class="enrollments">
    <div class="container">
      <div class="header">
        <div class="headerContent">
          <h1 class="title">
            <i class="bi bi-journal-bookmark-fill"></i>
            My Enrollments
          </h1>
          <p class="subtitle">View and manage your enrolled courses.</p>
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
              <button :class="{'dropdownItem': true, 'dropdownItemActive': sortDir === 'default'}" @click="handleSort('default', 'default')">
                <span class="dropdownItemLeft">
                  <i class="bi bi-grid"></i>
                  Default
                </span>
                <i v-if="sortDir === 'default'" class="bi bi-check-lg"></i>
              </button>
              <button :class="{'dropdownItem': true, 'dropdownItemActive': sortDir === 'asc'}" @click="handleSort('enrolledAt', 'asc')">
                <span class="dropdownItemLeft">
                  <i class="bi bi-sort-down"></i>
                  Oldest First
                </span>
                <i v-if="sortDir === 'asc'" class="bi bi-check-lg"></i>
              </button>
              <button :class="{'dropdownItem': true, 'dropdownItemActive': sortDir === 'desc'}" @click="handleSort('enrolledAt', 'desc')">
                <span class="dropdownItemLeft">
                  <i class="bi bi-sort-up"></i>
                  Newest First
                </span>
                <i v-if="sortDir === 'desc'" class="bi bi-check-lg"></i>
              </button>
            </div>
          </div>

          <div class="dropdown" ref="categoryRef">
            <button class="filterBtn" @click="categoryOpen = !categoryOpen">
              <span>{{ selectedCategoryName || 'Category' }}</span>
              <i :class="{ 'bi bi-chevron-down': true, rotate: categoryOpen }"></i>
            </button>
            <div v-if="categoryOpen" class="dropdownMenu">
              <button :class="{'dropdownItem': true, 'dropdownItemActive': selectedCategory === 0}" @click="handleCategoryChange(0)">
                <span class="dropdownItemLeft">
                  Category
                </span>
                <i v-if="selectedCategory === 0" class="bi bi-check-lg"></i>
              </button>
              <button v-for="category in categories" :key="category.id" :class="{'dropdownItem': true, 'dropdownItemActive': selectedCategory === category.id}" @click="handleCategoryChange(category.id)">
                <span class="dropdownItemLeft">
                  {{ category.name }}
                </span>
                <i v-if="selectedCategory === category.id" class="bi bi-check-lg"></i>
              </button>
            </div>
          </div>
        </div>

        <p v-if="courses.length > 0" class="resultCount">
          Showing {{ courses.length }} courses
        </p>
      </div>

      <div class="content">
        <div v-if="isLoading" class="loading">
          <div class="spinner"></div>
          <p>Loading...</p>
        </div>
        <div v-else-if="courses.length === 0" class="empty">
          <i class="bi bi-inbox"></i>
          <h3>No Item Found!</h3>
        </div>
        <div v-else class="grid">
          <div v-for="course in courses" :key="course.id" class="card">
            <div class="cardImage">
              <img :src="course.thumbnailUrl" :alt="course.name">
            </div>

            <div class="cardBody">
              <div class="cardHeader">
                <h3 class="cardTitle">{{ course.name }}</h3>
              </div>
              <div v-if="course.categories?.length > 0" class="categoryTags">
                <span v-for="category in course.categories" :key="category.id" class="categoryTag">
                  {{ category.name }}
                </span>
              </div>
              <div class="cardFooter">
                <div class="addedDate">
                  <i class="bi bi-calendar-check"></i>
                  <span>Enrolled {{ formatDate(course.enrolledAt) }}</span>
                </div>
                <button class="viewBtn" @click="handleLearn(course)">
                  Study
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
.enrollments {
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
    color: #667eea;
    animation: floatIcon 3s ease-in-out infinite;
  }
}

@keyframes floatIcon {
  0%,
  100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-5px);
  }
}

.subtitle {
  font-size: 1.1rem;
  color: #64748b;
}
// ─── Filters ─────────────────────────────────────────────────────────────────
.filters {
  background: white;
  padding: 20px 24px;
  border-radius: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  margin-bottom: 32px;
}

.filterGroup {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
  align-items: center;
  margin-bottom: 12px;
}

.searchBox {
  position: relative;
  flex: 1;
  min-width: 240px;

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
    font-size: 0.97rem;
    font-family: inherit;
    transition: all 0.3s;
    color: #1e293b;

    &:focus {
      outline: none;
      border-color: #667eea;
      box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
    }

    &::placeholder {
      color: #94a3b8;
    }
  }
}

.resultCount {
  color: #94a3b8;
  font-size: 0.9rem;
  font-weight: 500;
  margin: 0;
}

// ─── Sort Dropdown ────────────────────────────────────────────────────────────
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

// ─── Reset button ─────────────────────────────────────────────────────────────
.resetBtn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 11px 18px;
  background: #fff5f5;
  border: 2px solid #fecaca;
  border-radius: 10px;
  font-size: 0.95rem;
  font-weight: 600;
  color: #ef4444;
  cursor: pointer;
  transition: all 0.3s;
  font-family: inherit;

  i {
    font-size: 1rem;
  }

  &:hover {
    background: #ef4444;
    border-color: #ef4444;
    color: white;
  }
}

// ─── Content / Grid ───────────────────────────────────────────────────────────
.content {
  min-height: 400px;
}

.grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 24px;
}

// ─── Card ─────────────────────────────────────────────────────────────────────
.card {
  background: white;
  border-radius: 16px;
  overflow: hidden;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  transition: all 0.3s;
  display: flex;
  flex-direction: column;

  &:hover {
    transform: translateY(-4px);
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  }
}

.cardImage {
  position: relative;
  width: 100%;
  height: 190px;
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

.cardBody {
  padding: 18px 20px 20px;
  display: flex;
  flex-direction: column;
  flex: 1;
  gap: 10px;
}

.cardHeader {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
}

.cardTitle {
  font-size: 1.08rem;
  font-weight: 700;
  color: #1e293b;
  line-height: 1.45;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  margin: 0;
  flex: 1;
}

// ─── Category Tags (on card only) ─────────────────────────────────────────────
.categoryTags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.categoryTag {
  display: inline-flex;
  align-items: center;
  padding: 3px 10px;
  background: #f0f2ff;
  color: #667eea;
  border-radius: 20px;
  font-size: 0.75rem;
  font-weight: 600;
  border: 1px solid #e0e7ff;
  white-space: nowrap;
  transition: all 0.2s;

  &:hover {
    background: #667eea;
    color: white;
    border-color: #667eea;
  }
}

// ─── Card Footer ─────────────────────────────────────────────────────────────
.cardFooter {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: auto;
  padding-top: 14px;
  border-top: 1px solid #e2e8f0;
}

.addedDate {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #64748b;
  font-size: 0.84rem;
  font-weight: 500;

  i {
    color: #10b981;
    font-size: 0.9rem;
  }
}

.viewBtn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 18px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.3s;
  font-family: inherit;

  i {
    font-size: 1rem;
    transition: transform 0.3s;
  }

  &:hover {
    transform: translateX(2px);
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);

    i {
      transform: translateX(4px);
    }
  }
}

// ─── Empty State ─────────────────────────────────────────────────────────────
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
  .searchBox {
    min-width: 100%;
  }
  .filterBtn {
    width: 100%;
    justify-content: space-between;
  }
  .dropdownMenu {
    min-width: 100%;
  }
  .resetBtn {
    width: 100%;
    justify-content: center;
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
  .enrollments {
    padding: 20px 10px 60px;
  }
  .title {
    font-size: 1.6rem;
  }
  .subtitle {
    font-size: 0.95rem;
  }
}
</style>
