<script setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import api from '@/api/api'
import { connection } from '@/signalr/signalr'

const { t } = useI18n()
const router = useRouter()
const route = useRoute()
const courses = ref([])
const categories = ref([])
const isLoading = ref(false)
const totalCourses = ref(0)

const currentPage = ref(Number(route.query.page) || 1)
const totalPages = ref(1)
const coursesPerPage = 12
const from = computed(() => {
  return totalCourses.value === 0 ? 0 : (currentPage.value - 1) * coursesPerPage + 1
})
const to = computed(() => {
  return Math.min(currentPage.value * coursesPerPage, totalCourses.value)
})

const categoryOpen = ref(false)
const categoryRef = ref(null)
const priceOpen = ref(false)
const priceRef = ref(null)

const searchKeyword = ref(route.query.keyword || '')
const selectedCategory = computed(() => {
  if (!categories.value.length) return 0
  return categories.value.find((cat) => cat.name === route.query.category)?.id || 0
})
const selectedCategoryName = computed(() => {
  return categories.value.find((cat) => cat.id === selectedCategory.value)?.name || ''
})
const sortByPrice = ref(route.query.sortByPrice || 'default')
const sortName = computed(() => {
  if (sortByPrice.value === 'asc') return t('courses.price.low')
  else if (sortByPrice.value === 'desc') return t('courses.price.high')
  return t('courses.price.default')
})

const handleCategoryChange = (categoryId) => {
  const category = categories.value.find((cat) => cat.id === categoryId)
  router.replace({
    query: {
      ...route.query,
      category: category ? category.name : undefined,
      page: 1
    },
  })
  categoryOpen.value = false
}

const handleSortChange = (sort) => {
  sortByPrice.value = sort
  router.replace({
    query: {
      ...route.query,
      sortByPrice: sort !== 'default' ? sort : undefined,
      page: 1
    },
  })
  priceOpen.value = false
}

const handleClickOutside = (e) => {
  if (categoryRef.value && !categoryRef.value.contains(e.target)) {
    categoryOpen.value = false
  }
  if (priceRef.value && !priceRef.value.contains(e.target)) {
    priceOpen.value = false
  }
}

const handleViewDetails = (courseId) => {
  const course = courses.value.find((c) => c.id === courseId)
  router.push(`/public-course-details/${course.slug}/${courseId}`)
}

const formatPrice = (price) => {
  return price.toLocaleString('vi-VN', {
    style: 'currency',
    currency: 'VND',
  })
}

const getAllCategories = async () => {
  const res = await api.get('/categories/all')
  categories.value = res.data
}

const getAllCourses = async () => {
  isLoading.value = true

  try {
    const res = await api.get('/courses/public', {
      params: {
        page: currentPage.value,
        pageSize: coursesPerPage,
        keyword: route.query.keyword || '',
        categoryId: selectedCategory.value || '',
        sortByPrice: sortByPrice.value !== 'default' ? sortByPrice.value : '',
      },
    })
    courses.value = res.data.items
    totalPages.value = res.data.totalPages
    totalCourses.value = res.data.totalItems
  } catch (error) {
    console.log(error)
  } finally {
    isLoading.value = false
  }
}

connection.on("CourseAdded", async() => {
  await getAllCourses()
})

const paginationPages = computed(() => {
  const pages = []
  const maxVisiblePages = 5

  let startPage = Math.max(1, currentPage.value - Math.floor(maxVisiblePages / 2))
  let endPage = Math.min(totalPages.value, startPage + maxVisiblePages - 1)

  if (endPage - startPage < maxVisiblePages - 1) {
    startPage = Math.max(1, endPage - maxVisiblePages + 1)
  }

  // Previous button
  pages.push({ type: 'prev' })

  // First page + dots
  if (startPage > 1) {
    pages.push({ type: 'page', number: 1 })
    if (startPage > 2) {
      pages.push({ type: 'dots', key: 'dots1' })
    }
  }

  // Page numbers
  for (let i = startPage; i <= endPage; i++) {
    pages.push({ type: 'page', number: i })
  }

  // Last page + dots
  if (endPage < totalPages.value) {
    if (endPage < totalPages.value - 1) {
      pages.push({ type: 'dots', key: 'dots2' })
    }
    pages.push({ type: 'page', number: totalPages.value })
  }

  // Next button
  pages.push({ type: 'next' })

  return pages
})

onMounted(async() => {
  if (!route.query.page) {
    await router.replace({
      query: {
        ...route.query,
        page: 1
      }
    })
  }
  await getAllCategories()
  getAllCourses()
  document.addEventListener('click', handleClickOutside)
  try {
    await connection.start()
    console.log("Connected SignalR!")
  } catch (error) {
    console.log(error)
  }
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})

watch(
  () => route.query,
  (newQuery) => {
    currentPage.value = Number(newQuery.page) || 1
    sortByPrice.value = newQuery.sortByPrice || 'default'
    getAllCourses()
  }
)

const goToPage = (page) => {
  router.replace({
    query: { ...route.query, page },
  })
}

watch(
  () => route.query,
  () => getAllCourses()
)

let timer = null
watch(searchKeyword, (newVal) => {
  if (timer) clearTimeout(timer)

  timer = setTimeout(() => {
    currentPage.value = 1
    router.replace({
      query: {
        ...route.query,
        keyword: newVal || undefined
      }
    })
  }, 300)
})
</script>

<template>
  <div class="courses">
    <div class="courses__container">
      <div class="courses__header">
        <h1 class="courses__title">{{ t('courses.public') }}</h1>
        <p class="courses__subtitle">{{ t('courses.subtitle') }}</p>
      </div>

      <div class="courses__filters">
        <div class="filters__search">
          <i class="bi bi-search"></i>
          <input
            type="text"
            :placeholder="t('courses.search')"
            v-model="searchKeyword"
            @change="currentPage = 1"
            class="filters__search_input"
          />
        </div>
        <div class="filters__category" ref="categoryRef">
          <label>{{ t('courses.category') }}</label>
          <button class="filterBtn" @click="categoryOpen = !categoryOpen">
            <span>{{ selectedCategoryName || t('courses.category') }}</span>
            <i :class="{ 'bi bi-chevron-down': true, rotate: categoryOpen }"></i>
          </button>
          <div v-if="categoryOpen" class="filterDropdownMenu">
            <button
              :class="{ filterDropdownItem: true, active: selectedCategory === 0 }"
              @click="handleCategoryChange(0)"
            >
              {{ t('courses.category') }}
              <i v-if="selectedCategory === 0" class="bi bi-check-lg"></i>
            </button>
            <button
              v-for="cat in categories"
              :key="cat.id"
              :class="{ filterDropdownItem: true, active: selectedCategory === cat.id }"
              @click="handleCategoryChange(cat.id)"
            >
              {{ cat.name }}
              <i v-if="selectedCategory === cat.id" class="bi bi-check-lg"></i>
            </button>
          </div>
        </div>
        <div class="filters__sort" ref="priceRef">
          <label>{{ t('courses.price') }}</label>
          <button class="filterBtn" @click="priceOpen = !priceOpen">
            <span>{{ sortName }}</span>
            <i :class="{ 'bi bi-chevron-down': true, rotate: priceOpen }"></i>
          </button>
          <div v-if="priceOpen" class="filterDropdownMenu">
            <button
              :class="{ filterDropdownItem: true, active: sortByPrice === 'default' }"
              @click="handleSortChange('default')"
            >
              {{ t('courses.price.default') }}
              <i v-if="sortByPrice === 'default'" class="bi bi-check-lg"></i>
            </button>
            <button
              :class="{ filterDropdownItem: true, active: sortByPrice === 'asc' }"
              @click="handleSortChange('asc')"
            >
              {{ t('courses.price.low') }}
              <i v-if="sortByPrice === 'asc'" class="bi bi-check-lg"></i>
            </button>
            <button
              :class="{ filterDropdownItem: true, active: sortByPrice === 'desc' }"
              @click="handleSortChange('desc')"
            >
              {{ t('courses.price.high') }}
              <i v-if="sortByPrice === 'desc'" class="bi bi-check-lg"></i>
            </button>
          </div>
        </div>
      </div>

      <div class="courses__results">
        {{ t('courses.showing') }} {{ from }} {{ t('courses.to') }} {{ to }} {{ t('courses.text') }}
      </div>

      <div v-if="isLoading" class="courses__loading">
        <div class="spinner"></div>
        <p>{{ t('courses.loading') }}</p>
      </div>
      <div v-else-if="courses.length === 0" class="courses__empty">
        <i class="bi bi-inbox"></i>
        <h3>{{ t('courses.noCourse') }}</h3>
        <p>{{ t('courses.noCourse.subtitle') }}</p>
      </div>
      <div v-else class="courses__grid">
        <div v-for="course in courses" :key="course.id" class="course_card">
          <div class="card__thumbnail">
            <img :src="course.thumbnailUrl" :alt="course.name" />
            <div
              v-if="course.salePrice && course.salePrice < course.listedPrice"
              class="card__badge"
            >
              {{ Math.round((1 - course.salePrice / course.listedPrice) * 100) }} %OFF
            </div>
          </div>
          <div class="card__content">
            <h3 class="card__title">{{ course.name }}</h3>
            <div class="card__instructor">
              <i class="bi bi-person-circle"></i>
              <span>{{ course.instructor.fullName || 'Expert Instructor' }}</span>
            </div>
            <div class="card__footer">
              <div class="card__price">
                <div v-if="course.salePrice === 0">
                  <span class="card__price_original">
                    {{ formatPrice(course.listedPrice) }}
                  </span>
                  <span class="card__price_sale">FREE</span>
                </div>
                <div v-if="course.salePrice !== null && course.salePrice !== 0">
                  <span class="card__price_original">
                    {{ formatPrice(course.listedPrice) }}
                  </span>
                  <span class="card__price_sale">
                    {{ formatPrice(course.salePrice) }}
                  </span>
                </div>
                <div v-if="course.salePrice === null">
                  <span class="card__price_normal">
                    {{ formatPrice(course.listedPrice) }}
                  </span>
                </div>
              </div>

              <button class="card__button" @click="handleViewDetails(course.id)">
                {{ t('courses.details') }}
              </button>
            </div>
          </div>
        </div>
      </div>

      <div v-if="!isLoading && courses.length > 0 && totalPages > 1">
        <div class="courses__pagination">
          <div class="pagination">
            <template v-for="(item, index) in paginationPages" :key="index">
              <button
                v-if="item.type === 'prev'"
                class="pagination__button"
                :disabled="currentPage === 1"
                @click="goToPage(Math.max(1, currentPage - 1))"
              >
                <i class="bi bi-chevron-left"></i>
              </button>
              <button
                v-else-if="item.type === 'next'"
                class="pagination__button"
                :disabled="currentPage === totalPages"
                @click="goToPage(Math.min(totalPages, currentPage + 1))"
              >
                <i class="bi bi-chevron-right"></i>
              </button>

              <span v-else-if="item.type === 'dots'" class="pagination__dots">...</span>

              <button
                v-else
                :class="{'pagination__number': true, 'pagination__number_active': item.number === currentPage}"
                @click="goToPage(item.number)"
              >
                {{ item.number }}
              </button>
            </template>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
.courses {
  min-height: 100vh;
  background: #f8f9fa;
  padding: 100px 0 80px;

  &__container {
    max-width: 1400px;
    margin: 0 auto;
    padding: 0 20px;
  }

  &__header {
    text-align: center;
    margin-bottom: 50px;
  }

  &__title {
    font-size: 3rem;
    font-weight: 900;
    color: #1e293b;
    margin-bottom: 10px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    background-clip: text;
  }

  &__subtitle {
    font-size: 1.1rem;
    color: #64748b;
  }

  // Filters
  &__filters {
    display: flex;
    gap: 20px;
    margin-bottom: 30px;
    flex-wrap: wrap;
    align-items: flex-end;
  }

  .filters__search {
    flex: 2;
    min-width: 300px;
    position: relative;

    i {
      position: absolute;
      left: 16px;
      top: 50%;
      transform: translateY(-50%);
      color: #64748b;
      font-size: 1.1rem;
    }

    &_input {
      width: 100%;
      height: 50px;
      padding: 0 20px 0 50px;
      border: 2px solid #e2e8f0;
      border-radius: 12px;
      font-size: 0.95rem;
      transition: all 0.3s;
      outline: none;

      &:focus {
        border-color: #667eea;
        box-shadow: 0 0 0 4px rgba(102, 126, 234, 0.1);
      }

      &::placeholder {
        color: #94a3b8;
      }
    }
  }

  .filters__category,
  .filters__sort {
    position: relative;

    label {
      display: block;
      font-weight: 600;
      color: #334155;
      margin-bottom: 8px;
      font-size: 0.9rem;
    }
  }

  // Results Count
  &__results {
    color: #64748b;
    font-size: 0.9rem;
    margin-bottom: 20px;
  }

  // Loading & Empty States
  &__loading {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 80px 20px;
    color: #64748b;

    .spinner {
      width: 50px;
      height: 50px;
      border: 4px solid #e2e8f0;
      border-top-color: #667eea;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin-bottom: 20px;
    }
  }

  &__empty {
    text-align: center;
    padding: 80px 20px;
    color: #64748b;

    i {
      font-size: 4rem;
      margin-bottom: 20px;
      opacity: 0.5;
    }

    h3 {
      font-size: 1.5rem;
      color: #334155;
      margin-bottom: 10px;
    }
  }

  // Courses Grid
  &__grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
    gap: 30px;
    margin-bottom: 50px;
  }

  // Course Card with Animation
  .course_card {
    background: white;
    border-radius: 16px;
    overflow: hidden;
    transition: all 0.3s ease;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
    cursor: pointer;

    // Chỉ animate opacity
    opacity: 0;
    animation: fadeIn 0.6s ease forwards;

    @for $i from 1 through 24 {
      &:nth-child(#{$i}) {
        animation-delay: #{$i * 0.05}s;
      }
    }

    &:hover {
      transform: translateY(-8px);
      box-shadow: 0 12px 30px rgba(102, 126, 234, 0.2);

      .card__thumbnail img {
        transform: scale(1.05);
      }
    }
  }

  // Keyframe mới - chỉ fade in
  @keyframes fadeIn {
    to {
      opacity: 1;
    }
  }

  .card__thumbnail {
    position: relative;
    width: 100%;
    height: 200px;
    overflow: hidden;
    background: #e2e8f0;

    img {
      width: 100%;
      height: 100%;
      object-fit: cover;
      transition: transform 0.3s;
    }
  }

  .card__badge {
    position: absolute;
    top: 12px;
    right: 12px;
    background: linear-gradient(135deg, #f59e0b 0%, #ef4444 100%);
    color: white;
    padding: 6px 12px;
    border-radius: 20px;
    font-weight: 700;
    font-size: 0.75rem;
    box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
  }

  .card__content {
    padding: 20px;
  }

  .card__title {
    font-size: 1.1rem;
    font-weight: 700;
    color: #1e293b;
    margin-bottom: 12px;
    line-height: 1.4;
    display: -webkit-box;
    -webkit-line-clamp: 2;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }

  .card__instructor {
    display: flex;
    align-items: center;
    gap: 8px;
    color: #64748b;
    font-size: 0.9rem;
    margin-bottom: 16px;

    i {
      font-size: 1.2rem;
    }
  }

  .card__footer {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding-top: 16px;
    border-top: 1px solid #e2e8f0;
  }

  .card__price {
    display: flex;
    flex-direction: column;
    gap: 4px;

    &_original {
      text-decoration: line-through;
      color: #94a3b8;
      font-size: 0.85rem;
    }

    &_sale {
      color: #667eea;
      font-weight: 800;
      font-size: 1.2rem;
      margin-left: 10px;
    }

    &_normal {
      color: #1e293b;
      font-weight: 800;
      font-size: 1.2rem;
    }
  }

  .card__button {
    padding: 10px 20px;
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    border: none;
    border-radius: 8px;
    font-weight: 600;
    font-size: 0.85rem;
    cursor: pointer;
    transition: all 0.3s;
    white-space: nowrap;

    &:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 20px rgba(102, 126, 234, 0.4);
    }

    &:active {
      transform: translateY(0);
    }
  }

  // Pagination
  &__pagination {
    display: flex;
    justify-content: center;
    margin-top: 50px;
  }

  .pagination {
    display: flex;
    gap: 8px;
    align-items: center;

    &__button,
    &__number {
      min-width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      border: 2px solid #e2e8f0;
      background: white;
      border-radius: 8px;
      cursor: pointer;
      transition: all 0.3s;
      font-weight: 600;
      color: #64748b;

      &:hover:not(:disabled) {
        border-color: #667eea;
        color: #667eea;
        background: #f0f2ff;
      }

      &:disabled {
        opacity: 0.4;
        cursor: not-allowed;
      }
    }

    &__number_active {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border-color: transparent;

      &:hover {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
      }
    }

    &__dots {
      padding: 0 8px;
      color: #94a3b8;
    }
  }

  // Keyframe Animations
  @keyframes slideUp {
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
  @media (max-width: 1024px) {
    &__grid {
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 24px;
    }
  }

  @media (max-width: 768px) {
    padding: 80px 0 60px;

    &__title {
      font-size: 2.2rem;
    }

    &__filters {
      flex-direction: column;
      gap: 16px;
    }

    .filters__search,
    .filters__category,
    .filters__sort {
      min-width: 100%;
    }

    &__grid {
      grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
      gap: 20px;
    }

    .pagination {
      gap: 6px;

      &__button,
      &__number {
        min-width: 36px;
        height: 36px;
        font-size: 0.9rem;
      }
    }
  }

  @media (max-width: 576px) {
    &__grid {
      grid-template-columns: 1fr;
    }

    .card__footer {
      flex-direction: column;
      gap: 12px;
      align-items: flex-start;
    }

    .card__button {
      width: 100%;
    }
  }
}

.filterBtn {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  min-width: 150px;
  padding: 12px 16px;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  font-size: 0.95rem;
  font-weight: 500;
  color: #374151;
  cursor: pointer;
  transition: all 0.3s;
  white-space: nowrap;

  i {
    font-size: 0.75rem;
    color: #9ca3af;
    transition: transform 0.3s;

    &.rotate {
      transform: rotate(180deg);
    }
  }

  &:hover {
    border-color: #d1d5db;
    background: #f9fafb;
  }
}

.filterDropdownMenu {
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  min-width: 100%;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  padding: 8px;
  z-index: 100;
  animation: slideDown 0.2s ease;
}

.filterDropdownItem {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 14px;
  background: transparent;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 500;
  color: #374151;
  cursor: pointer;
  transition: all 0.2s;
  text-align: left;

  i {
    font-size: 1rem;
    color: #667eea;
    flex-shrink: 0;
  }

  &:hover {
    background: #f3f4f6;
    color: #1f2937;
  }

  &.active {
    background: #f0f2ff;
    color: #667eea;
    font-weight: 600;
  }
}
</style>
