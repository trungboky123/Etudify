<script setup>
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import api from '@/api/api'
import { QuillEditor } from '@vueup/vue-quill'

const { t, locale } = useI18n()
const router = useRouter()
const { sidebarCollapsed } = defineProps({
  sidebarCollapsed: Boolean,
})
const course = ref({
  name: '',
  description: '',
  duration: '',
  instructorId: '',
  status: true,
  listedPrice: '',
  salePrice: '',
  categoryIds: [],
})
const categories = ref([])
const previewThumbnail = ref('')
const thumbnailFile = ref(null)
const fileInputRef = ref(null)
const instructors = ref([])
const message = ref('')
const isError = ref(false)
const isSaving = ref(false)

const instructorOpen = ref(false)
const instructorRef = ref(null)

const modules = {
  toolbar: [
    [{ header: [1, 2, 3, false] }],
    ['bold', 'italic', 'underline', 'strike'],
    [{ list: 'ordered' }, { list: 'bullet' }],
    [{ color: [] }, { background: [] }],
    ['link'],
    ['clean'],
  ],
}

const handleClickOutside = (e) => {
  if (instructorRef.value && !instructorRef.value.contains(e.target)) {
    instructorOpen.value = false
  }
}

const handleThumbnailClick = () => {
  fileInputRef.value.click()
}

const handleThumbnailChange = (e) => {
  const file = e.target.files[0]
  if (file) {
    thumbnailFile.value = file
    previewThumbnail.value = URL.createObjectURL(file)
  }
}

const handleRemoveThumbnail = () => {
  previewThumbnail.value = ''
  thumbnailFile.value = null
  fileInputRef.value = null
}

const calculateDiscount = computed(() => {
  if (!course.value.salePrice || !course.value.listedPrice) return 0
  const discount =
    ((course.value.listedPrice - course.value.salePrice) / course.value.listedPrice) * 100
  return Math.round(discount)
})

const formatPrice = (price) => {
  if (!price) return ''
  return new Intl.NumberFormat('vi-VN').format(price)
}

const handleInstructorChange = (instId) => {
  course.value.instructorId = instId
  instructorOpen.value = false
}

const instructorName = computed(() => {
  return instructors.value.find((inst) => inst.id === course.value.instructorId)?.fullName || ''
})

const getAllCategories = async () => {
  const res = await api.get('/categories/all')
  categories.value = res.data
}

const getAllInstructors = async () => {
  const res = await api.get('/users/instructors/all')
  instructors.value = res.data
}

const handleSubmit = async () => {
  const formData = new FormData()

  formData.append('name', course.value.name)
  formData.append('listedPrice', course.value.listedPrice)
  formData.append('salePrice', course.value.salePrice ?? '')
  formData.append('instructorId', course.value.instructorId)
  formData.append('duration', Number(course.value.duration))
  formData.append('description', course.value.description || '')
  formData.append('status', course.value.status)

  course.value.categoryIds.forEach((id) => {
    formData.append('categoryIds', id)
  })

  if (thumbnailFile.value) {
    formData.append('thumbnail', thumbnailFile.value)
  }

  const lang = localStorage.getItem('lang') || 'en'
  isSaving.value = true
  let res = null
  try {
    res = await api.post('/courses/add', formData, {
      headers: {
        'Accept-Language': lang,
      },
    })
  } catch (error) {
    message.value = error.response?.data?.message
    isError.value = true
    return
  } finally {
    isSaving.value = false
  }

  isError.value = false
  message.value = res.data?.message?.value
  setTimeout(() => {
    router.push('/admin/courses')
  }, 1800)
}

onMounted(() => {
  getAllCategories()
  getAllInstructors()
  document.addEventListener('click', handleClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})
</script>

<template>
  <div class="layout">
    <div :class="{ main: true, mainCollapsed: sidebarCollapsed }">
      <div class="wrapper">
        <div class="breadcrumb">
          <span class="breadcrumbLink" @click="router.push('/admin/courses')">
            <i class="bi bi-book-fill"></i>
            {{ t('admin.addCourse.courses') }}
          </span>
          <i class="bi bi-chevron-right"></i>
          <span class="breadcrumbCurrent">{{ t('admin.addCourse.title') }}</span>
        </div>

        <div class="pageHeader">
          <div>
            <h1 class="pageTitle">{{ t('admin.addCourse.title') }}</h1>
            <p class="pageSubtitle">{{ t('admin.addCourse.subtitle') }}</p>
          </div>
          <button class="backBtn" @click="router.push('/admin/courses')">
            <i class="bi bi-arrow-left"></i>
            {{ t('admin.addCourse.back') }}
          </button>
        </div>

        <form @submit.prevent="handleSubmit">
          <div class="formGrid">
            <div class="leftColumn">
              <div class="card">
                <h3 class="cardTitle">
                  <i class="bi bi-image"></i>
                  {{ t('admin.addCourse.thumbnail') }}
                </h3>
                <div class="thumbnailWrapper">
                  <div class="thumbnailPreview" @click="handleThumbnailClick">
                    <img
                      v-if="previewThumbnail"
                      :src="previewThumbnail"
                      alt="Thumbnail"
                      class="thumbnailImg"
                    />
                    <div v-else class="thumbnailPlaceholder">
                      <i class="bi bi0image"></i>
                      <span>{{ t('admin.addCourse.noImage') }}</span>
                    </div>
                    <div class="thumbnailOverlay">
                      <i class="bi bi-camera"></i>
                      <span>{{ t('admin.addCourse.changeImage') }}</span>
                    </div>
                  </div>
                  <input
                    type="file"
                    ref="fileInputRef"
                    accept="image/*"
                    @change="handleThumbnailChange"
                    class="fileInput"
                  />
                  <div class="thumbnailActions">
                    <button
                      v-if="previewThumbnail"
                      type="button"
                      class="removeBtn"
                      @click="handleRemoveThumbnail"
                    >
                      <i class="bi bi-trash"></i>
                      {{ t('admin.addCourse.remove') }}
                    </button>
                  </div>
                  <p class="thumbnailNote">{{ t('admin.addCourse.recommended') }}</p>
                </div>
              </div>
              <div class="card">
                <h3 class="cardTitle">
                  <i class="bi bi-tag-fill"></i>
                  {{ t('admin.addCourse.pricing') }}
                </h3>
                <div class="formGroup">
                  <label for="listedPrice" class="label">{{ t('admin.addCourse.listed') }} (VND)</label>
                  <div class="inputWrapper">
                    <i class="bi bi-cash"></i>
                    <input
                      type="number"
                      id="listedPrice"
                      v-model.number="course.listedPrice"
                      :placeholder="t('admin.addCourse.listed.placeholder')"
                      class="input"
                    />
                  </div>
                </div>
                <div class="formGroup">
                  <label for="salePrice" class="label">{{ t('admin.addCourse.sale') }} (VND)</label>
                  <div class="inputWrapper">
                    <i class="bi bi-cash"></i>
                    <input
                      type="number"
                      id="salePrice"
                      v-model.number="course.salePrice"
                      :placeholder="t('admin.addCourse.sale.placeholder')"
                      class="input"
                    />
                  </div>
                </div>
                <div v-if="course.listedPrice" class="discountPreview">
                  <div class="discountInfo">
                    <span class="discountLabel">{{ t('admin.addCourse.discount') }}:</span>
                    <span class="discountValue"> {{ calculateDiscount }}% OFF </span>
                  </div>
                  <div class="pricePreview">
                    <span class="originalPrice">
                      {{ formatPrice(course.listedPrice) }}
                    </span>
                    <span class="salePrice">
                      {{ formatPrice(course.salePrice) }}
                    </span>
                  </div>
                </div>
              </div>
              <div class="card">
                <h3 class="cardTitle">
                  <i class="bi bi-toggle-on"></i>
                  {{ t('admin.addCourse.status') }}
                </h3>
                <div class="radioGroup">
                  <label :class="{ radioItem: true, radioActive: course.status }">
                    <input
                      type="radio"
                      name="status"
                      v-model="course.status"
                      :value="true"
                      class="radioInput"
                    />
                    <div class="radioBox">
                      <div class="radioCircle"></div>
                      <div>
                        <span class="radioLabel">{{ t('admin.addCourse.active') }}</span>
                        <span class="radioDesc">{{ t('admin.addCourse.active.desc') }}</span>
                      </div>
                    </div>
                  </label>
                  <label :class="{ radioItem: true, radioActive: !course.status }">
                    <input
                      type="radio"
                      name="status"
                      v-model="course.status"
                      :value="false"
                      class="radioInput"
                    />
                    <div class="radioBox">
                      <div class="radioCircle"></div>
                      <div>
                        <span class="radioLabel">{{ t('admin.addCourse.inactive') }}</span>
                        <span class="radioDesc">{{ t('admin.addCourse.inactive.desc') }}</span>
                      </div>
                    </div>
                  </label>
                </div>
              </div>
            </div>

            <div class="rightColumn">
              <div class="card">
                <h3 class="cardTitle">
                  <i class="bi bi-info-circle-fill"></i>
                  {{ t('admin.addCourse.information') }}
                </h3>
                <div class="formGroup">
                  <label for="courseName" class="label">{{ t('admin.addCourse.name') }}</label>
                  <div class="inputWrapper">
                    <i class="bi bi-book"></i>
                    <input
                      type="text"
                      id="courseName"
                      v-model="course.name"
                      :placeholder="t('admin.addCourse.name.placeholder')"
                      class="input"
                    />
                  </div>
                </div>
                <div class="formGroup">
                  <label class="label">{{ t('admin.addCourse.categories') }}</label>
                  <div class="categoriesGrid">
                    <label
                      v-for="category in categories"
                      :key="category.id"
                      :class="[
                        'categoryItem',
                        course.categoryIds.includes(Number(category.id)) ? 'categorySelected' : '',
                      ]"
                    >
                      <input
                        type="checkbox"
                        v-model="course.categoryIds"
                        :value="Number(category.id)"
                      />
                      <span>{{ category.name }}</span>
                    </label>
                  </div>
                  <p v-if="course.categoryIds.length === 0" class="categoryHint">
                    <i class="bi bi-info-circle"></i>
                    {{ t('admin.addCourse.categories.note') }}
                  </p>
                </div>
                <div class="formRow">
                  <div class="formGroup">
                    <label class="label">{{ t('admin.addCourse.instructor') }}</label>
                    <div class="selectWrapper" ref="instructorRef">
                      <button
                        type="button"
                        class="filterBtn"
                        @click="instructorOpen = !instructorOpen"
                      >
                        <span>{{ instructorName || t('admin.addCourse.instructor.placeholder') }}</span>
                        <i :class="{ 'bi bi-chevron-down': true, rotate: instructorOpen }"></i>
                      </button>
                      <div v-if="instructorOpen" class="filterDropdownMenu">
                        <button
                          v-for="inst in instructors"
                          :key="inst.id"
                          type="button"
                          :class="{
                            filterDropdownItem: true,
                            active: inst.id === course.instructorId,
                          }"
                          @click="handleInstructorChange(inst.id)"
                        >
                          {{ inst.fullName }}
                          <i v-if="course.instructorId === inst.id" class="bi bi-check-lg"></i>
                        </button>
                      </div>
                    </div>
                  </div>
                  <div class="formGroup">
                    <label for="duration" class="label">{{ t('admin.addCourse.duration') }}</label>
                    <div class="inputWrapper">
                      <i class="bi bi-clock"></i>
                      <input
                        type="text"
                        id="duration"
                        v-model="course.duration"
                        :placeholder="t('admin.addCourse.duration.placeholder')"
                        class="input"
                      />
                    </div>
                  </div>
                </div>

                <div class="formGroup">
                  <label for="description">{{ t('admin.addCourse.description') }}</label>
                  <div class="editorWrapper">
                    <QuillEditor
                      :key="locale"
                      theme="snow"
                      v-model:content="course.description"
                      content-type="html"
                      :toolbar="modules.toolbar"
                      :placeholder="t('admin.addCourse.description.placeholder')"
                      class="editor"
                    />
                  </div>
                </div>

                <p v-if="message" :class="['message', isError ? 'messageError' : 'messageSuccess']">
                  <i :class="isError ? 'bi bi-exclamation-circle' : 'bi bi-check-circle'"></i>
                  {{ message }}
                </p>

                <div class="actions">
                  <button type="button" class="cancelBtn" @click="router.push('/admin/courses')">
                    <i class="bi bi-x-lg"></i>
                    {{ t('admin.addCourse.cancel') }}
                  </button>
                  <button type="submit" class="saveBtn"
                  :disabled="isSaving">
                    <span v-if="isSaving" class="btnLoading">
                      <span class="spinner"></span>
                      {{ t('admin.addCourse.saving') }}
                    </span>
                    <span v-else>
                      <i class="bi bi-floppy"></i>
                      {{ t('admin.addCourse.save') }}
                    </span>
                  </button>
                </div>
              </div>
            </div>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped lang="scss">
// Layout
.layout {
  display: flex;
  min-height: 100vh;
  font-family:
    'Plus Jakarta Sans',
    -apple-system,
    BlinkMacSystemFont,
    sans-serif;
  background: #f1f3f8;
}

.main {
  flex: 1;
  margin-left: 280px;
  transition: margin-left 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  margin-top: 50px;

  &Collapsed {
    margin-left: 85px;
  }
}

.wrapper {
  padding: 32px 36px;
  animation: fadeIn 0.4s ease;
}

// Breadcrumb
.breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.88rem;
  color: #94a3b8;
  margin-bottom: 20px;

  i {
    font-size: 0.7rem;
    color: #cbd5e1;
  }
}

.breadcrumbLink {
  display: flex;
  align-items: center;
  gap: 6px;
  color: #667eea;
  cursor: pointer;
  font-weight: 600;
  transition: color 0.2s;

  i {
    font-size: 0.85rem;
    color: #667eea;
  }

  &:hover {
    color: #5568d3;
  }
}

.breadcrumbCurrent {
  color: #64748b;
  font-weight: 600;
}

// Page Header
.pageHeader {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 28px;
}

.pageTitle {
  font-size: 1.75rem;
  font-weight: 800;
  color: #1e293b;
  margin: 0 0 4px;
  line-height: 1.2;
}

.pageSubtitle {
  color: #64748b;
  font-size: 0.92rem;
  margin: 0;
}

.backBtn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  background: #fff;
  color: #475569;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  font-weight: 600;
  font-size: 0.9rem;
  cursor: pointer;
  transition: all 0.25s;

  i {
    font-size: 1rem;
  }

  &:hover {
    border-color: #667eea;
    color: #667eea;
    background: #f5f3ff;
  }
}

// Form Grid
.formGrid {
  display: grid;
  grid-template-columns: 380px 1fr;
  gap: 28px;
  align-items: start;
}

.leftColumn,
.rightColumn {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

// Cards
.card {
  background: #fff;
  border-radius: 18px;
  padding: 28px;
  box-shadow:
    0 1px 3px rgba(0, 0, 0, 0.06),
    0 4px 16px rgba(0, 0, 0, 0.04);
  border: 1px solid #eef2f7;
}

.cardTitle {
  display: flex;
  align-items: center;
  gap: 10px;
  font-size: 1rem;
  font-weight: 700;
  color: #1e293b;
  margin: 0 0 20px;

  i {
    font-size: 1.1rem;
    color: #667eea;
  }
}

// Thumbnail
.thumbnailWrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
}

.thumbnailPreview {
  position: relative;
  width: 100%;
  aspect-ratio: 16 / 9;
  border-radius: 12px;
  cursor: pointer;
  margin-bottom: 18px;
  overflow: hidden;
  border: 3px solid #eef2f7;
  transition: border-color 0.3s;

  &:hover {
    border-color: #667eea;
  }

  &:hover .thumbnailOverlay {
    opacity: 1;
  }
}

.thumbnailImg {
  width: 100%;
  height: 100%;
  object-fit: cover;
  display: block;
}

.thumbnailPlaceholder {
  width: 100%;
  height: 100%;
  background: linear-gradient(135deg, #f1f5f9 0%, #e2e8f0 100%);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 8px;

  i {
    font-size: 3rem;
    color: #cbd5e1;
  }

  span {
    font-size: 0.9rem;
    color: #94a3b8;
    font-weight: 600;
  }
}

.thumbnailOverlay {
  position: absolute;
  inset: 0;
  background: rgba(30, 41, 59, 0.6);
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 6px;
  opacity: 0;
  transition: opacity 0.25s;

  i {
    font-size: 1.8rem;
    color: #fff;
  }

  span {
    font-size: 0.85rem;
    color: #fff;
    font-weight: 600;
  }
}

.fileInput {
  display: none;
}

.thumbnailActions {
  justify-content: center;
  display: flex;
  gap: 10px;
  margin-bottom: 12px;
  width: 100%;
}

.uploadBtn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 10px 18px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  border: none;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.25s;

  i {
    font-size: 0.95rem;
  }

  &:hover {
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.35);
  }
}

.removeBtn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 10px 16px;
  background: #fff5f5;
  color: #ef4444;
  border: 1.5px solid #fecaca;
  border-radius: 8px;
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.25s;

  i {
    font-size: 0.9rem;
  }

  &:hover {
    background: #fee2e2;
    border-color: #f87171;
  }
}

.thumbnailNote {
  font-size: 0.78rem;
  color: #94a3b8;
  text-align: center;
  margin: 0;
}

// Form Groups
.formGroup {
  margin-bottom: 20px;
}

.formRow {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
}

.label {
  display: block;
  font-size: 0.88rem;
  font-weight: 600;
  color: #374151;
  margin-bottom: 8px;
}

// Input
.inputWrapper {
  position: relative;

  i {
    position: absolute;
    left: 14px;
    top: 50%;
    transform: translateY(-50%);
    color: #94a3b8;
    font-size: 1rem;
    transition: color 0.25s;
  }

  &:focus-within i {
    color: #667eea;
  }
}

.input {
  width: 100%;
  height: 46px;
  padding: 0 16px 0 42px;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  font-size: 0.93rem;
  color: #1e293b;
  background: #fafbfc;
  outline: none;
  transition: all 0.25s;
  font-family: inherit;
  box-sizing: border-box;

  &:focus {
    border-color: #667eea;
    background: #fff;
    box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.12);
  }

  &::placeholder {
    color: #cbd5e1;
  }
}

// Select / Dropdown
.selectWrapper {
  position: relative;
}

.filterBtn {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  width: 100%;
  height: 46px;
  padding: 0 16px;
  background: #fafbfc;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  font-size: 0.93rem;
  font-weight: 500;
  color: #1e293b;
  cursor: pointer;
  transition: all 0.25s;
  text-align: left;

  i {
    font-size: 0.75rem;
    color: #94a3b8;
    transition: transform 0.3s;

    &.rotate {
      transform: rotate(180deg);
    }
  }

  &:hover {
    border-color: #cbd5e1;
    background: #fff;
  }

  &:focus {
    border-color: #667eea;
    background: #fff;
    box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.12);
  }
}

.filterDropdownMenu {
  position: absolute;
  top: calc(100% + 8px);
  left: 0;
  width: 100%;
  background: #ffffff;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
  padding: 6px;
  z-index: 100;
  animation: slideDown 0.2s ease;
  max-height: 240px;
  overflow-y: auto;

  &::-webkit-scrollbar {
    width: 6px;
  }

  &::-webkit-scrollbar-track {
    background: #f3f4f6;
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

.filterDropdownItem {
  width: 100%;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 10px 12px;
  background: transparent;
  border: none;
  border-radius: 8px;
  font-size: 0.9rem;
  font-weight: 500;
  color: #1e293b;
  cursor: pointer;
  transition: all 0.2s;
  text-align: left;

  i {
    font-size: 1rem;
    color: #667eea;
    flex-shrink: 0;
  }

  &:hover {
    background: #f8fafc;
    color: #667eea;
  }

  &.active {
    background: #f0f2ff;
    color: #667eea;
    font-weight: 600;
  }
}

// Categories Grid
.categoriesGrid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
  gap: 10px;
}

.categoryItem {
  position: relative;
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 14px;
  border: 1.5px solid #e2e8f0;
  border-radius: 8px;
  background: #fafbfc;
  cursor: pointer;
  transition: all 0.25s;
  user-select: none;

  &:hover {
    border-color: #c7d2fe;
    background: #f8f9ff;
  }

  &.categorySelected {
    border-color: #667eea;
    background: #f0f2ff;

    .categoryName {
      color: #667eea;
      font-weight: 600;
    }

    i {
      color: #667eea;
      font-size: 1rem;
      margin-left: auto;
    }
  }
}

.categoryCheckbox {
  display: none;
}

.categoryName {
  font-size: 0.85rem;
  color: #475569;
  transition: all 0.25s;
  flex: 1;
}

.categoryHint {
  display: flex;
  align-items: center;
  gap: 6px;
  margin: 8px 0 0;
  font-size: 0.8rem;
  color: #94a3b8;

  i {
    font-size: 0.9rem;
  }
}

// Radio Group
.radioGroup {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.radioItem {
  display: flex;
  cursor: pointer;
  padding: 12px 14px;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  background: #fafbfc;
  transition: all 0.25s;

  &:hover {
    border-color: #c7d2fe;
    background: #f8f9ff;
  }

  &.radioActive {
    border-color: #667eea;
    background: #f0f2ff;
  }
}

.radioInput {
  display: none;
}

.radioBox {
  display: flex;
  align-items: center;
  gap: 12px;
  width: 100%;
}

.radioCircle {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  border: 2px solid #cbd5e1;
  flex-shrink: 0;
  position: relative;
  transition: all 0.25s;

  .radioActive & {
    border-color: #667eea;

    &::after {
      content: '';
      position: absolute;
      top: 3px;
      left: 3px;
      right: 3px;
      bottom: 3px;
      border-radius: 50%;
      background: linear-gradient(135deg, #667eea, #764ba2);
    }
  }
}

.radioLabel {
  display: block;
  font-size: 0.9rem;
  font-weight: 600;
  color: #1e293b;
}

.radioDesc {
  display: block;
  font-size: 0.78rem;
  color: #94a3b8;
  margin-top: 1px;
}

// Discount Preview
.discountPreview {
  padding: 16px;
  background: linear-gradient(135deg, #f0fdf4 0%, #dcfce7 100%);
  border: 1px solid #bbf7d0;
  border-radius: 12px;
  margin-top: 12px;
}

.discountInfo {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}

.discountLabel {
  font-size: 0.85rem;
  color: #15803d;
  font-weight: 600;
}

.discountValue {
  padding: 4px 12px;
  background: #16a34a;
  color: #fff;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 700;
}

.pricePreview {
  display: flex;
  align-items: center;
  gap: 12px;
}

.originalPrice {
  font-size: 0.9rem;
  color: #94a3b8;
  text-decoration: line-through;
}

.salePrice {
  font-size: 1.3rem;
  font-weight: 800;
  color: #16a34a;
}

// Rich Text Editor
.editorWrapper {
  :deep(.ql-toolbar) {
    border: 1.5px solid #e2e8f0;
    border-radius: 10px 10px 0 0;
    background: #fafbfc;
  }
  :deep(.ql-container) {
    border: 1.5px solid #e2e8f0;
    border-top: none;
    border-radius: 0 0 10px 10px;
    background: #fff;
    font-family:
      'Plus Jakarta Sans',
      -apple-system,
      BlinkMacSystemFont,
      sans-serif;
    font-size: 0.93rem;
    min-height: 200px;
  }
  :deep(.ql-editor) {
    min-height: 200px;
    padding: 16px;

    &.ql-blank::before {
      color: #cbd5e1;
      font-style: normal;
    }
  }
  :deep(.quill:focus-within .ql-toolbar) {
    border-color: #667eea;
  }
  :deep(.quill:focus-within .ql-container) {
    border-color: #667eea;
  }
}

.editor {
  min-height: 200px;
}

// Message
.message {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 16px;
  border-radius: 10px;
  font-size: 0.88rem;
  font-weight: 600;
  margin-bottom: 20px;
  animation: slideDown 0.3s ease;

  i {
    font-size: 1.1rem;
    flex-shrink: 0;
  }
}

.messageError {
  background: #fef2f2;
  color: #dc2626;
  border: 1px solid #fecaca;
}

.messageSuccess {
  background: #f0fdf4;
  color: #16a34a;
  border: 1px solid #bbf7d0;
}

// Actions
.actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}

.cancelBtn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 11px 22px;
  background: #fff;
  color: #64748b;
  border: 1.5px solid #e2e8f0;
  border-radius: 10px;
  font-size: 0.9rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.25s;

  i {
    font-size: 0.9rem;
  }

  &:hover {
    border-color: #cbd5e1;
    color: #475569;
    background: #f8fafc;
  }
}

.saveBtn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 11px 28px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  border: none;
  border-radius: 10px;
  font-size: 0.9rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.25s;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.3);

  i {
    font-size: 1rem;
  }

  &:hover:not(:disabled) {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(102, 126, 234, 0.4);
  }

  &:active:not(:disabled) {
    transform: translateY(0);
  }

  &:disabled {
    opacity: 0.7;
    cursor: not-allowed;

    i {
      animation: spin 0.8s linear infinite;
    }
  }

  .btnLoading {
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
  }
}

.spinner {
  display: inline-block;
  width: 18px;
  height: 18px;
  border: 3px solid rgba(255, 255, 255, 0.3);
  border-top-color: white;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
  flex-shrink: 0;
}

// Animations
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(16px);
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

// Responsive
@media (max-width: 1100px) {
  .formGrid {
    grid-template-columns: 1fr;
  }

  .leftColumn {
    max-width: 600px;
  }

  .formRow {
    grid-template-columns: 1fr;
  }

  .categoriesGrid {
    grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
  }
}

@media (max-width: 768px) {
  .main {
    margin-left: 85px;

    &Collapsed {
      margin-left: 85px;
    }
  }

  .wrapper {
    padding: 20px;
  }

  .pageHeader {
    flex-direction: column;
    gap: 16px;
  }

  .thumbnailActions {
    flex-direction: column;
  }

  .actions {
    flex-direction: column;
  }

  .saveBtn,
  .cancelBtn {
    width: 100%;
    justify-content: center;
  }
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}
</style>
