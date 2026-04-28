<script setup>
import { onMounted, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useRoute, useRouter } from 'vue-router'
import api from '@/api/api'
import { useAuthStore } from '@/stores/auth'
import { usePaymentStore } from '@/stores/payment'

const { t } = useI18n()
const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const id = route.params.id
const slug = route.params.slug
const course = ref({
  id: id,
  name: '',
  listedPrice: '',
  categories: [],
  salePrice: '',
  thumbnailUrl: '',
  instructor: {
    fullName: '',
    avatarUrl: '',
    email: '',
  },
  slug: slug,
  duration: '',
  description: '',
})
const lessons = ref([])
const isSignedIn = ref(false)
const isInWishList = ref(false)
const hasEnrolled = ref(false)

const formatPrice = (price) => {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(price)
}

const calculateDiscount = () => {
  if (course.value.salePrice === null) return 0
  const discount =
    ((course.value.listedPrice - course.value.salePrice) / course.value.listedPrice) * 100
  return Math.round(discount)
}

const handleLogin = () => {
  router.push({
    path: '/login',
    query: {
      redirect: route.fullPath,
    },
    replace: true,
  })
}

const checkLogin = () => {
  if (!auth.user) {
    isSignedIn.value = false
  } else {
    isSignedIn.value = true
  }
}

const checkInWishlist = async() => {
  const itemId = id
  const res = await api.get(`/wishlists/find?itemId=${itemId}`)

  if (!res.data) {
    isInWishList.value = false
  } else {
    isInWishList.value = true
  }
}

const checkEnrollment = async() => {
  const res = await api.get('/enrollments/check', {
    params: {
      itemId: id
    }
  })
  const data = res.data
  if (data === true) {
    hasEnrolled.value = true
  } else {
    hasEnrolled.value = false
  }
}

const handleAddToWishlist = async() => {
  const itemId = id
  try {
    await api.post(`/wishlists/add?itemId=${itemId}`)
  } catch (error) {
    console.log("Error in adding to your wishlist: " + error.response.data);
    isInWishList.value = false
    return
  }

  isInWishList.value = true
}

const removeFromWishlist = async() => {
  const itemId = id
  try {
    await api.delete(`/wishlists/remove?itemId=${itemId}`)
  } catch (error) {
    console.log("Error in removing from your wishlist: " + error.response.data);
    isInWishList.value = false
    return
  }

  isInWishList.value = false
}

const handleBuyCourse = async(courseId) => {
  const payment = usePaymentStore()
  const res = await api.post("/payments/create", {
    itemId: courseId,
    amount: course.value.salePrice ?? course.value.listedPrice
  })
  payment.info = res.data
  payment.info = {
    ...payment.info,
    name: course.value.name,
    id: course.value.id
  }
  router.push({
    path: `/payment/${course.value.slug}/${courseId}`,
  })
}

const getCourse = async () => {
  const res = await api.get(`/courses/${id}`)
  course.value = res.data
}

const getLessons = async () => {
  const res = await api.get(`/lessons/${id}`)
  lessons.value = res.data
}

onMounted(async () => {
  await getCourse()
  getLessons()
  checkLogin()
  checkInWishlist()
  checkEnrollment()
  document.title = `${course.value.name}`
})
</script>

<template>
  <div class="course">
    <div class="container">
      <div class="row g-4">
        <div class="col-lg-8">
          <div class="courseHeader">
            <div class="courseBreadcrumb">
              <RouterLink to="/home" style="color: black"> {{ t('details.home') }} </RouterLink>
              <i class="bi bi-chevron-right"></i>
              <RouterLink to="/public-courses" style="color: black"> {{ t('details.courses') }} </RouterLink>
              <i class="bi bi-chevron-right"></i>
              <span class="courseBreadcrumbCurrent">
                {{ course.name }}
              </span>
            </div>
            <h1 class="courseTitle">
              {{ course.name }}
            </h1>

            <div class="courseCategories">
              <span
                v-for="category in course.categories"
                :key="category.id"
                class="courseCategoriesBadge"
              >
                {{ category.name }}
              </span>
            </div>
          </div>

          <div class="section">
            <h2 class="sectionTitle">
              <i class="bi bi-book-fill"></i>
              {{ t('details.description') }}
            </h2>
            <div class="thumbnail">
              <img :src="course.thumbnailUrl" :alt="course.name" class="thumbnailImg" />
            </div>
            <div class="courseDescription" v-html="course.description"></div>
          </div>

          <div class="section">
            <h2 class="sectionTitle">
              <i class="bi bi-person-fill"></i>
              {{ t('details.instructor') }}
            </h2>
            <div class="instructor">
              <img
                :src="course.instructor.avatarUrl"
                :alt="course.instructor.fullName"
                class="instructorAvatar"
              />
              <div class="instructorInfo">
                <h3 class="instructorName">
                  {{ course.instructor.fullName }}
                </h3>
                <p class="instructorTitle">
                  {{ t('details.expert') }}
                </p>
                <div class="instructorEmail">
                  <i class="bi bi-envelope-fill"></i>
                  <span>{{ course.instructor.email }}</span>
                </div>
              </div>
            </div>
          </div>
          <div class="section">
            <h2 class="sectionTitle">
              <i class="bi bi-list-ul"></i>
              {{ t('details.content') }}
            </h2>
            <div class="contentStats">
              <span>{{ lessons.length }} {{ t('details.lessons') }}</span>
              <span>•</span>
              <span>{{ course.duration }} {{ t('details.minutes') }}</span>
            </div>

            <div class="lessonsList">
              <div v-if="lessons.length === 0" class="noLessons">
                <i class="bi bi-info-circle"></i>
                <span>{{ t('details.noLesson') }}</span>
              </div>

              <div v-for="(lesson, index) in lessons" :key="lesson.id" class="lessonsItem">
                <div class="lessonsLeft">
                  <span class="lessonNumber">{{ index + 1 }}.</span>
                  <i class="bi bi-play-circle-fill"></i>
                  <span class="lessonName">{{ lesson.name }}</span>
                </div>
                <div class="lessonsRight">
                  <RouterLink
                    v-if="lesson.isPreview === true"
                    class="previewBadge"
                    :to="`/${course.slug}/${id}/${lesson.slug}`"
                  >
                    <i class="bi bi-eye-fill"></i>
                    {{ t('details.preview') }}
                  </RouterLink>
                  <span v-else class="lockedBadge">
                    <i class="bi bi-lock-fill"></i>
                    {{ t('details.locked') }}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="col-lg-4">
          <div class="sidebar">
            <div class="price">
              <div class="priceHeader">
                <div class="priceCard">
                  <div v-if="course.salePrice !== null">
                    <span class="priceSale">
                      <span v-if="course.salePrice !== 0">
                        {{ formatPrice(course.salePrice) }}
                      </span>
                      <span v-if="course.salePrice === 0">Free</span>
                    </span>
                    <span class="priceListed">
                      {{ formatPrice(course.listedPrice) }}
                    </span>
                  </div>
                  <div v-else>
                    <span class="priceSale">
                      {{ formatPrice(course.listedPrice) }}
                    </span>
                  </div>
                </div>
                <div v-if="course.salePrice !== null" class="priceDiscount">
                  {{ calculateDiscount() }}% OFF
                </div>
              </div>

              <div v-if="isSignedIn">
                <div v-if="hasEnrolled">
                  <div class="enrolledMessage">
                    <i class="bi bi-check-circle-fill"></i>
                    <span>
                      {{ t('details.hasEnrolled') }}
                    </span>
                  </div>
                  <button class="goToMyCourseBtn" @click="handleGoToMyEnrollments">
                    <i class="bi bi-collection-play-fill"></i>
                    {{ t('details.goTo') }}
                  </button>
                </div>
                <div v-else>
                  <button
                    v-if="course.listedPrice === 0 || course.salePrice === 0"
                    class="enrollBtn"
                    @click="handleGetCourse(course.id)"
                  >
                    <i class="bi bi-cart-plus-fill"></i>
                    {{ t('details.getCourse') }}
                  </button>
                  <button v-else class="enrollBtn" @click="handleBuyCourse(course.id)">
                    <i class="bi bi-cart-plus-fill"></i>
                    {{ t('details.buyCourse') }}
                  </button>
                  <button v-if="isInWishList" class="wishlistBtn" @click="removeFromWishlist">
                    <i class="bi bi-check-square-fill"></i>
                    {{ t('details.inWishlist') }}
                  </button>
                  <button v-else class="wishlistBtn" @click="handleAddToWishlist">
                    <i class="bi bi-heart"></i>
                    {{ t('details.addTo') }}
                  </button>
                </div>
              </div>
              <div v-else>
                <button class="loginToEnrollBtn" @click="handleLogin">
                  <i class="bi bi-box-arrow-in-right"></i>
                  {{ t('details.loginTo') }}
                </button>
              </div>

              <div class="feature">
                <div class="featureItem">
                  <i class="bi bi-infinity"></i>
                  <span>{{ t('details.lifetime') }}</span>
                </div>
                <div class="featureItem">
                  <i class="bi bi-phone"></i>
                  <span>{{ t('details.mobileTV') }}</span>
                </div>
                <div class="featureItem">
                  <i class="bi bi-award"></i>
                  <span>{{ t('details.certificate') }}</span>
                </div>
                <div class="featureItem">
                  <i class="bi bi-arrow-clockwise"></i>
                  <span>{{ t('details.guarantee') }}</span>
                </div>
              </div>
            </div>

            <div class="share">
              <h3>{{ t('details.share') }}</h3>
              <div class="shareWrapper">
                <button class="shareBtn">
                  <i class="bi bi-facebook"></i>
                </button>
                <button class="shareBtn">
                  <i class="bi bi-twitter"></i>
                </button>
                <button class="shareBtn">
                  <i class="bi bi-linkedin"></i>
                </button>
                <button class="shareBtn">
                  <i class="bi bi-link-45deg"></i>
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
a {
  text-decoration: none;
}

.course {
  min-height: 100vh;
  background: #f8f9fa;
  padding: 40px 20px;
  margin-top: 70px;

  &Header {
    background: white;
    padding: 32px;
    border-radius: 16px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
    margin-bottom: 24px;
  }

  &Breadcrumb {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 0.9rem;
    color: #6c757d;
    margin-bottom: 20px;

    i {
      font-size: 0.7rem;
    }

    &Current {
      color: #667eea;
      font-weight: 500;
    }
  }

  &Title {
    font-size: 2.5rem;
    font-weight: 800;
    color: #1e293b;
    margin-bottom: 20px;
    line-height: 1.2;
  }

  &Categories {
    display: flex;
    flex-wrap: wrap;
    gap: 8px;

    &Badge {
      display: inline-block;
      padding: 6px 16px;
      background: linear-gradient(135deg, #667eea15 0%, #764ba215 100%);
      color: #667eea;
      border-radius: 20px;
      font-size: 0.85rem;
      font-weight: 500;
      border: 1px solid #667eea30;
    }
  }

  &Description {
    color: #475569;
    line-height: 1.8;
    font-size: 1rem;

    p {
      margin-bottom: 16px;

      &:last-child {
        margin-bottom: 0;
      }
    }
  }
}

// Section Card
.section {
  background: white;
  padding: 32px;
  border-radius: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
  margin-bottom: 24px;

  &Title {
    font-size: 1.5rem;
    font-weight: 700;
    color: #1e293b;
    margin-bottom: 24px;
    display: flex;
    align-items: center;
    gap: 12px;

    i {
      color: #667eea;
      font-size: 1.3rem;
    }
  }
}

// Description
.thumbnail {
  position: relative;
  width: 100%;
  margin-bottom: 24px;
  border-radius: 12px;
  overflow: hidden;

  &Img {
    width: 100%;
    height: auto;
    display: block;
    aspect-ratio: 16 / 9;
    object-fit: cover;
  }
}

// Instructor
.instructor {
  display: flex;
  gap: 20px;
  padding: 24px;
  background: #f8fafc;
  border-radius: 12px;

  &Avatar {
    width: 100px;
    height: 100px;
    border-radius: 50%;
    object-fit: cover;
    border: 4px solid white;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
    flex-shrink: 0;
  }

  &Info {
    flex: 1;
  }

  &Name {
    font-size: 1.4rem;
    font-weight: 700;
    color: #1e293b;
    margin-bottom: 4px;
  }

  &Title {
    color: #64748b;
    font-size: 0.95rem;
    margin-bottom: 16px;
  }

  &Email {
    display: flex;
    align-items: center;
    gap: 10px;
    color: #475569;
    font-size: 0.95rem;

    i {
      color: #667eea;
      font-size: 1.1rem;
    }

    span {
      color: #667eea;
      text-decoration: none;
      transition: all 0.2s;
    }
  }
}

.stat {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #475569;
  font-size: 0.9rem;

  i {
    color: #f59e0b;
    font-size: 1rem;
  }
}

// Course Content
.contentStats {
  display: flex;
  flex-wrap: wrap;
  gap: 12px;
  padding: 16px 20px;
  background: #f8fafc;
  border-radius: 8px;
  margin-bottom: 20px;
  color: #475569;
  font-size: 0.95rem;
  font-weight: 500;
}

.chevronIcon {
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.lessonsList {
  background: #fff;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  overflow: hidden;
}

.lessonsItem {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 24px;
  border-bottom: 1px solid #f1f5f9;
  transition: all 0.25s;

  &:last-child {
    border-bottom: none;
  }

  &:hover {
    background: #f8fafc;
  }
}

.lessonsLeft {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;

  .lessonNumber {
    font-weight: 700;
    color: #667eea;
    min-width: 28px;
    font-size: 0.95rem;
  }

  i {
    font-size: 1.2rem;
    color: #94a3b8;
  }

  .lessonName {
    font-size: 0.95rem;
    color: #334155;
    font-weight: 500;
  }
}

.lessonsRight {
  display: flex;
  align-items: center;
  gap: 8px;
}

.previewBadge {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 14px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: #fff;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;
  text-decoration: none;
  transition: all 0.25s;
  box-shadow: 0 2px 8px rgba(102, 126, 234, 0.25);

  i {
    font-size: 0.9rem;
  }

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(102, 126, 234, 0.35);
    color: #fff;
  }
}

.lockedBadge {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 14px;
  background: #f1f5f9;
  color: #64748b;
  border: 1px solid #e2e8f0;
  border-radius: 20px;
  font-size: 0.8rem;
  font-weight: 600;

  i {
    font-size: 0.85rem;
  }
}

.noLessons {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 40px 24px;
  color: #94a3b8;
  font-size: 0.95rem;

  i {
    font-size: 1.5rem;
  }
}

// Content Stats (adjust if needed)
.contentStats {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 20px;
  font-size: 0.9rem;
  color: #64748b;
  font-weight: 500;

  span {
    &:not(:last-child):after {
      content: '';
    }
  }
}

@keyframes slideIn {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@for $i from 1 through 20 {
  .lessonItem:nth-child(#{$i}) {
    animation-delay: #{$i * 0.05}s;
  }
}

.previewBadge {
  padding: 4px 12px;
  background: #10b98120;
  color: #059669;
  border-radius: 12px;
  font-size: 0.8rem;
  font-weight: 600;
}

// Sidebar
.sidebar {
  position: sticky;
  top: 20px;
}

.price {
  background: white;
  padding: 28px;
  border-radius: 16px;
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.1);
  margin-bottom: 20px;

  &Header {
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 24px;
  }

  &Card {
    display: flex;
    flex-direction: column;
    gap: 4px;
  }

  &Sale {
    font-size: 2.5rem;
    font-weight: 800;
    color: #1e293b;
    line-height: 1;

    // Style for "Free" text when salePrice === 0
    span {
      background: linear-gradient(135deg, #10b981 0%, #059669 100%);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
      background-clip: text;
      font-size: 3rem;
      font-weight: 900;
      letter-spacing: 1px;
      text-transform: uppercase;
      display: inline-block;
      animation: shimmer 2s ease-in-out infinite;
    }
  }

  &Listed {
    font-size: 1.1rem;
    color: #94a3b8;
    text-decoration: line-through;
  }

  &Discount {
    padding: 8px 16px;
    background: linear-gradient(135deg, #ef4444 0%, #dc2626 100%);
    color: white;
    border-radius: 20px;
    font-weight: 700;
    font-size: 0.9rem;
  }
}

@keyframes shimmer {
  0%,
  100% {
    filter: brightness(1);
  }
  50% {
    filter: brightness(1.2);
  }
}

.enrollBtn {
  width: 100%;
  padding: 16px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 12px;
  font-weight: 700;
  font-size: 1.1rem;
  cursor: pointer;
  transition: all 0.3s;
  margin-bottom: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
  }

  i {
    font-size: 1.3rem;
  }
}

.wishlistBtn {
  width: 100%;
  padding: 14px;
  background: white;
  color: #667eea;
  border: 2px solid #667eea;
  border-radius: 12px;
  font-weight: 600;
  font-size: 1rem;
  cursor: pointer;
  transition: all 0.3s;
  margin-bottom: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;

  &:hover {
    background: #667eea;
    color: white;
  }

  i {
    font-size: 1.2rem;
  }
}

.loginToEnrollBtn {
  width: 100%;
  padding: 16px;
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  color: white;
  border: none;
  border-radius: 12px;
  font-weight: 700;
  font-size: 1.1rem;
  cursor: pointer;
  transition: all 0.3s;
  margin-bottom: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(102, 126, 234, 0.4);
  }

  i {
    font-size: 1.3rem;
  }
}

.feature {
  border-top: 1px solid #e2e8f0;
  padding-top: 20px;

  &Item {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 12px 0;
    color: #475569;
    font-size: 0.95rem;

    i {
      color: #667eea;
      font-size: 1.2rem;
    }
  }
}

// Share Card
.share {
  background: white;
  padding: 24px;
  border-radius: 16px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);

  h3 {
    font-size: 1.1rem;
    font-weight: 700;
    color: #1e293b;
    margin-bottom: 16px;
  }

  &Wrapper {
    display: flex;
    gap: 12px;
  }

  &Btn {
    flex: 1;
    padding: 12px;
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 8px;
    color: #64748b;
    cursor: pointer;
    transition: all 0.3s;
    display: flex;
    align-items: center;
    justify-content: center;

    &:hover {
      background: #667eea;
      color: white;
      border-color: #667eea;
    }

    i {
      font-size: 1.3rem;
    }
  }
}

.enrolledMessage {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding: 20px;
  background: linear-gradient(135deg, #d1fae5 0%, #a7f3d0 100%);
  border: 2px solid #10b981;
  border-radius: 12px;
  margin-bottom: 16px;
  animation: slideInDown 0.5s ease;

  i {
    font-size: 1.8rem;
    color: #059669;
  }

  span {
    color: #065f46;
    font-weight: 700;
    font-size: 1rem;
    text-align: center;
    line-height: 1.4;
  }
}

.goToMyCourseBtn {
  width: 100%;
  padding: 16px;
  background: linear-gradient(135deg, #10b981 0%, #059669 100%);
  color: white;
  border: none;
  border-radius: 12px;
  font-weight: 700;
  font-size: 1.1rem;
  cursor: pointer;
  transition: all 0.3s;
  margin-bottom: 24px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 20px rgba(16, 185, 129, 0.4);
  }

  &:active {
    transform: translateY(0);
  }

  i {
    font-size: 1.3rem;
  }
}

@keyframes slideInDown {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

// Responsive
@media (max-width: 992px) {
  .sidebar {
    position: static;
  }
}

@media (max-width: 768px) {
  .courseTitle {
    font-size: 1.8rem;
  }

  .instructor {
    flex-direction: column;
    text-align: center;
  }

  .instructorAvatar {
    margin: 0 auto;
  }

  .priceSale {
    font-size: 2rem;

    span {
      font-size: 2.4rem;
    }
  }

  .lessonsItem {
    padding-left: 48px;
  }

  .shareWrapper {
    flex-wrap: wrap;
  }

  .shareBtn {
    flex: 0 0 calc(25% - 9px);
  }
}
</style>
