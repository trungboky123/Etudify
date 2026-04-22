<script setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '@/api/api'
import DeactivateConfirmation from '@/components/DeactivateConfirmation.vue'
import SuccessfulToast from '@/components/SuccessfulToast.vue'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const router = useRouter()
const route = useRoute()
const { sidebarCollapsed } = defineProps({
  sidebarCollapsed: Boolean,
})
const courses = ref([])
const categories = ref([])
const instructors = ref([])

const categoryOpen = ref(false)
const instructorOpen = ref(false)
const statusOpen = ref(false)
const categoryRef = ref(false)
const instructorRef = ref(false)
const statusRef = ref(false)

const type = ref('')
const activation = ref(false)
const confirmOpen = ref(false)
const toastOpen = ref(false)
const selectedCourseId = ref('')

const handleClickOutside = (e) => {
  if (categoryRef.value && !categoryRef.value.contains(e.target)) {
    categoryOpen.value = false
  }
  if (instructorRef.value && !instructorRef.value.contains(e.target)) {
    instructorOpen.value = false
  }
  if (statusRef.value && !statusRef.value.contains(e.target)) {
    statusOpen.value = false
  }
}

const formatCurrency = (amount) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(amount)
}

const getStatusBadgeClass = (status) => {
  return status ? 'statusActive' : 'statusInactive'
}

const getStatus = (status) => {
  return status ? 'Active' : 'Inactive'
}

const searchKeyword = ref(route.query.keyword || '')
const selectedCategory = computed(() => {
  if (!categories.value.length) return 0
  const category = categories.value.find((c) => c.name === route.query.category)
  return category?.id || 0
})
const selectedCategoryName = computed(() => {
  const category = categories.value.find((c) => c.id === selectedCategory.value)
  return category?.name || ''
})

const selectedInstructor = computed(() => {
  if (!instructors.value.length) return 0
  const instructor = instructors.value.find((inst) => inst.fullName === route.query.instructor)
  return instructor?.id || 0
})
const selectedInstructorName = computed(() => {
  if (!instructors.value.length) return 0
  const instructor = instructors.value.find((inst) => inst.id === selectedInstructor.value)
  return instructor?.fullName || ''
})

const selectedStatus = computed(() => {
  if (route.query.status === 'Active') return 'true'
  if (route.query.status === 'Inactive') return 'false'
  return 'all'
})
const selectedStatusName = computed(() => {
  return route.query.status || ''
})

const handleCategoryChange = (categoryId) => {
  const category = categories.value.find((c) => c.id === categoryId)
  router.replace({
    query: {
      ...route.query,
      category: category ? category.name : undefined,
    },
  })
  categoryOpen.value = false
}

const handleInstructorChange = (instId) => {
  const instructor = instructors.value.find((inst) => inst.id === instId)
  router.replace({
    query: {
      ...route.query,
      instructor: instructor ? instructor.fullName : undefined,
    },
  })
  instructorOpen.value = false
}

const handleStatusChange = (status) => {
  let statusQuery

  if (status === 'true') statusQuery = 'Active'
  else if (status === 'false') statusQuery = 'Inactive'
  else statusQuery = undefined

  router.replace({
    query: {
      ...route.query,
      status: statusQuery,
    },
  })
  statusOpen.value = false
}

const getAllCourses = async () => {
  const res = await api.get('/courses/all', {
    params: {
      keyword: route.query.keyword || '',
      categoryId: selectedCategory.value || '',
      instructorId: selectedInstructor.value || '',
      status: selectedStatus.value || '',
    },
  })
  courses.value = res.data
}

const getAllCategories = async () => {
  const res = await api.get('/categories/all')
  categories.value = res.data
}

const getAllInstructors = async () => {
  const res = await api.get('/users/instructors/all')
  instructors.value = res.data
}

const handleActivate = (courseId) => {
  type.value = 'Course'
  activation.value = true
  confirmOpen.value = true
  selectedCourseId.value = courseId
}

const handleDeactivate = (courseId) => {
  type.value = 'Course'
  activation.value = false
  confirmOpen.value = true
  selectedCourseId.value = courseId
}

const handleCancel = () => {
  confirmOpen.value = false
  selectedCourseId.value = ''
}

let timer = null
const toggleStatus = async (courseId) => {
  await api.put(`/courses/status/${courseId}`)
  await getAllCourses()
  selectedCourseId.value = ''
  confirmOpen.value = false
  toastOpen.value = true

  if (timer) clearTimeout(timer)
  timer = setTimeout(() => {
    toastOpen.value = false
  }, 2500)
}

onMounted(() => {
  getAllCourses()
  getAllCategories()
  getAllInstructors()
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})

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

watch(
  () => route.query,
  () => getAllCourses(),
)
</script>

<template>
  <div
    class="courses"
    :style="{
      marginLeft: sidebarCollapsed ? '85px' : '280px',
      transition: 'margin-left 0.3s cubic-bezier(0.4,0, 0.2, 1)',
    }"
  >
    <div class="container">
      <div class="header">
        <div>
          <h1 class="title">{{ t('admin.courses.title') }}</h1>
          <p class="subtitle">{{ t('admin.courses.subtitle') }}</p>
        </div>
      </div>

      <div class="toolbar">
        <div class="filters">
          <div class="searchBox">
            <i class="bi bi-search"></i>
            <input
              type="text"
              :placeholder="t('admin.courses.search')"
              v-model="searchKeyword"
              class="searchInput"
            />
          </div>

          <div class="dropdown" ref="categoryRef">
            <button class="filterBtn" @click="categoryOpen = !categoryOpen">
              <span>{{ selectedCategoryName || t('admin.courses.category') }}</span>
              <i :class="{ 'bi bi-chevron-down': true, rotate: categoryOpen }"></i>
            </button>

            <div v-if="categoryOpen" class="filterDropdownMenu">
              <button
                :class="{ filterDropdownItem: true, active: selectedCategory === 0 }"
                @click="handleCategoryChange(0)"
              >
                {{ t('admin.courses.category') }}
                <i v-if="selectedCategory === 0" class="bi bi-check-lg"></i>
              </button>
              <button
                v-for="category in categories"
                :key="category.id"
                :class="{ filterDropdownItem: true, active: selectedCategory === category.id }"
                @click="handleCategoryChange(category.id)"
              >
                {{ category.name }}
                <i v-if="selectedCategory === category.id" class="bi bi-check-lg"></i>
              </button>
            </div>
          </div>

          <div class="dropdown" ref="instructorRef">
            <button class="filterBtn" @click="instructorOpen = !instructorOpen">
              <span>{{ selectedInstructorName || t('admin.courses.instructor') }}</span>
              <i :class="{ 'bi bi-chevron-down': true, rotate: instructorOpen }"></i>
            </button>

            <div v-if="instructorOpen" class="filterDropdownMenu">
              <button
                :class="{ filterDropdownItem: true, active: selectedInstructor === 0 }"
                @click="handleInstructorChange(0)"
              >
                {{ t('admin.courses.instructor') }}
                <i v-if="selectedInstructor === 0" class="bi bi-check-lg"></i>
              </button>
              <button
                v-for="inst in instructors"
                :key="inst.id"
                :class="{ filterDropdownItem: true, active: selectedInstructor === inst.id }"
                @click="handleInstructorChange(inst.id)"
              >
                {{ inst.fullName }}
                <i v-if="selectedInstructor === inst.id" class="bi bi-check-lg"></i>
              </button>
            </div>
          </div>

          <div class="dropdown" ref="statusRef">
            <button class="filterBtn" @click="statusOpen = !statusOpen">
              <span>{{ selectedStatusName || t('admin.courses.status') }}</span>
              <i :class="{ 'bi bi-chevron-down': true, rotate: statusOpen }"></i>
            </button>

            <div v-if="statusOpen" class="filterDropdownMenu">
              <button
                :class="{ filterDropdownItem: true, active: selectedStatus === 'all' }"
                @click="handleStatusChange('all')"
              >
                {{ t('admin.courses.status') }}
                <i v-if="selectedStatus === 'all'" class="bi bi-check-lg"></i>
              </button>
              <button
                :class="{ filterDropdownItem: true, active: selectedStatus === 'true' }"
                @click="handleStatusChange('true')"
              >
                {{ t('admin.courses.status.active') }}
                <i v-if="selectedStatus === 'true'" class="bi bi-check-lg"></i>
              </button>
              <button
                :class="{ filterDropdownItem: true, active: selectedStatus === 'false' }"
                @click="handleStatusChange('false')"
              >
                {{ t('admin.courses.status.inactive') }}
                <i v-if="selectedStatus === 'false'" class="bi bi-check-lg"></i>
              </button>
            </div>
          </div>
        </div>

        <div class="actions">
          <button class="addBtn" @click="router.push('/admin/courses/add')">
            <i class="bi bi-plus-lg"></i>
            <span>{{ t('admin.courses.add') }}</span>
          </button>
        </div>
      </div>

      <div class="resultsInfo">
        {{ t('admin.showing') }} {{ courses.length }} {{ t('admin.courses.text') }}
      </div>

      <div class="tableWrapper">
        <table class="table">
          <thead>
            <tr>
              <th>Index</th>
              <th>Thumbnail</th>
              <th>{{ t('admin.courses.name') }}</th>
              <th>{{ t('admin.courses.categories') }}</th>
              <th>{{ t('admin.courses.instructor') }}</th>
              <th>{{ t('admin.courses.listed') }}</th>
              <th>{{ t('admin.courses.sale') }}</th>
              <th>{{ t('admin.courses.status') }}</th>
              <th>{{ t('admin.courses.actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-if="courses.length === 0">
              <td colspan="9" class="emptyState">
                <i class="bi bi-inbox"></i>
                <p>{{ t('admin.courses.noCourses') }}</p>
              </td>
            </tr>
            <tr v-for="(course, index) in courses" :key="course.id">
              <td>{{ index + 1 }}</td>
              <td>
                <img :src="course.thumbnailUrl" :alt="course.name" class="thumbnail" />
              </td>
              <td class="nameCell">
                <RouterLink
                  :to="`/public-course-details/${course.slug}/${course.id}`"
                  style="text-decoration: none; color: black"
                >
                  {{ course.name }}
                </RouterLink>
              </td>
              <td>
                <div class="categoryTags">
                  <span
                    v-for="category in course.categories"
                    :key="category.id"
                    class="categoryTag"
                  >
                    {{ category.name }}
                  </span>
                </div>
              </td>
              <td>{{ course.instructor.fullName }}</td>
              <td class="priceCell">
                {{ formatCurrency(course.listedPrice) }}
              </td>
              <td class="priceCell">
                <span v-if="course.salePrice !== null" class="salePrice">
                  {{ formatCurrency(course.salePrice) }}
                </span>
                <span v-else class="noSale">-</span>
              </td>
              <td>
                <span :class="`statusBadge ${getStatusBadgeClass(course.status)}`">
                  {{ getStatus(course.status) }}
                </span>
              </td>
              <td>
                <div class="actionBtns">
                  <button
                    @click="handleEditCourse(course.id)"
                    class="actionBtn"
                    title="Edit course"
                  >
                    <i class="bi bi-pencil-fill"></i>
                  </button>
                  <button
                    v-if="course.status"
                    class="actionBtn dangerBtn"
                    @click="handleDeactivate(course.id)"
                    title="Deactivate course"
                  >
                    <i class="bi bi-x-circle-fill"></i>
                  </button>
                  <button
                    v-else
                    class="actionBtn successBtn"
                    @click="handleActivate(course.id)"
                    title="Activate course"
                  >
                    <i class="bi bi-check-circle-fill"></i>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>

  <DeactivateConfirmation
    v-if="confirmOpen"
    :id="selectedCourseId"
    @cancel="handleCancel"
    :type="type"
    :activation="activation"
    @confirm="toggleStatus"
  />
  <SuccessfulToast v-if="toastOpen" @close="toastOpen = false" />
</template>

<style scoped lang="scss">
.courses {
  min-height: 100vh;
  background: linear-gradient(135deg, #f5f7fa 0%, #f0f2f5 100%);
  padding: 40px;
  margin-top: 70px;
  font-family:
    'Plus Jakarta Sans',
    -apple-system,
    BlinkMacSystemFont,
    sans-serif;
}

.container {
  max-width: 1600px;
  margin: 0 auto;
}

// Header
.header {
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

// Message Box
.messageBox {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  padding: 16px 20px;
  border-radius: 12px;
  margin-bottom: 20px;
  animation: slideDown 0.3s ease;
  border: 1px solid;
}

.messageSuccess {
  background: #f0fdf4;
  border-color: #bbf7d0;
  color: #166534;

  i {
    color: #16a34a;
  }
}

.messageWarning {
  background: #fef3c7;
  border-color: #fbbf24;
  color: #92400e;

  i {
    color: #d97706;
  }
}

.messageError {
  background: #fef2f2;
  border-color: #fecaca;
  color: #991b1b;

  i {
    color: #dc2626;
  }
}

.messageContent {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  flex: 1;

  i {
    font-size: 1.3rem;
    flex-shrink: 0;
    margin-top: 2px;
  }
}

.messageText {
  flex: 1;
}

.messageTitle {
  font-size: 0.95rem;
  font-weight: 600;
  margin: 0 0 6px;
}

.importStats {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.85rem;
  font-weight: 500;
  margin-top: 4px;
}

.successText {
  color: #16a34a;
  font-weight: 600;
}

.failedText {
  color: #dc2626;
  font-weight: 600;
}

.closeBtn {
  background: none;
  border: none;
  color: inherit;
  cursor: pointer;
  padding: 4px;
  border-radius: 6px;
  transition: all 0.2s;
  opacity: 0.6;

  i {
    font-size: 0.9rem;
  }

  &:hover {
    opacity: 1;
    background: rgba(0, 0, 0, 0.05);
  }
}

// Errors Box
.errorsBox {
  background: #fff;
  border: 1px solid #fecaca;
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 20px;
  animation: slideDown 0.4s ease;
}

.errorsTitle {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 1rem;
  font-weight: 700;
  color: #991b1b;
  margin: 0 0 16px;

  i {
    font-size: 1.2rem;
    color: #dc2626;
  }
}

.errorsList {
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-height: 300px;
  overflow-y: auto;
  padding-right: 8px;

  &::-webkit-scrollbar {
    width: 6px;
  }

  &::-webkit-scrollbar-track {
    background: #f3f4f6;
    border-radius: 3px;
  }

  &::-webkit-scrollbar-thumb {
    background: #d1d5db;
    border-radius: 3px;

    &:hover {
      background: #9ca3af;
    }
  }
}

.errorItem {
  display: flex;
  gap: 12px;
  padding: 10px 12px;
  background: #fef2f2;
  border-radius: 8px;
  font-size: 0.85rem;
  line-height: 1.5;
}

.errorRow {
  font-weight: 700;
  color: #991b1b;
  white-space: nowrap;
  flex-shrink: 0;
}

.errorMessage {
  color: #7f1d1d;
  flex: 1;
}

// Toolbar
.toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 20px;
  margin-bottom: 24px;
  flex-wrap: wrap;
  animation: fadeInUp 0.6s ease;
}

.filters {
  display: flex;
  gap: 12px;
  flex: 1;
  flex-wrap: wrap;
}

.searchBox {
  position: relative;
  flex: 1;
  min-width: 200px;

  i {
    position: absolute;
    left: 16px;
    top: 50%;
    transform: translateY(-50%);
    color: #9ca3af;
    font-size: 1.1rem;
  }
}

.searchInput {
  width: 100%;
  padding: 12px 16px 12px 44px;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  font-size: 0.95rem;
  transition: all 0.3s;
  background: #ffffff;

  &:focus {
    outline: none;
    border-color: #667eea;
    box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
  }

  &::placeholder {
    color: #9ca3af;
  }
}

.filterSelect {
  padding: 12px 16px;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  font-size: 0.95rem;
  font-weight: 500;
  background: #ffffff;
  color: #374151;
  cursor: pointer;
  transition: all 0.3s;
  min-width: 150px;

  &:focus {
    outline: none;
    border-color: #667eea;
    box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
  }

  &:hover {
    border-color: #d1d5db;
  }
}

.actions {
  display: flex;
  gap: 12px;
}

// Dropdown
.dropdown {
  position: relative;
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
  max-height: 300px;
  overflow-y: auto;

  &::-webkit-scrollbar {
    width: 6px;
  }

  &::-webkit-scrollbar-track {
    background: #f3f4f6;
    border-radius: 3px;
  }

  &::-webkit-scrollbar-thumb {
    background: #d1d5db;
    border-radius: 3px;

    &:hover {
      background: #9ca3af;
    }
  }
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

.toolsBtn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 20px;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  font-size: 0.95rem;
  font-weight: 600;
  color: #374151;
  cursor: pointer;
  transition: all 0.3s;

  i.bi-chevron-down {
    font-size: 0.75rem;
    transition: transform 0.3s;

    &.rotate {
      transform: rotate(180deg);
    }
  }

  &:hover {
    background: #f9fafb;
    border-color: #d1d5db;
  }
}

.dropdownMenu {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  min-width: 250px;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  padding: 8px;
  z-index: 100;
  animation: slideDown 0.2s ease;
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

.dropdownItem {
  width: 100%;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 16px;
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
  }

  &:hover {
    background: #f3f4f6;
    color: #1f2937;
  }
}

.addBtn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px 24px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border: none;
  border-radius: 12px;
  font-size: 0.95rem;
  font-weight: 700;
  color: #ffffff;
  cursor: pointer;
  transition: all 0.3s;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3);

  i {
    font-size: 1rem;
  }

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
  }

  &:active {
    transform: translateY(0);
  }
}

// Results Info
.resultsInfo {
  font-size: 0.9rem;
  color: #6b7280;
  margin-bottom: 16px;
  font-weight: 500;
}

// Table
.tableWrapper {
  background: #ffffff;
  border-radius: 16px;
  overflow: auto;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  border: 1px solid #f3f4f6;
  animation: fadeInUp 0.7s ease;
}

.table {
  width: 100%;
  border-collapse: collapse;

  thead {
    background: #f9fafb;
    border-bottom: 1px solid #e5e7eb;

    th {
      padding: 16px 20px;
      text-align: left;
      font-size: 0.875rem;
      font-weight: 700;
      color: #374151;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      white-space: nowrap;
    }
  }

  tbody {
    tr {
      border-bottom: 1px solid #f3f4f6;
      transition: all 0.2s;

      &:last-child {
        border-bottom: none;
      }

      &:hover {
        background: #f9fafb;
      }

      td {
        padding: 16px 20px;
        font-size: 0.9rem;
        color: #374151;
        vertical-align: middle;
      }
    }
  }
}

.thumbnail {
  width: 100px;
  height: 60px;
  border-radius: 8px;
  object-fit: cover;
  border: 2px solid #e5e7eb;
  display: block;
}

.nameCell {
  font-weight: 600;
  color: #1f2937;
  max-width: 300px;
}

.categoryTags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  max-width: 250px;
}

.categoryTag {
  display: inline-block;
  padding: 4px 10px;
  background: #f3f4f6;
  color: #667eea;
  border-radius: 12px;
  font-size: 0.75rem;
  font-weight: 600;
  white-space: nowrap;
  border: 1px solid #e5e7eb;
}

.priceCell {
  font-weight: 600;
  color: #1f2937;
  white-space: nowrap;
}

.salePrice {
  color: #10b981;
  font-weight: 700;
}

.noSale {
  color: #9ca3af;
  font-weight: 500;
}

// Badges
.statusBadge {
  display: inline-block;
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.3px;
}

.statusActive {
  background: #d1fae5;
  color: #065f46;
}

.statusInactive {
  background: #fee2e2;
  color: #991b1b;
}

// Action Buttons
.actionBtns {
  display: flex;
  gap: 8px;
}

.actionBtn {
  width: 36px;
  height: 36px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: #f3f4f6;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  color: #667eea;

  i {
    font-size: 1rem;
  }

  &:hover {
    background: #667eea;
    color: #ffffff;
    transform: translateY(-2px);
    box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3);
  }

  &.dangerBtn {
    color: #dc2626;

    &:hover {
      background: #dc2626;
      color: #ffffff;
      box-shadow: 0 2px 8px rgba(220, 38, 38, 0.3);
    }
  }
}

// Empty State
.emptyState {
  text-align: center;
  padding: 60px 20px !important;
  color: #9ca3af;

  i {
    font-size: 3rem;
    margin-bottom: 16px;
    display: block;
  }

  p {
    font-size: 1rem;
    font-weight: 500;
    margin: 0;
  }
}

// Pagination
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 8px;
  margin-top: 24px;
  animation: fadeInUp 0.8s ease;
}

.paginationBtn {
  min-width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 0 12px;
  background: #ffffff;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 600;
  color: #374151;
  cursor: pointer;
  transition: all 0.2s;

  &:hover:not(:disabled) {
    background: #667eea;
    color: #ffffff;
    border-color: #667eea;
  }

  &.active {
    background: #667eea;
    color: #ffffff;
    border-color: #667eea;
  }

  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
}

.paginationDots {
  padding: 0 8px;
  color: #9ca3af;
  font-weight: 600;
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
  .toolbar {
    flex-direction: column;
    align-items: stretch;
  }

  .filters {
    width: 100%;
  }

  .actions {
    width: 100%;
    justify-content: flex-end;
  }
}

@media (max-width: 768px) {
  .accounts {
    padding: 20px;
  }

  .title {
    font-size: 1.75rem;
  }

  .filters {
    flex-direction: column;
  }

  .searchBox {
    min-width: 100%;
  }

  .filterBtn {
    width: 100%;
  }

  .actions {
    flex-direction: column;
  }

  .toolsBtn,
  .addBtn {
    width: 100%;
    justify-content: center;
  }

  .tableWrapper {
    overflow-x: auto;
  }

  .table {
    min-width: 800px;
  }
}

@media (max-width: 480px) {
  .courses {
    padding: 16px;
  }

  .title {
    font-size: 1.5rem;
  }

  .table {
    font-size: 0.85rem;

    thead th,
    tbody td {
      padding: 12px 16px;
    }
  }

  .thumbnail {
    width: 80px;
    height: 48px;
  }
}
</style>
